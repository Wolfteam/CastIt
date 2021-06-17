using CastIt.Domain.Dtos.Requests;
using CastIt.Domain.Dtos.Responses;
using CastIt.Domain.Enums;
using CastIt.Domain.Interfaces;
using CastIt.Infrastructure.Models;
using CastIt.Server.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CastIt.Server.Hubs
{
    //Task OnConnectedAsync();
    //Task GoToSeconds(GoToSecondsRequestDto dto);
    //Task SkipSeconds(GoToSecondsRequestDto dto);
    //Task GoTo(GoToRequestDto dto);
    //Task TogglePlayBack();
    //Task StopPlayBack();
    //Task SetPlayListOptions(SetPlayListOptionsRequestDto dto);
    //Task DeletePlayList(long id);
    //Task DeleteFile(long id, long playlistId);
    //Task LoopFile(SetLoopFileRequestDto dto);
    //Task SetFileOptions(SetFileOptionsRequestDto dto);
    //Task UpdateSettings(AppSettingsRequestDto dto);
    //Task SetVolume(SetVolumeRequestDto dto);
    //Task RenamePlayList(RenamePlayListRequestDto dto);
    //Task CloseApp();
    //Task SendPlayLists();
    //Task GetPlayList(long playlistId);
    //Task ClientConnected();
    //void FileLoading();
    //Task SendCurrentFileLoaded();
    //void FileLoadingError(string error);
    //void EndReached();
    //void TimeChanged(double seconds);
    //void Paused();
    //void ChromeCastDisconnected();
    //void VolumeLevelChanged(double newLevel, bool isMuted);
    //void AppClosing();
    //Task RefreshPlayList(long id, bool wasDeleted = false);
    //void SendInfoMessage(string msg);
    //Task SendMsg(string msgType, bool succeed = true);
    //Task SendMsg<T>(T result, string msgType, bool succeed = true) where T : class;

    //The exposed name methods here must match the ones that the client listens for
    public interface ICastItHub
    {
        Task SendPlayLists(List<GetAllPlayListResponseDto> playLists);

        Task StoppedPlayBack();

        Task PlayListAdded(GetAllPlayListResponseDto playList);

        Task PlayListChanged(GetAllPlayListResponseDto playList);

        Task PlayListDeleted(long id);

        Task PlayListIsBusy(long id, bool isBusy);

        Task FileAdded(FileItemResponseDto file);

        Task FileChanged(FileItemResponseDto file);

        Task FileDeleted(long playListId, long id);

        Task FileLoading(FileItemResponseDto file);

        Task FileLoaded(FileItemResponseDto file);

        Task FileEndReached(FileItemResponseDto file);

        //Task FileStoppedPlayback(FileItemResponseDto file);

        //Task CurrentPlayedFileStatusChanged(ServerFileItem file);

        Task PlayerStatusChanged(ServerPlayerStatusResponseDto status);

        Task PlayerSettingsChanged(ServerAppSettings settings);

        Task ServerMessage(AppMessageType type);

        Task CastDeviceSet(IReceiver device);

        Task CastDevicesChanged(List<IReceiver> devices);

        Task CastDeviceDisconnected();
    }

    //The exposed name methods here must match the ones that the client can call
    public class CastItHub : Hub<ICastItHub>
    {
        private readonly ILogger<CastItHub> _logger;
        private readonly IServerCastService _castService;
        private readonly IServerAppSettingsService _settingsService;

        public CastItHub(
            ILogger<CastItHub> logger,
            IServerCastService castService,
            IServerAppSettingsService settingsService)
        {
            _logger = logger;
            _castService = castService;
            _settingsService = settingsService;
        }

        public override async Task OnConnectedAsync()
        {
            await SendSettingsChanged();
            //await SendClientConnected().ConfigureAwait(false);
            if (_castService.IsPlayingOrPaused)
            {
                await SendPlayerStatusChanged();
            }
            await SendPlayListsToClient();
            await SendCastDevicesChanged();
        }

        #region Client Msgs
        public Task Play(PlayFileRequestDto dto)
        {
            return _castService.PlayFile(dto.Id, dto.Force, dto.FileOptionsChanged);
        }

        public Task GoToSeconds(double seconds)
        {
            return _castService.GoToSeconds(seconds);
        }

        public Task SkipSeconds(double seconds)
        {
            return _castService.AddSeconds(seconds);
        }

        public Task GoTo(bool next, bool previous)
        {
            if (next)
                return _castService.GoTo(true);

            if (previous)
                return _castService.GoTo(false);

            return Task.CompletedTask;
        }

        public Task TogglePlayBack()
        {
            return _castService.TogglePlayback();
        }

        public Task StopPlayback()
        {
            return _castService.StopPlayback();
        }

        public Task DeleteFile(long playlistId, long id)
        {
            return _castService.RemoveFiles(playlistId, id);
        }

        public Task LoopFile(SetLoopFileRequestDto dto)
        {
            _castService.LoopFile(dto.Id, dto.PlayListId, dto.Loop);
            return Task.CompletedTask;
        }

        public Task SetFileOptions(SetFileOptionsRequestDto dto)
        {
            return _castService.SetCurrentPlayedFileOptions(dto.StreamIndex, dto.IsAudio, dto.IsSubTitle, dto.IsQuality);
        }

        public async Task UpdateSettings(ServerAppSettings settings)
        {
            await _settingsService.UpdateSettings(settings);
            await SendSettingsChanged();
        }

        public async Task SetVolume(SetVolumeRequestDto dto)
        {
            await _castService.SetVolume(dto.VolumeLevel);
            await _castService.SetIsMuted(dto.IsMuted);
        }

        public Task<PlayListItemResponseDto> AddNewPlayList()
        {
            return _castService.AddNewPlayList();
        }

        public Task<PlayListItemResponseDto> GetPlayList(long id)
        {
            var playList = _castService.GetPlayList(id);
            return Task.FromResult(playList);
        }

        public Task UpdatePlayList(UpdatePlayListRequestDto dto)
        {
            return _castService.UpdatePlayList(dto.Id, dto.Name, dto.Position);
        }

        public Task SetPlayListOptions(SetPlayListOptionsRequestDto dto)
        {
            _castService.SetPlayListOptions(dto.Id, dto.Loop, dto.Shuffle);
            return Task.CompletedTask;
        }

        public Task DeletePlayList(long id)
        {
            return _castService.DeletePlayList(id);
        }

        public Task DeleteAllPlayLists(long exceptId = -1)
        {
            return _castService.DeleteAllPlayLists(exceptId);
        }

        public Task RemoveFiles(long playListId, List<long> ids)
        {
            return _castService.RemoveFiles(playListId, ids.ToArray());
        }

        public Task RemoveFilesThatStartsWith(long playListId, string path)
        {
            return _castService.RemoveFilesThatStartsWith(playListId, path);
        }

        public Task RemoveAllMissingFiles(long playListId)
        {
            return _castService.RemoveAllMissingFiles(playListId);
        }

        public Task AddFolders(long playListId, AddFolderOrFilesToPlayListRequestDto dto)
        {
            return _castService.AddFolder(playListId, dto.IncludeSubFolders, dto.Folders.ToArray());
        }

        public Task AddFiles(long playListId, AddFolderOrFilesToPlayListRequestDto dto)
        {
            return _castService.AddFiles(playListId, dto.Files.ToArray());
        }

        public Task AddUrlFile(long playListId, AddUrlToPlayListRequestDto dto)
        {
            return _castService.AddUrl(playListId, dto.Url, dto.OnlyVideo);
        }

        public Task SetFileSubtitlesFromPath(string filePath)
        {
            return _castService.SetFileSubtitlesFromPath(filePath);
        }

        public Task ConnectToCastDevice(string id)
        {
            return _castService.SetCastRenderer(id);
        }

        public Task RefreshCastDevices(TimeSpan ts)
        {
            return _castService.RefreshCastDevices(ts);
        }

        public Task CloseApp()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Server Msgs
        public async Task SendPlayListsToClient()
        {
            var playLists = await _castService.GetAllPlayLists();
            await Clients.Caller.SendPlayLists(playLists);
        }

        //public async Task SendClientConnected()
        //{
        //    await SendMsgToClients(ClientConnectedMsgType).ConfigureAwait(false);
        //}

        public Task SendFileLoading(FileItemResponseDto file)
        {
            return Clients.All.FileLoading(file);
        }

        public Task SendFileLoaded(FileItemResponseDto file)
        {
            return Clients.All.FileLoaded(file);
        }

        public Task SendPlayerStatusChanged()
        {
            var status = _castService.GetPlayerStatus();
            return Clients.All.PlayerStatusChanged(status);
        }

        public Task SendEndReached(FileItemResponseDto file)
        {
            return Clients.All.FileEndReached(file);
        }

        public Task SendCastDevicesChanged()
        {
            var devices = _castService.AvailableDevices;
            return Clients.All.CastDevicesChanged(devices);
        }

        //public Task SendCurrentPlayedFileStatusChanged(ServerFileItem file)
        //{
        //    return Clients.All.CurrentPlayedFileStatusChanged(file);
        //}

        //public async void ChromeCastDisconnected()
        //{
        //    await SendMsgToClients(ChromeCastDisconnectedMsgType).ConfigureAwait(false);
        //}

        public Task SendSettingsChanged()
        {
            var settings = _settingsService.Settings;
            return Clients.All.PlayerSettingsChanged(settings);
        }

        //public async void PlayListChanged(long id)
        //{
        //    await SendPlayLists().ConfigureAwait(false);
        //    await SendPlayList(id).ConfigureAwait(false);
        //    if (_view.IsCurrentlyPlaying)
        //    {
        //        await FileLoadedTask().ConfigureAwait(false);
        //    }
        //}

        //public async void PlayListDeleted(long id)
        //{
        //    await SendPlayLists().ConfigureAwait(false);
        //    await RefreshPlayList(id, true).ConfigureAwait(false);
        //    if (_view.IsCurrentlyPlaying)
        //    {
        //        await FileLoadedTask().ConfigureAwait(false);
        //    }
        //}

        //public async void FileAdded(long onPlayListId)
        //{
        //    await SendPlayLists().ConfigureAwait(false);
        //    await SendPlayList(onPlayListId).ConfigureAwait(false);
        //}

        //public async void FileChanged(long onPlayListId)
        //{
        //    await SendPlayList(onPlayListId).ConfigureAwait(false);
        //    if (_view.IsCurrentlyPlaying)
        //    {
        //        await FileLoadedTask().ConfigureAwait(false);
        //    }
        //}

        //public async void FilesDeleted(long onPlayListId)
        //{
        //    await SendPlayLists().ConfigureAwait(false);
        //    await SendPlayList(onPlayListId).ConfigureAwait(false);
        //    if (_view.IsCurrentlyPlaying)
        //    {
        //        await FileLoadedTask().ConfigureAwait(false);
        //    }
        //}

        //public async Task SendFileOptions(long id)
        //{
        //    var options = _view.GetFileOptions(id);
        //    await SendMsg(options, SendFileOptionsMsgType).ConfigureAwait(false);
        //    VolumeLevelChanged(_view.VolumeLevel, _view.IsMuted);
        //}

        public Task SendServerMessage(AppMessageType type)
        {
            return Clients.All.ServerMessage(type);
        }
        #endregion
    }
}
