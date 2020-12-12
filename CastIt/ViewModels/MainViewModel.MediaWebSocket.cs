using CastIt.Domain.Dtos.Responses;
using CastIt.Domain.Enums;
using CastIt.Models.Messages;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CastIt.ViewModels
{
    public partial class MainViewModel
    {
        public Task CloseAppFromMediaWebSocket()
        {
            return CloseAppCommand.ExecuteAsync();
        }

        public Task GoToSecondsFromMediaWebSocket(long seconds)
        {
            return GoToSecondsCommand.ExecuteAsync(seconds);
        }

        public void GoToNextFromMediaWebSocket()
        {
            NextCommand.Execute();
        }

        public void GoToPreviousFromMediaWebSocket()
        {
            PreviousCommand.Execute();
        }

        public Task SetVolumeFromMediaWebSocket()
        {
            return SetVolumeCommand.ExecuteAsync();
        }

        public Task SkipFromMediaWebSocket(int seconds)
        {
            return SkipCommand.ExecuteAsync(seconds);
        }

        public Task StopPlayBackFromMediaWebSocket()
        {
            return StopPlayBackCommand.ExecuteAsync();
        }

        public Task ToggleMuteFromMediaWebSocket()
        {
            return ToggleMuteCommand.ExecuteAsync();
        }

        public Task TogglePlayBackFromMediaWebSocket()
        {
            return TogglePlayBackCommand.ExecuteAsync();
        }

        public List<GetAllPlayListResponseDto> GetAllPlayListsForMediaWebSocket()
        {
            return PlayLists.Select(pl => new GetAllPlayListResponseDto
            {
                Id = pl.Id,
                Loop = pl.Loop,
                Name = pl.Name,
                NumberOfFiles = pl.Items.Count,
                Position = pl.Position,
                Shuffle = pl.Shuffle,
                TotalDuration = pl.TotalDuration
            }).ToList();
        }

        public PlayListItemResponseDto GetPlayListForMediaWebSocket(long playlistId)
        {
            var playlist = PlayLists.FirstOrDefault(pl => pl.Id == playlistId);
            if (playlist == null)
                return null;

            return new PlayListItemResponseDto
            {
                Id = playlist.Id,
                Loop = playlist.Loop,
                Name = playlist.Name,
                NumberOfFiles = playlist.Items.Count,
                Position = playlist.Position,
                Shuffle = playlist.Shuffle,
                TotalDuration = playlist.TotalDuration,
                Files = _mapper.Map<List<FileItemResponseDto>>(playlist.Items)
            };
        }

        public FileLoadedResponseDto GetCurrentFileLoadedForMediaWebSocket()
        {
            if (_currentlyPlayedFile == null)
                return null;

            var response = new FileLoadedResponseDto
            {
                Id = _currentlyPlayedFile.Id,
                Duration = _currentlyPlayedFile.TotalSeconds,
                Filename = _currentlyPlayedFile.Filename,
                LoopFile = _currentlyPlayedFile.Loop,
                CurrentSeconds = CurrentPlayedSeconds,
                IsPaused = IsPaused,
                IsMuted = IsMuted,
                VolumeLevel = VolumeLevel,
                ThumbnailUrl = CurrentFileThumbnail
            };

            var playlist = PlayLists.FirstOrDefault(pl => pl.Id == _currentlyPlayedFile.PlayListId);
            response.PlayListId = playlist?.Id ?? 0;
            response.PlayListName = playlist?.Name ?? "N/A";
            response.LoopPlayList = playlist?.Loop ?? false;
            response.ShufflePlayList = playlist?.Shuffle ?? false;
            return response;
        }

        public Task PlayFileForMediaWebSocket(long id, long playlistId, bool force)
        {
            var playList = PlayLists.FirstOrDefault(pl => pl.Id == playlistId);
            if (playList == null)
            {
                Logger.LogWarning($"{nameof(PlayFileForMediaWebSocket)}: Couldnt play fileId = {id} because playlistId = {playlistId} doesnt exists");
                _appWebServer.OnServerMsg?.Invoke(GetText("PlayListDoesntExist"));
                return Task.CompletedTask;
            }

            var file = playList.Items.FirstOrDefault(f => f.Id == id);
            if (file != null)
                return PlayFile(file, force);
            Logger.LogWarning($"{nameof(PlayFileForMediaWebSocket)}: Couldnt play fileId = {id} because it doesnt exists");
            _appWebServer.OnServerMsg?.Invoke(GetText("FileDoesntExist"));
            return Task.CompletedTask;
        }

        public void SetPlayListOptions(long id, bool loop, bool shuffle)
        {
            var playlist = PlayLists.FirstOrDefault(pl => pl.Id == id);
            if (playlist == null)
            {
                Logger.LogWarning($"{nameof(SetPlayListOptions)}: PlaylistId = {id} doesnt exists");
                _appWebServer.OnServerMsg?.Invoke(GetText("PlayListDoesntExist"));
                return;
            }

            playlist.Loop = loop;
            playlist.Shuffle = shuffle;
        }

        public Task DeletePlayList(long id)
        {
            var playlist = PlayLists.FirstOrDefault(pl => pl.Id == id);
            if (playlist != null)
            {
                _beforeDeletingPlayList.Raise(playlist);
                return Task.CompletedTask;
            }
            Logger.LogWarning($"{nameof(DeletePlayList)}: Cant delete playlistId = {id} because it doesnt exists");
            return ShowSnackbarMsg(GetText("PlayListDoesntExist"));
        }

        public Task DeleteFile(long id, long playListId)
        {
            var playList = PlayLists.FirstOrDefault(pl => pl.Id == playListId);
            if (playList != null)
                return playList.RemoveFile(id);
            Logger.LogWarning($"{nameof(DeleteFile)}: Couldnt delete fileId = {id} because playlistId = {playListId} doesnt exists");
            return ShowSnackbarMsg(GetText("PlayListDoesntExist"));
        }

        public Task SetFileLoop(long id, long playlistId, bool loop)
        {
            var pl = PlayLists.FirstOrDefault(pl => pl.Id == playlistId);
            if (pl == null)
            {
                Logger.LogWarning($"{nameof(SetFileLoop)}: Couldnt update fileId = {id} because playlistId = {playlistId} doesnt exists");
                return ShowSnackbarMsg(GetText("PlayListDoesntExist"));
            }

            var file = pl.Items.FirstOrDefault(f => f.Id == id);
            if (file == null)
            {
                Logger.LogWarning($"{nameof(SetFileLoop)}: Couldnt update fileId = {id} because it doesnt exists");
                return ShowSnackbarMsg(GetText("FileDoesntExist"));
            }

            file.Loop = loop;
            return Task.CompletedTask;
        }

        public List<FileItemOptionsResponseDto> GetFileOptions(long id)
        {
            var fileOptions = new List<FileItemOptionsResponseDto>();
            if (_currentlyPlayedFile == null || _currentlyPlayedFile.Id != id)
                return fileOptions;

            fileOptions.AddRange(_mapper.Map<List<FileItemOptionsResponseDto>>(CurrentFileAudios));
            fileOptions.AddRange(_mapper.Map<List<FileItemOptionsResponseDto>>(CurrentFileQualities));
            fileOptions.AddRange(_mapper.Map<List<FileItemOptionsResponseDto>>(CurrentFileSubTitles));
            fileOptions.AddRange(_mapper.Map<List<FileItemOptionsResponseDto>>(CurrentFileVideos));
            return fileOptions;
        }

        public Task SetFileOptions(int streamIndex, bool isAudio, bool isSubtitle, bool isQuality)
        {
            if (!isAudio && !isSubtitle && !isQuality)
                return Task.CompletedTask;

            if (_currentlyPlayedFile == null)
                return Task.CompletedTask;

            var options = isAudio
                ? CurrentFileAudios.FirstOrDefault(a => a.Id == streamIndex)
                : isSubtitle
                    ? CurrentFileSubTitles.FirstOrDefault(s => s.Id == streamIndex)
                    : CurrentFileQualities.FirstOrDefault(q => q.Id == streamIndex);
            return FileOptionsChanged(options);
        }

        public void UpdateSettings(
            bool startFilesFromTheStart,
            bool playNextFileAutomatically,
            bool forceVideoTranscode,
            bool forceAudioTranscode,
            VideoScaleType videoScale,
            bool enableHardwareAcceleration)
        {
            Messenger.Publish(new SettingsExternallyUpdatedMessage(
                this,
                startFilesFromTheStart,
                playNextFileAutomatically,
                forceVideoTranscode,
                forceAudioTranscode,
                videoScale,
                enableHardwareAcceleration));
            //TODO: IMPROVE THIS. The settings subscription sometimes gets lost... thats why i do this
            _settingsService.StartFilesFromTheStart = startFilesFromTheStart;
            _settingsService.PlayNextFileAutomatically = playNextFileAutomatically;
            _settingsService.ForceAudioTranscode = forceAudioTranscode;
            _settingsService.ForceVideoTranscode = forceVideoTranscode;
            _settingsService.VideoScale = videoScale;
            _settingsService.EnableHardwareAcceleration = enableHardwareAcceleration;
            _appWebServer.OnAppSettingsChanged?.Invoke();
        }

        public Task RenamePlayList(long id, string newName)
        {
            var playlist = PlayLists.FirstOrDefault(pl => pl.Id == id);
            if (playlist != null)
                return playlist.SavePlayList(newName);
            Logger.LogWarning($"{nameof(RenamePlayList)}: Cant rename playlistId = {id} because it doesnt exists");
            return ShowSnackbarMsg(GetText("PlayListDoesntExist"));
        }

        public AppSettingsResponseDto GetCurrentAppSettings()
        {
            return new AppSettingsResponseDto
            {
                ForceVideoTranscode = _settingsService.ForceVideoTranscode,
                ForceAudioTranscode = _settingsService.ForceAudioTranscode,
                EnableHardwareAcceleration = _settingsService.EnableHardwareAcceleration,
                PlayNextFileAutomatically = _settingsService.PlayNextFileAutomatically,
                StartFilesFromTheStart = _settingsService.StartFilesFromTheStart,
                VideoScale = _settingsService.VideoScale
            };
        }
    }
}
