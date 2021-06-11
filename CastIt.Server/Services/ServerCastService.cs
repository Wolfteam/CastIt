using AutoMapper;
using CastIt.Application.Common;
using CastIt.Application.Common.Extensions;
using CastIt.Application.Common.Utils;
using CastIt.Application.Interfaces;
using CastIt.Domain.Dtos.Responses;
using CastIt.Domain.Entities;
using CastIt.Domain.Enums;
using CastIt.Domain.Exceptions;
using CastIt.Domain.Extensions;
using CastIt.Domain.Models.FFmpeg.Info;
using CastIt.GoogleCast.Interfaces;
using CastIt.Infrastructure.Models;
using CastIt.Server.Common;
using CastIt.Server.Common.Comparers;
using CastIt.Server.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CastIt.Server.Services
{
    public interface INewCastService
    {
        Task PlayFile(FileItem file, bool force = false);
        Task PlayFile(long id, long playListId, bool force, bool fileOptionsChanged);
        Task PlayFile(FileItem file, bool force, bool fileOptionsChanged);
        Task GoTo(bool nextTrack, bool isAnAutomaticCall = false);
        Task UpdatePlayList(long playListId, string newName, int position);
        Task RemoveFilesThatStartsWith(long playListId, string path);
        Task RemoveAllMissingFiles(long playListId);
        Task RemoveFiles(long playListId, params long[] ids);
        void SortFiles(long playListId, SortModeType sortBy);
        void SetPositionIfChanged(long playListId);
        Task AddFolder(long playListId, string[] folders);
        Task AddFiles(long playListId, string[] paths);
        Task AddUrl(long playListId, string url, bool onlyVideo);
        FileLoadedResponseDto GetCurrentFileLoaded();
        void SetPlayListOptions(long id, bool loop, bool shuffle);
        void DisableLoopForAllFiles(long exceptFileId = -1);
        PlayListItemResponseDto GetPlayList(long playListId);
    }

    public delegate void OnFilesAdded(long playListId, params FileItem[] files);

    public class ServerCastService : BaseCastService, IServerCastService
    {
        #region Members
        private readonly IAppDataService _appDataService;
        private readonly IMapper _mapper;

        private bool _onSkipOrPrevious;
        public ServerFileItem CurrentPlayedFile { get; private set; }

        public CancellationTokenSource FileCancellationTokenSource { get; } = new CancellationTokenSource();
        #endregion

        #region Delegates
        public OnFilesAdded OnFilesAdded { get; set; }
        #endregion

        #region Properties
        public List<ServerPlayList> PlayLists { get; } = new List<ServerPlayList>();
        //TODO: REMOVE THIS DTOS AND USE A SERVER CLASS
        public List<FileItemOptionsResponseDto> CurrentFileVideos { get; }
            = new List<FileItemOptionsResponseDto>();
        public List<FileItemOptionsResponseDto> CurrentFileAudios { get; }
            = new List<FileItemOptionsResponseDto>();
        public List<FileItemOptionsResponseDto> CurrentFileSubTitles { get; }
            = new List<FileItemOptionsResponseDto>();
        public List<FileItemOptionsResponseDto> CurrentFileQualities { get; }
            = new List<FileItemOptionsResponseDto>();

        public int CurrentFileVideoStreamIndex
            => CurrentFileVideos.Find(f => f.IsSelected)?.Id ?? ServerConstants.DefaultSelectedStreamId;
        public int CurrentFileAudioStreamIndex
            => CurrentFileAudios.Find(f => f.IsSelected)?.Id ?? ServerConstants.DefaultSelectedStreamId;
        public int CurrentFileSubTitleStreamIndex
            => CurrentFileSubTitles.Find(f => f.IsSelected)?.Id ?? ServerConstants.NoStreamSelectedId;
        public int CurrentFileQuality
            => CurrentFileQualities.Find(f => f.IsSelected)?.Id ?? ServerConstants.DefaultQualitySelected;
        #endregion

        public ServerCastService(
            ILogger<ServerCastService> logger,
            IBaseWebServer appWebServer,
            IFFmpegService ffmpegService,
            IYoutubeUrlDecoder youtubeUrlDecoder,
            ITelemetryService telemetryService,
            IServerAppSettingsService appSettings,
            IFileService fileService,
            IPlayer player,
            IAppDataService appDataService,
            IMapper mapper)
            : base(logger, appWebServer, ffmpegService, youtubeUrlDecoder, telemetryService, appSettings, fileService, player)
        {
            _appDataService = appDataService;
            _mapper = mapper;
        }

        public override async Task Init()
        {
            var mappedPlayLists = new List<ServerPlayList>();
            var playLists = await _appDataService.GetAllPlayLists();
            foreach (var playlist in playLists)
            {
                var files = await _appDataService.GetAllFiles(playlist.Id);
                var mapped = _mapper.Map<ServerPlayList>(playlist);
                mapped.Files = files
                    .Select(f => ServerFileItem.From(FileService, f))
                    .OrderBy(f => f.Position)
                    .ToList();
                mappedPlayLists.Add(mapped);
            }

            PlayLists.AddRange(mappedPlayLists.OrderBy(pl => pl.Position));
            await base.Init();
        }

        public override Task CleanThemAll()
        {
            FileCancellationTokenSource.Cancel();
            return base.CleanThemAll();
        }

        public Task PlayFile(ServerFileItem file, bool force = false)
        {
            return PlayFile(file, force, false);
        }

        public async Task PlayFile(long id, bool force, bool fileOptionsChanged)
        {
            var file = PlayLists.SelectMany(pl => pl.Files).FirstOrDefault(f => f.Id == id);
            //TODO: THROW NOT FOUND EX
            await PlayFile(file, force, fileOptionsChanged);
        }

        public async Task PlayFile(ServerFileItem file, bool force, bool fileOptionsChanged)
        {
            if (file == null)
            {
                Logger.LogWarning("The provided file won't be played cause it is null");
                OnServerMessage?.Invoke(AppMessageType.FileNotFound);
                throw new Domain.Exceptions.FileNotFoundException("The provided file won't be played cause it is null");
            }
            //TODO: REMOVE ALL THE ONSERVERMESSAGE AND HANDLE THOSE IN THE MIDDLEWARE
            var type = FileService.GetFileType(file.Path);

            if (type.DoesNotExist())
            {
                Logger.LogWarning($"The provided file = {file.Path} does not exist");
                OnServerMessage?.Invoke(AppMessageType.FileNotFound);
                throw new Domain.Exceptions.FileNotFoundException($"The provided file = {file.Path} does not exist");
            }
            //TODO: if (file == _currentlyPlayedFile && !force && !fileOptionsChanged && !file.Loop)
            bool fileIsBeingPlayed = file.Id == CurrentPlayedFile?.Id && !force && !fileOptionsChanged;
            if (fileIsBeingPlayed)
            {
                Logger.LogInformation($"The provided file = {file.Path} is already being played");
                OnServerMessage?.Invoke(AppMessageType.FileIsAlreadyBeingPlayed);
                throw new InvalidRequestException($"The provided file = {file.Path} is already being played", AppMessageType.FileIsAlreadyBeingPlayed);
            }

            if (string.IsNullOrEmpty(file.Duration))
            {
                Logger.LogInformation(
                    $"{nameof(PlayFile)}: Cant play file = {file.Filename} yet, because im still setting the duration for some files.");
                throw new InvalidRequestException($"The provided file = {file.Path} is already being played", AppMessageType.OneOrMoreFilesAreNotReadyYet);
            }

            if (!AvailableDevices.Any())
            {
                Logger.LogInformation($"File = {file.Path} won't be played cause there are any devices available");
                OnServerMessage?.Invoke(AppMessageType.NoDevicesFound);
                throw new NoDevicesException($"File = {file.Path} won't be played cause there are any devices available");
            }

            if (type.IsUrl() && !NetworkUtils.IsInternetAvailable())
            {
                Logger.LogInformation($"File = {file.Path} won't be played cause there is no internet connection available");
                OnServerMessage?.Invoke(AppMessageType.NoInternetConnection);
                throw new InvalidRequestException($"File = {file.Path} won't be played cause there is no internet connection available", AppMessageType.NoInternetConnection);
            }

            var playList = PlayLists.Find(f => f.Id == file.PlayListId);
            if (playList == null)
            {
                Logger.LogInformation($"File = {file.Path} won't be played cause the playListId = {file.PlayListId} does not exist");
                OnServerMessage?.Invoke(AppMessageType.PlayListNotFound);
                throw new PlayListNotFoundException($"File = {file.Path} won't be played cause the playListId = {file.PlayListId} does not exist");
            }
            //playList.SelectedItem = file;

            DisableLoopForAllFiles(file.Id);
            await StopPlayBack();
            SendFileLoading();
            var fileInfo = await GetFileInfo(file.Path, FileCancellationTokenSource.Token);
            if (!fileOptionsChanged)
            {
                await SetAvailableAudiosAndSubTitles(file.Path, fileInfo);
            }

            try
            {
                if (file.CanStartPlayingFromCurrentPercentage &&
                    !type.IsUrl() &&
                    !force &&
                    !AppSettings.StartFilesFromTheStart)
                {
                    Logger.LogInformation(
                        $"{nameof(PlayFile)}: File will be resumed from = {file.PlayedPercentage} %");
                    await GoToPosition(
                        file.Path,
                        CurrentFileVideoStreamIndex,
                        CurrentFileAudioStreamIndex,
                        CurrentFileSubTitleStreamIndex,
                        CurrentFileQuality,
                        file.PlayedPercentage,
                        file.TotalSeconds,
                        fileInfo);
                }
                else
                {
                    Logger.LogInformation($"{nameof(PlayFile)}: Playing file from the start");
                    await StartPlay(
                        file.Path,
                        CurrentFileVideoStreamIndex,
                        CurrentFileAudioStreamIndex,
                        CurrentFileSubTitleStreamIndex,
                        CurrentFileQuality,
                        fileInfo);
                }

                //_currentlyPlayedFile.ListenEvents();
                CurrentPlayedFile = file
                    .BeingPlayed()
                    .UpdateFileInfo(CurrentFileInfo)
                    .WithStreams(CurrentFileVideoStreamIndex, CurrentFileAudioStreamIndex, CurrentFileSubTitleStreamIndex, CurrentFileQuality);
                GenerateThumbnails(file.Path);

                Logger.LogInformation($"{nameof(PlayFile)}: File is being played...");
            }
            catch (Exception e)
            {
                await HandlePlayException(e);
            }
            finally
            {
                //IsBusy = false;
                _onSkipOrPrevious = false;
            }
        }

        public override async Task GoTo(bool nextTrack, bool isAnAutomaticCall = false)
        {
            if (CurrentPlayedFile == null || _onSkipOrPrevious)
                return;

            var playList = InternalGetPlayList(CurrentPlayedFile.PlayListId, false);
            if (playList == null)
            {
                Logger.LogInformation($"{nameof(GoTo)}: PlaylistId = {CurrentPlayedFile.PlayListId} does not exist. It may have been deleted. Playback will stop now");
                await StopPlayBack();
                return;
            }
            var files = playList.Files;
            if (!files.Any())
            {
                Logger.LogInformation($"{nameof(GoTo)}: PlaylistId = {CurrentPlayedFile.PlayListId} does not have any file to play. Playback will stop now");
                await StopPlayBack();
                return;
            }

            _onSkipOrPrevious = true;
            Logger.LogInformation($"{nameof(GoTo)}: Getting the next / previous file to play.... Going to next file = {nextTrack}");
            int increment = nextTrack ? 1 : -1;
            var fileIndex = files.FindIndex(f => f.Id == CurrentPlayedFile.Id);
            int newIndex = fileIndex + increment;
            //TODO: HERE THE PLAYLIST SHOULD BE UP TO DATE WITH THE CHANGES MADE FROM THE CLIENT
            bool random = playList.Shuffle && files.Count > 1;
            if (random)
                Logger.LogInformation($"{nameof(GoTo)}: Random is active for playListId = {playList.Id}, picking a random file ...");

            if (!isAnAutomaticCall && !random && files.Count > 1 && files.ElementAtOrDefault(newIndex) == null)
            {
                Logger.LogInformation($"{nameof(GoTo)}: The new index = {newIndex} does not exist in the playlist, falling back to the first or last item in the list");
                var nextOrPreviousFile = nextTrack ? files.First() : files.Last();
                await PlayFile(nextOrPreviousFile);
                return;
            }

            if (fileIndex < 0)
            {
                Logger.LogInformation(
                    $"{nameof(GoTo)}: File = {CurrentPlayedFile.Path} is no longer present in the playlist, " +
                    "it may have been deleted, getting the closest one...");
                //TODO: HERE THE PLAYED FILE SHOULD BE UP TO DATE WITH THE CHANGES MADE FROM THE CLIENT
                int nextPosition = CurrentPlayedFile.Position + increment;
                int closestPosition = files
                    .Select(f => f.Position)
                    .GetClosest(nextPosition);

                var closestFile = files.FirstOrDefault(f => f.Position == closestPosition);

                Logger.LogInformation($"{nameof(GoTo)}: Closest file is = {closestFile?.Path}, trying to play it");
                if (closestFile?.Id != CurrentPlayedFile.Id)
                    await PlayFile(closestFile);
                return;
            }

            var file = random
                ? files.PickRandom(fileIndex)
                : files.ElementAtOrDefault(newIndex);

            if (file != null)
            {
                Logger.LogInformation(
                    $"{nameof(GoTo)}: The next file to play is = {file.Path} and it's index is = {newIndex} " +
                    $"compared to the old one = {fileIndex}....");
                await PlayFile(file);
                return;
            }
            Logger.LogInformation(
                $"{nameof(GoTo)}: File at index = {fileIndex} in playListId {playList.Id} was not found. " +
                "Probably an end of playlist");

            //TODO: HERE THE PLAYLIST SHOULD BE UP TO DATE WITH THE CHANGES MADE FROM THE CLIENT
            if (!playList.Loop)
            {
                Logger.LogInformation(
                    $"{nameof(GoTo)}: Since no file was found and playlist is not marked to loop, the playback of this playlist will end here");
                await StopPlayBack();
                return;
            }

            Logger.LogInformation($"{nameof(GoTo)}: Looping playlistId = {playList.Id}");
            await PlayFile(files.FirstOrDefault());
        }

        //TODO: MAYBE USE A LOCK TO AVOID PROBLEMS  WHILE UPDATING PLAYLISTS

        public PlayListItemResponseDto GetPlayList(long playListId)
        {
            var playList = InternalGetPlayList(playListId);
            var mapped = _mapper.Map<PlayListItemResponseDto>(playList);
            return mapped;
        }

        public async Task UpdatePlayList(long playListId, string newName, int? position)
        {
            if (string.IsNullOrWhiteSpace(newName))
            {
                SendInvalidRequest();
                return;
            }

            var playList = InternalGetPlayList(playListId);
            newName = newName.Trim();
            if (newName.Length > 100)
            {
                newName = newName.Substring(0, 100);
            }

            playList.Name = newName;
            if (position.HasValue && playList.Position != position && position >= 0)
            {
                //TODO: UPDATE THE PLAYLISTS OR MAYBE ADD A NEW METHOD TO UPDATE THE POSITION
                playList.Position = position.Value;
            }

            await _appDataService.UpdatePlayList(playListId, newName, playList.Position);
            //_appWebServer.OnPlayListChanged?.Invoke(playListId);
        }

        public async Task RemoveFilesThatStartsWith(long playListId, string path)
        {
            var separators = new[] { '/', '\\' };
            var playlist = InternalGetPlayList(playListId);
            var toDelete = playlist.Files
                .Where(f =>
                {
                    if (string.IsNullOrWhiteSpace(f.Path))
                        return false;
                    var filePath = string.Concat(f.Path.Split(separators, StringSplitOptions.RemoveEmptyEntries));
                    var startsWith = string.Concat(path.Split(separators, StringSplitOptions.RemoveEmptyEntries));
                    return filePath.StartsWith(startsWith, StringComparison.OrdinalIgnoreCase);
                })
                .ToList();
            if (!toDelete.Any())
                return;
            await _appDataService.DeleteFiles(toDelete.ConvertAll(f => f.Id));
            playlist.Files.RemoveAll(f => toDelete.Contains(f));
            await AfterRemovingFiles(playListId);
        }

        public async Task RemoveAllMissingFiles(long playListId)
        {
            var playList = InternalGetPlayList(playListId);

            var items = playList.Files.Where(f => !f.Exists).ToList();
            if (items.Count == 0)
                return;

            await _appDataService.DeleteFiles(items.ConvertAll(f => f.Id));
            playList.Files.RemoveAll(f => items.Contains(f));

            //_appWebServer.OnFileDeleted?.Invoke(2);
        }

        public async Task RemoveFiles(long playListId, params long[] ids)
        {
            var playList = InternalGetPlayList(playListId);

            await _appDataService.DeleteFiles(ids.ToList());
            playList.Files.RemoveAll(f => ids.Contains(f.Id));

            foreach (var id in ids)
            {
                SendFileDeleted(playListId, id);
            }
        }

        public Task<List<GetAllPlayListResponseDto>> GetAllPlayLists()
        {
            var mapped = _mapper.Map<List<GetAllPlayListResponseDto>>(PlayLists.OrderBy(pl => pl.Position));

            return Task.FromResult(mapped);
        }

        public async Task<PlayListItemResponseDto> AddNewPlayList()
        {
            var position = PlayLists.Any()
                ? PlayLists.Max(pl => pl.Position) + 1
                : 1;
            var playList = await _appDataService.AddNewPlayList($"New PlayList {PlayLists.Count}", position);
            var serverPlayList = _mapper.Map<ServerPlayList>(playList);
            PlayLists.Add(serverPlayList);

            SendPlayListAdded(playList.Id);
            return _mapper.Map<PlayListItemResponseDto>(serverPlayList);
        }

        public async Task DeletePlayList(long playListId)
        {
            await _appDataService.DeletePlayList(playListId);
            PlayLists.RemoveAll(pl => pl.Id == playListId);
            SendPlayListDeleted(playListId);
            //InitializeOrUpdateFileWatcher(true);
        }

        public async Task DeleteAllPlayLists(long exceptId)
        {
            if (PlayLists.Count <= 1)
                return;
            var items = PlayLists.Where(pl => pl.Id != exceptId).ToList();

            var ids = items.Select(pl => pl.Id).ToList();
            await _appDataService.DeletePlayLists(ids);

            PlayLists.RemoveAll(pl => ids.Contains(pl.Id));
            foreach (var id in ids)
            {
                SendPlayListDeleted(id);
            }

            //InitializeOrUpdateFileWatcher(true);
        }

        public void SortFiles(long playListId, SortModeType sortBy)
        {
            var playList = InternalGetPlayList(playListId);

            if ((sortBy == SortModeType.DurationAsc || sortBy == SortModeType.DurationDesc) &&
                playList.Files.Any(f => string.IsNullOrEmpty(f.Duration)))
            {
                OnServerMessage?.Invoke(AppMessageType.OneOrMoreFilesAreNotReadyYet);
                return;
            }
            playList.Files = sortBy switch
            {
                SortModeType.AlphabeticalPathAsc => playList.Files.OrderBy(f => f.Path, new WindowsExplorerComparer()).ToList(),
                SortModeType.AlphabeticalPathDesc => playList.Files.OrderByDescending(f => f.Path, new WindowsExplorerComparer()).ToList(),
                SortModeType.DurationAsc => playList.Files.OrderBy(f => f.TotalSeconds).ToList(),
                SortModeType.DurationDesc => playList.Files.OrderByDescending(f => f.TotalSeconds).ToList(),
                SortModeType.AlphabeticalNameAsc => playList.Files.OrderBy(f => f.Filename, new WindowsExplorerComparer()).ToList(),
                SortModeType.AlphabeticalNameDesc => playList.Files.OrderByDescending(f => f.Filename, new WindowsExplorerComparer()).ToList(),
                _ => throw new ArgumentOutOfRangeException(nameof(sortBy), sortBy, "Invalid sort mode"),
            };

            SetPositionIfChanged(playListId);
        }

        public void SetPositionIfChanged(long playListId)
        {
            var playList = InternalGetPlayList(playListId);
            for (int i = 0; i < playList.Files.Count; i++)
            {
                var item = playList.Files[i];
                int newValue = i + 1;
                if (item.Position != newValue)
                {
                    item.Position = newValue;
                    item.PositionChanged = true;
                }
            }
        }

        private Task AfterRemovingFiles(long id)
        {
            return Task.CompletedTask;
            //SelectedItems.Clear();
            //SetPositionIfChanged();

            //AppWebServer.OnPlayListDeleted?.Invoke(id);
            //await UpdatePlayedTime();
        }

        public Task AddFolder(long playListId, string[] folders)
        {
            var files = new List<string>();
            foreach (var folder in folders)
            {
                Logger.LogInformation($"{nameof(AddFolder)}: Getting all the media files from folder = {folder}");
                var filesInDir = Directory.EnumerateFiles(folder, "*.*", SearchOption.AllDirectories)
                    .Where(s => FileFormatConstants.AllowedFormats.Contains(Path.GetExtension(s).ToLower()))
                    .ToList();
                files.AddRange(filesInDir);
            }
            return AddFiles(playListId, files.ToArray());
        }

        public async Task AddFiles(long playListId, string[] paths)
        {
            Logger.LogInformation($"{nameof(AddFiles)}: Trying to add new files to playListId = {playListId}...");
            if (paths == null || paths.Length == 0)
            {
                Logger.LogInformation($"{nameof(AddFiles)}: No paths were provided");
                OnServerMessage?.Invoke(AppMessageType.NoFilesToBeAdded);
                throw new InvalidRequestException("No paths were provided", AppMessageType.NoFilesToBeAdded);
            }

            if (paths.Any(p => string.IsNullOrEmpty(p) || p.Length > 1000))
            {
                Logger.LogInformation($"{nameof(AddFiles)}: One of the provided paths is not valid");
                OnServerMessage?.Invoke(AppMessageType.FilesAreNotValid);
                throw new FileNotSupportedException("One of the provided paths is not valid");
            }

            var playList = PlayLists.Find(pl => pl.Id == playListId);
            if (playList == null)
            {
                Logger.LogWarning($"{nameof(AddFiles)}: PlayListId = {playListId} does not exist");
                OnServerMessage?.Invoke(AppMessageType.PlayListNotFound);
                throw new PlayListNotFoundException($"PlayListId = {playListId} does not exist");
            }

            int startIndex = playList.Files.Count + 1;
            var files = paths.Where(path =>
                {
                    var ext = Path.GetExtension(path);
                    return FileFormatConstants.AllowedFormats.Contains(ext.ToLower()) && playList.Files.All(f => f.Path != path);
                }).OrderBy(p => p, new WindowsExplorerComparer())
                .Select((path, index) => new FileItem
                {
                    Position = startIndex + index,
                    PlayListId = playListId,
                    Path = path,
                    CreatedAt = DateTime.Now
                }).ToList();

            var createdFiles = await _appDataService.AddFiles(files);
            OnFilesAdded.Invoke(playListId, createdFiles.ToArray());
        }

        public async Task AddFile(long playListId, FileItem file)
        {
            var playList = PlayLists.Find(f => f.Id == playListId);
            if (playList == null)
            {
                Logger.LogWarning($"{nameof(AddFile)}: PlayListId = {playListId} does not exist. It may have been removed");
                return;
            }

            var fileInfo = await GetFileInfo(file.Path, FileCancellationTokenSource.Token);
            playList.Files.Add(ServerFileItem.From(FileService, file, fileInfo));
            SendFileAdded(playList.Id, file.Id);
        }

        public async Task AddUrl(long playListId, string url, bool onlyVideo)
        {
            if (!NetworkUtils.IsInternetAvailable())
            {
                throw new InvalidRequestException(
                    $"Can't add file = {url} to playListId = {playListId} cause there's no internet connection",
                    AppMessageType.NoInternetConnection);
            }

            bool isUrlFile = FileService.IsUrlFile(url);
            if (!isUrlFile || !YoutubeUrlDecoder.IsYoutubeUrl(url))
            {
                OnServerMessage?.Invoke(AppMessageType.UrlNotSupported);
                return;
            }

            try
            {
                Logger.LogInformation($"{nameof(AddUrl)}: Trying to parse url = {url}");
                if (!YoutubeUrlDecoder.IsPlayList(url) || YoutubeUrlDecoder.IsPlayListAndVideo(url) && onlyVideo)
                {
                    Logger.LogInformation($"{nameof(AddUrl)}: Url is either not a playlist or we just want the video, parsing it...");
                    await AddYoutubeUrl(playListId, url);
                    return;
                }

                Logger.LogInformation($"{nameof(AddUrl)}: Parsing playlist...");
                var links = await YoutubeUrlDecoder.ParseYouTubePlayList(url, FileCancellationTokenSource.Token);
                foreach (var link in links)
                {
                    if (FileCancellationTokenSource.IsCancellationRequested)
                        break;
                    Logger.LogInformation($"{nameof(AddUrl)}: Parsing playlist url = {link}");
                    await AddYoutubeUrl(playListId, link);
                }
            }
            catch (Exception e)
            {
                OnServerMessage?.Invoke(AppMessageType.UrlCouldntBeParsed);
                TelemetryService.TrackError(e);
                Logger.LogError(e, $"{nameof(AddUrl)}: Couldn't parse url = {url}");
            }
        }

        public ServerPlayerStatus GetPlayerStatus()
        {
            return new ServerPlayerStatus
            {
                PlayerStatus = Player.State,
                CurrentFileItem = CurrentPlayedFile
            };
        }

        private async Task AddYoutubeUrl(long playListId, string url)
        {
            //TODO: AppConstants.MaxCharsPerString = 1000
            var media = await YoutubeUrlDecoder.Parse(url, null, false);
            if (media == null)
            {
                Logger.LogWarning($"{nameof(AddYoutubeUrl)}: Couldn't parse url = {url}");
                OnServerMessage?.Invoke(AppMessageType.UrlCouldntBeParsed);
                return;
            }
            if (!string.IsNullOrEmpty(media.Title) && media.Title.Length > 1000)
            {
                media.Title = media.Title.Substring(0, 1000);
            }
            var playList = InternalGetPlayList(playListId);
            var createdFile = await _appDataService.AddFile(playListId, url, playList.Files.Count + 1, media.Title);
            var fileInfo = await GetFileInfo(createdFile.Path, FileCancellationTokenSource.Token);
            playList.Files.Add(ServerFileItem.From(FileService, createdFile, fileInfo));
            SendFileAdded(playListId, createdFile.Id);
        }

        public FileLoadedResponseDto GetCurrentFileLoaded()
        {
            if (CurrentPlayedFile == null)
                return null;

            var playList = InternalGetPlayList(CurrentPlayedFile.PlayListId, false);
            return new FileLoadedResponseDto
            {
                Id = CurrentPlayedFile.Id,
                Duration = CurrentPlayedFile.TotalSeconds,
                Filename = CurrentPlayedFile.Filename,
                LoopFile = CurrentPlayedFile.Loop,
                CurrentSeconds = Player.ElapsedSeconds,
                IsPaused = Player.IsPaused,
                IsMuted = Player.IsMuted,
                VolumeLevel = Player.CurrentVolumeLevel,
                ThumbnailUrl = CurrentThumbnailUrl,
                PlayListId = playList?.Id ?? 0,
                PlayListName = playList?.Name ?? "N/A",
                LoopPlayList = playList?.Loop ?? false,
                ShufflePlayList = playList?.Shuffle ?? false
            };
        }

        public void SetPlayListOptions(long id, bool loop, bool shuffle)
        {
            var playlist = InternalGetPlayList(id);
            playlist.Loop = loop;
            playlist.Shuffle = shuffle;
        }

        private async Task HandlePlayException(Exception e)
        {
            CurrentPlayedFile = null;
            //playList.SelectedItem = null;
            await StopPlayBack();

            //TODO: USE THE MIDDLEWARE TO HANDLE ALL THE CASES
            switch (e)
            {
                case NotSupportedException _:
                    OnServerMessage.Invoke(AppMessageType.FileNotSupported);
                    break;
                case NoDevicesException _:
                    OnServerMessage.Invoke(AppMessageType.NoDevicesFound);
                    break;
                case ConnectingException _:
                    OnServerMessage.Invoke(AppMessageType.ConnectionToDeviceIsStillInProgress);
                    break;
                default:
                    Logger.LogError(e, $"{nameof(HandlePlayException)}: Unknown error occurred");
                    OnServerMessage.Invoke(AppMessageType.UnknownErrorLoadingFile);
                    TelemetryService.TrackError(e);
                    break;
            }
        }

        private async Task StopPlayBack()
        {
            if (IsPlayingOrPaused)
            {
                await StopPlayback();
            }

            await OnStoppedPlayBack();
        }

        private async Task OnStoppedPlayBack()
        {
            CurrentPlayedFile = null;
            OnEndReached?.Invoke();
            await StopRunningProcess();
            //SetCurrentlyPlayingInfo(null, false);
            //IsPaused = false;
            DisableLoopForAllFiles();
        }

        public async Task<FFProbeFileInfo> GetFileInfo(string mrl, CancellationToken token)
        {
            if (FileService.IsUrlFile(mrl))
            {
                return new FFProbeFileInfo
                {
                    Format = new FileInfoFormat()
                };
            }

            return await FFmpegService.GetFileInfo(mrl, token);
        }

        private async Task SetAvailableAudiosAndSubTitles(string mrl, FFProbeFileInfo fileInfo)
        {
            Logger.LogInformation($"{nameof(SetAvailableAudiosAndSubTitles)}: Cleaning current file videos, audios and subs streams");
            CurrentFileVideos.Clear();
            CurrentFileAudios.Clear();
            CurrentFileQualities.Clear();
            CurrentFileSubTitles.Clear();
            //if (_currentlyPlayedFile == null)
            //{
            //    _logger.LogWarning($"{nameof(GetAvailableAudiosAndSubTitles)}: Current file is null");
            //    return;
            //}

            if (fileInfo == null)
            {
                fileInfo = await GetFileInfo(mrl, FileCancellationTokenSource.Token);
                if (fileInfo == null)
                {
                    Logger.LogWarning($"{nameof(SetAvailableAudiosAndSubTitles)}: Current file = {mrl} doesnt have a fileinfo");
                    return;
                }
            }

            Logger.LogInformation($"{nameof(SetAvailableAudiosAndSubTitles)}: Setting available file videos, audios and subs streams");

            //Videos
            bool isSelected = true;
            bool isEnabled = fileInfo.Videos.Count > 1;
            foreach (var video in fileInfo.Videos)
            {
                CurrentFileVideos.Add(FileItemOptionsResponseDto.ForVideo(video.Index, isSelected, isEnabled, video.VideoText));
                isSelected = false;
            }

            //Audios
            isSelected = true;
            isEnabled = fileInfo.Audios.Count > 1;
            foreach (var audio in fileInfo.Audios)
            {
                CurrentFileAudios.Add(FileItemOptionsResponseDto.ForAudio(audio.Index, isSelected, isEnabled, audio.AudioText));
                isSelected = false;
            }

            //Subtitles
            if (!FileService.IsVideoFile(mrl))
                return;

            var (localSubsPath, filename) = TryGetSubTitlesLocalPath(mrl);
            bool localSubExists = !string.IsNullOrEmpty(localSubsPath);
            isEnabled = fileInfo.SubTitles.Count > 1 || localSubExists;
            //TODO: TRANSLATE THE NONE ?
            CurrentFileSubTitles.Add(FileItemOptionsResponseDto.ForEmbeddedSubtitles(ServerConstants.NoStreamSelectedId, !localSubExists && fileInfo.SubTitles.Count == 0 || !AppSettings.LoadFirstSubtitleFoundAutomatically, isEnabled, "None"));
            if (localSubExists)
            {
                CurrentFileSubTitles.Add(FileItemOptionsResponseDto.ForLocalSubtitles(ServerConstants.NoStreamSelectedId - 1, filename, localSubsPath));
            }

            isEnabled = fileInfo.SubTitles.Count > 0;
            isSelected = !localSubExists && AppSettings.LoadFirstSubtitleFoundAutomatically;
            foreach (var subtitle in fileInfo.SubTitles)
            {
                CurrentFileSubTitles.Add(FileItemOptionsResponseDto.ForEmbeddedSubtitles(subtitle.Index, isSelected, isEnabled, subtitle.SubTitleText));
                isSelected = false;
            }
        }

        private (string, string) TryGetSubTitlesLocalPath(string mrl)
        {
            Logger.LogInformation($"{nameof(TryGetSubTitlesLocalPath)}: Checking if subtitle exist in the same dir as file = {mrl}");
            var (possibleSubTitlePath, filename) = FileService.TryGetSubTitlesLocalPath(mrl);
            if (!string.IsNullOrWhiteSpace(possibleSubTitlePath))
            {
                Logger.LogInformation($"{nameof(TryGetSubTitlesLocalPath)}: Found subtitles in path = {possibleSubTitlePath}");
                return (possibleSubTitlePath, filename);
            }

            Logger.LogInformation($"{nameof(TryGetSubTitlesLocalPath)}: No subtitles were found for file = {mrl}");
            return (possibleSubTitlePath, filename);
        }

        public void DisableLoopForAllFiles(long exceptFileId = -1)
        {
            var files = PlayLists.SelectMany(pl => pl.Files).Where(f => f.Loop && f.Id != exceptFileId).ToList();

            foreach (var file in files)
                file.Loop = false;

            //TODO: TRIGGER CHANGE ?
        }

        private ServerPlayList InternalGetPlayList(long id, bool throwOnNotFound = true)
        {
            var playList = PlayLists.Find(pl => pl.Id == id);
            if (playList != null)
                return playList;
            Logger.LogWarning($"{nameof(InternalGetPlayList)}: PlaylistId = {id} doesn't exists");
            OnServerMessage?.Invoke(AppMessageType.PlayListNotFound);
            if (throwOnNotFound)
                throw new PlayListNotFoundException($"PlayListId = {id} does not exist");
            return null;
        }
    }
}
