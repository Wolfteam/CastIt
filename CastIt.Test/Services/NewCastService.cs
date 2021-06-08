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
using CastIt.Infrastructure.Interfaces;
using CastIt.Infrastructure.Services;
using CastIt.Test.Common;
using CastIt.Test.Common.Comparers;
using CastIt.Test.Interfaces;
using CastIt.Test.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CastIt.Test
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

    public class NewCastService : CastService, IServerCastService
    {
        #region Members
        private readonly IAppDataService _appDataService;
        private readonly IMapper _mapper;

        private bool _onSkipOrPrevious;
        //TODO: COMBINE THIS PROPS IN THE CurrentPlayedFile  CLASS
        private PlayList _currentPlayList;
        private FileItem _currentlyPlayedFile;
        private ServerFileItem _currentPlayedFile;

        private readonly CancellationTokenSource _fileCancellationTokenSource = new CancellationTokenSource();
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

        public NewCastService(
            ILogger<CastService> logger,
            IBaseWebServer appWebServer,
            IFFmpegService ffmpegService,
            IYoutubeUrlDecoder youtubeUrlDecoder,
            ITelemetryService telemetryService,
            IAppSettingsService appSettings,
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
                    .Select(f => MapToServerFileItem(f, null))
                    .OrderBy(f => f.Position)
                    .ToList();
                mappedPlayLists.Add(mapped);
            }

            PlayLists.AddRange(mappedPlayLists.OrderBy(pl => pl.Position));
            await base.Init();
        }

        public override Task CleanThemAll()
        {
            _fileCancellationTokenSource.Cancel();
            return base.CleanThemAll();
        }

        public Task PlayFile(FileItem file, bool force = false)
        {
            return PlayFile(file, force, false);
        }

        public async Task PlayFile(long id, bool force, bool fileOptionsChanged)
        {
            var file = await _appDataService.GetFile(id);
            //TODO: THROW NOT FOUND EX
            await PlayFile(file, force, fileOptionsChanged);
        }

        public async Task PlayFile(FileItem file, bool force, bool fileOptionsChanged)
        {
            if (file == null)
            {
                _logger.LogWarning("The provided file won't be played cause it is null");
                OnServerMessage?.Invoke(AppMessageType.FileNotFound);
                throw new Domain.Exceptions.FileNotFoundException("The provided file won't be played cause it is null");
            }

            var type = _fileService.GetFileType(file.Path);

            if (type.DoesNotExist())
            {
                _logger.LogWarning($"The provided file = {file.Path} does not exist");
                OnServerMessage?.Invoke(AppMessageType.FileNotFound);
                throw new Domain.Exceptions.FileNotFoundException($"The provided file = {file.Path} does not exist");
            }
            //TODO: if (file == _currentlyPlayedFile && !force && !fileOptionsChanged && !file.Loop)
            bool fileIsBeingPlayed = file.Id == _currentlyPlayedFile?.Id && !force && !fileOptionsChanged;
            if (fileIsBeingPlayed)
            {
                _logger.LogInformation($"The provided file = {file.Path} is already being played");
                OnServerMessage?.Invoke(AppMessageType.FileIsAlreadyBeingPlayed);
                throw new InvalidRequestException($"The provided file = {file.Path} is already being played", AppMessageType.FileIsAlreadyBeingPlayed);
            }

            //if (string.IsNullOrEmpty(file.Duration))
            //{
            //    Logger.LogInformation(
            //        $"{nameof(PlayFile)}: Cant play file = {file.Filename} yet, " +
            //        $"because im still setting the duration for some files.");
            //    await ShowSnackbarMsg(GetText("FileIsNotReadyYet"));
            //    return false;
            //}

            if (!AvailableDevices.Any())
            {
                _logger.LogInformation($"File = {file.Path} won't be played cause there are any devices available");
                OnServerMessage?.Invoke(AppMessageType.NoDevicesFound);
                throw new NoDevicesException($"File = {file.Path} won't be played cause there are any devices available");
            }

            if (type.IsUrl() && !NetworkUtils.IsInternetAvailable())
            {
                _logger.LogInformation($"File = {file.Path} won't be played cause there is no internet connection available");
                OnServerMessage?.Invoke(AppMessageType.NoInternetConnection);
                throw new InvalidRequestException($"File = {file.Path} won't be played cause there is no internet connection available", AppMessageType.NoInternetConnection);
            }

            var playList = await _appDataService.GetPlayList(file.PlayListId);
            if (playList == null)
            {
                _logger.LogInformation($"File = {file.Path} won't be played cause the playListId = {file.PlayListId} does not exist");
                OnServerMessage?.Invoke(AppMessageType.PlayListNotFound);
                throw new PlayListNotFoundException($"File = {file.Path} won't be played cause the playListId = {file.PlayListId} does not exist");
            }
            //playList.SelectedItem = file;

            DisableLoopForAllFiles(file.Id);
            await StopPlayBack();
            SendFileLoading();
            //TODO: TOKEN HERE
            var fileInfo = await GetFileInfo(file.Path, default);
            if (!fileOptionsChanged)
            {
                await SetAvailableAudiosAndSubTitles(file.Path, fileInfo);
            }

            try
            {
                if (file.CanStartPlayingFromCurrentPercentage &&
                    !type.IsUrl() &&
                    !force &&
                    !_appSettings.StartFilesFromTheStart)
                {
                    _logger.LogInformation(
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
                    _logger.LogInformation($"{nameof(PlayFile)}: Playing file from the start");
                    await StartPlay(
                        file.Path,
                        CurrentFileVideoStreamIndex,
                        CurrentFileAudioStreamIndex,
                        CurrentFileSubTitleStreamIndex,
                        CurrentFileQuality,
                        fileInfo);
                }

                //_currentlyPlayedFile.ListenEvents();
                _currentPlayList = playList;
                _currentlyPlayedFile = file;
                GenerateThumbnails(file.Path);
                _currentPlayedFile = MapToServerFileItem(_currentlyPlayedFile, CurrentFileInfo);

                _logger.LogInformation($"{nameof(PlayFile)}: File is being played...");
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

        public async Task GoTo(bool nextTrack, bool isAnAutomaticCall = false)
        {
            if (_currentlyPlayedFile == null || _onSkipOrPrevious)
                return;

            var files = await _appDataService.GetAllFiles(_currentPlayList.Id);
            if (_currentPlayList == null || !files.Any())
            {
                _logger.LogInformation($"{nameof(GoTo)}: PlaylistId = {_currentlyPlayedFile.PlayListId} does not exist. It may have been deleted. Playback will stop now");
                await StopPlayBack();
                return;
            }

            _onSkipOrPrevious = true;
            _logger.LogInformation($"{nameof(GoTo)}: Getting the next / previous file to play.... Going to next file = {nextTrack}");
            int increment = nextTrack ? 1 : -1;
            var fileIndex = files.FindIndex(f => f.Id == _currentlyPlayedFile.Id);
            int newIndex = fileIndex + increment;
            //TODO: HERE THE PLAYLIST SHOULD BE UP TO DATE WITH THE CHANGES MADE FROM THE CLIENT
            bool random = _currentPlayList.Shuffle && files.Count > 1;
            if (random)
                _logger.LogInformation($"{nameof(GoTo)}: Random is active for playListId = {_currentPlayList.Id}, picking a random file ...");

            if (!isAnAutomaticCall && !random && files.Count > 1 && files.ElementAtOrDefault(newIndex) == null)
            {
                _logger.LogInformation($"{nameof(GoTo)}: The new index = {newIndex} does not exist in the playlist, falling back to the first or last item in the list");
                var nextOrPreviousFile = nextTrack ? files.First() : files.Last();
                await PlayFile(nextOrPreviousFile);
                return;
            }

            if (fileIndex < 0)
            {
                _logger.LogInformation(
                    $"{nameof(GoTo)}: File = {_currentlyPlayedFile.Path} is no longer present in the playlist, " +
                    "it may have been deleted, getting the closest one...");
                //TODO: HERE THE PLAYED FILE SHOULD BE UP TO DATE WITH THE CHANGES MADE FROM THE CLIENT
                int nextPosition = _currentlyPlayedFile.Position + increment;
                int closestPosition = files
                    .Select(f => f.Position)
                    .GetClosest(nextPosition);

                var closestFile = files.FirstOrDefault(f => f.Position == closestPosition);

                _logger.LogInformation($"{nameof(GoTo)}: Closest file is = {closestFile?.Path}, trying to play it");
                if (closestFile?.Id != _currentlyPlayedFile.Id)
                    await PlayFile(closestFile);
                return;
            }

            var file = random
                ? files.PickRandom(fileIndex)
                : files.ElementAtOrDefault(newIndex);

            if (file != null)
            {
                _logger.LogInformation(
                    $"{nameof(GoTo)}: The next file to play is = {file.Path} and it's index is = {newIndex} " +
                    $"compared to the old one = {fileIndex}....");
                await PlayFile(file);
                return;
            }
            _logger.LogInformation(
                $"{nameof(GoTo)}: File at index = {fileIndex} in playListId {_currentPlayList.Id} was not found. " +
                "Probably an end of playlist");

            //TODO: HERE THE PLAYLIST SHOULD BE UP TO DATE WITH THE CHANGES MADE FROM THE CLIENT
            if (!_currentPlayList.Loop)
            {
                _logger.LogInformation(
                    $"{nameof(GoTo)}: Since no file was found and playlist is not marked to loop, the playback of this playlist will end here");
                await StopPlayBack();
                return;
            }

            _logger.LogInformation($"{nameof(GoTo)}: Looping playlistId = {_currentPlayList.Id}");
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
            _appWebServer.OnPlayListChanged?.Invoke(playListId);
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

            _appWebServer.OnFileDeleted?.Invoke(2);
        }

        public async Task RemoveFiles(long playListId, params long[] ids)
        {
            var playList = InternalGetPlayList(playListId);

            await _appDataService.DeleteFiles(ids.ToList());
            playList.Files.RemoveAll(f => ids.Contains(f.Id));

            _appWebServer.OnFileDeleted?.Invoke(1);
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

            _appWebServer.OnPlayListAdded?.Invoke(playList.Id);

            return _mapper.Map<PlayListItemResponseDto>(serverPlayList);
        }

        public async Task DeletePlayList(long playListId)
        {
            await _appDataService.DeletePlayList(playListId);
            PlayLists.RemoveAll(pl => pl.Id == playListId);
            _appWebServer.OnPlayListDeleted?.Invoke(playListId);
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
                _appWebServer.OnPlayListDeleted?.Invoke(id);
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

        private async Task AfterRemovingFiles(long id)
        {
            //SelectedItems.Clear();
            //SetPositionIfChanged();
            _appWebServer.OnPlayListDeleted?.Invoke(id);
            //await UpdatePlayedTime();
        }

        public Task AddFolder(long playListId, string[] folders)
        {
            var files = new List<string>();
            foreach (var folder in folders)
            {
                _logger.LogInformation($"{nameof(AddFolder)}: Getting all the media files from folder = {folder}");
                var filesInDir = Directory.EnumerateFiles(folder, "*.*", SearchOption.AllDirectories)
                    .Where(s => FileFormatConstants.AllowedFormats.Contains(Path.GetExtension(s).ToLower()))
                    .ToList();
                files.AddRange(filesInDir);
            }
            return AddFiles(playListId, files.ToArray());
        }

        public async Task AddFiles(long playListId, string[] paths)
        {
            _logger.LogInformation($"{nameof(AddFiles)}: Trying to add new files to playListId = {playListId}...");
            if (paths == null || paths.Length == 0)
            {
                _logger.LogInformation($"{nameof(AddFiles)}: No paths were provided");
                OnServerMessage?.Invoke(AppMessageType.NoFilesToBeAdded);
                throw new InvalidRequestException("No paths were provided", AppMessageType.NoFilesToBeAdded);
            }

            if (paths.Any(p => string.IsNullOrEmpty(p) || p.Length > 1000))
            {
                _logger.LogInformation($"{nameof(AddFiles)}: One of the provided paths is not valid");
                OnServerMessage?.Invoke(AppMessageType.FilesAreNotValid);
                throw new FileNotSupportedException("One of the provided paths is not valid");
            }

            var playList = PlayLists.Find(pl => pl.Id == playListId);
            if (playList == null)
            {
                _logger.LogWarning($"{nameof(AddFiles)}: PlayListId = {playListId} does not exist");
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
            //TODO: FIGURE OUT HOW CAN I IMPROVE THIS (maybe an event that gets handled in the casthosted service)
            AfterAddingFiles(playList, createdFiles);
        }

        private async void AfterAddingFiles(ServerPlayList playList, IEnumerable<FileItem> createdFiles)
        {
            foreach (var file in createdFiles)
            {
                var fileInfo = await GetFileInfo(file.Path, _fileCancellationTokenSource.Token);
                playList?.Files?.Add(MapToServerFileItem(file, fileInfo));
                _appWebServer.OnFileAdded?.Invoke(222);
            }
        }

        public async Task AddUrl(long playListId, string url, bool onlyVideo)
        {
            if (!NetworkUtils.IsInternetAvailable())
            {
                throw new InvalidRequestException(
                    $"Can't add file = {url} to playListId = {playListId} cause there's no internet connection",
                    AppMessageType.NoInternetConnection);
            }

            bool isUrlFile = _fileService.IsUrlFile(url);
            if (!isUrlFile || !_youtubeUrlDecoder.IsYoutubeUrl(url))
            {
                OnServerMessage?.Invoke(AppMessageType.UrlNotSupported);
                return;
            }

            try
            {
                _logger.LogInformation($"{nameof(AddUrl)}: Trying to parse url = {url}");
                if (!_youtubeUrlDecoder.IsPlayList(url) || _youtubeUrlDecoder.IsPlayListAndVideo(url) && onlyVideo)
                {
                    _logger.LogInformation($"{nameof(AddUrl)}: Url is either not a playlist or we just want the video, parsing it...");
                    await AddYoutubeUrl(playListId, url);
                    return;
                }

                _logger.LogInformation($"{nameof(AddUrl)}: Parsing playlist...");
                var links = await _youtubeUrlDecoder.ParseYouTubePlayList(url, _fileCancellationTokenSource.Token);
                foreach (var link in links)
                {
                    if (_fileCancellationTokenSource.IsCancellationRequested)
                        break;
                    _logger.LogInformation($"{nameof(AddUrl)}: Parsing playlist url = {link}");
                    await AddYoutubeUrl(playListId, link);
                }
            }
            catch (Exception e)
            {
                OnServerMessage?.Invoke(AppMessageType.UrlCouldntBeParsed);
                _telemetryService.TrackError(e);
                _logger.LogError(e, $"{nameof(AddUrl)}: Couldn't parse url = {url}");
            }
        }

        private async Task AddYoutubeUrl(long playListId, string url)
        {
            //TODO: AppConstants.MaxCharsPerString = 1000
            var media = await _youtubeUrlDecoder.Parse(url, null, false);
            if (media == null)
            {
                _logger.LogWarning($"{nameof(AddYoutubeUrl)}: Couldn't parse url = {url}");
                OnServerMessage?.Invoke(AppMessageType.UrlCouldntBeParsed);
                return;
            }
            if (!string.IsNullOrEmpty(media.Title) && media.Title.Length > 1000)
            {
                media.Title = media.Title.Substring(0, 1000);
            }
            var playList = InternalGetPlayList(playListId);
            var createdFile = await _appDataService.AddFile(playListId, url, playList.Files.Count + 1, media.Title);
            var fileInfo = await GetFileInfo(createdFile.Path, _fileCancellationTokenSource.Token);
            playList.Files.Add(MapToServerFileItem(createdFile, fileInfo));
            _appWebServer.OnFileAdded?.Invoke(createdFile.Id);
        }

        public FileLoadedResponseDto GetCurrentFileLoaded()
        {
            if (_currentPlayedFile == null)
                return null;

            return new FileLoadedResponseDto
            {
                Id = _currentPlayedFile.Id,
                Duration = _currentPlayedFile.TotalSeconds,
                Filename = _currentPlayedFile.Filename,
                LoopFile = _currentPlayedFile.Loop,
                CurrentSeconds = _player.ElapsedSeconds,
                IsPaused = _player.IsPaused,
                IsMuted = _player.IsMuted,
                VolumeLevel = _player.CurrentVolumeLevel,
                ThumbnailUrl = CurrentThumbnailUrl,
                PlayListId = _currentPlayList?.Id ?? 0,
                PlayListName = _currentPlayList?.Name ?? "N/A",
                LoopPlayList = _currentPlayList?.Loop ?? false,
                ShufflePlayList = _currentPlayList?.Shuffle ?? false
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
            _currentlyPlayedFile = null;
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
                    _logger.LogError(e, $"{nameof(HandlePlayException)}: Unknown error occurred");
                    OnServerMessage.Invoke(AppMessageType.UnknownErrorLoadingFile);
                    _telemetryService.TrackError(e);
                    throw e;
            }
        }

        private async Task StopPlayBack()
        {
            if (IsPlayingOrPaused)
            {
                await StopPlayback();
            }

            OnStoppedPlayBack();
        }

        private void OnStoppedPlayBack()
        {
            _currentlyPlayedFile = null;
            OnEndReached?.Invoke();
            StopRunningProcess();
            //SetCurrentlyPlayingInfo(null, false);
            //IsPaused = false;
            DisableLoopForAllFiles();
        }

        private async Task<FFProbeFileInfo> GetFileInfo(string mrl, CancellationToken token)
        {
            if (_fileService.IsUrlFile(mrl))
            {
                return new FFProbeFileInfo
                {
                    Format = new FileInfoFormat()
                };
            }

            return await _ffmpegService.GetFileInfo(mrl, token);
        }

        private async Task SetAvailableAudiosAndSubTitles(string mrl, FFProbeFileInfo fileInfo)
        {
            _logger.LogInformation($"{nameof(SetAvailableAudiosAndSubTitles)}: Cleaning current file videos, audios and subs streams");
            //if (_currentlyPlayedFile == null)
            //{
            //    _logger.LogWarning($"{nameof(GetAvailableAudiosAndSubTitles)}: Current file is null");
            //    return;
            //}

            if (fileInfo == null)
            {
                fileInfo = await GetFileInfo(mrl, _fileCancellationTokenSource.Token);
                if (fileInfo == null)
                {
                    _logger.LogWarning($"{nameof(SetAvailableAudiosAndSubTitles)}: Current file = {mrl} doesnt have a fileinfo");
                    return;
                }
            }

            _logger.LogInformation($"{nameof(SetAvailableAudiosAndSubTitles)}: Setting available file videos, audios and subs streams");

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
            if (!_fileService.IsVideoFile(mrl))
                return;

            var (localSubsPath, filename) = TryGetSubTitlesLocalPath(mrl);
            bool localSubExists = !string.IsNullOrEmpty(localSubsPath);
            isEnabled = fileInfo.SubTitles.Count > 1 || localSubExists;
            //TODO: TRANSLATE THE NONE ?
            CurrentFileSubTitles.Add(FileItemOptionsResponseDto.ForEmbeddedSubtitles(ServerConstants.NoStreamSelectedId, !localSubExists && fileInfo.SubTitles.Count == 0 || !_appSettings.LoadFirstSubtitleFoundAutomatically, isEnabled, "None"));
            if (localSubExists)
            {
                CurrentFileSubTitles.Add(FileItemOptionsResponseDto.ForLocalSubtitles(ServerConstants.NoStreamSelectedId - 1, filename, localSubsPath));
            }

            isEnabled = fileInfo.SubTitles.Count > 0;
            isSelected = !localSubExists && _appSettings.LoadFirstSubtitleFoundAutomatically;
            foreach (var subtitle in fileInfo.SubTitles)
            {
                CurrentFileSubTitles.Add(FileItemOptionsResponseDto.ForEmbeddedSubtitles(subtitle.Index, isSelected, isEnabled, subtitle.SubTitleText));
                isSelected = false;
            }
        }

        private (string, string) TryGetSubTitlesLocalPath(string mrl)
        {
            _logger.LogInformation($"{nameof(TryGetSubTitlesLocalPath)}: Checking if subtitle exist in the same dir as file = {mrl}");
            var (possibleSubTitlePath, filename) = _fileService.TryGetSubTitlesLocalPath(mrl);
            if (!string.IsNullOrWhiteSpace(possibleSubTitlePath))
            {
                _logger.LogInformation($"{nameof(TryGetSubTitlesLocalPath)}: Found subtitles in path = {possibleSubTitlePath}");
                return (possibleSubTitlePath, filename);
            }

            _logger.LogInformation($"{nameof(TryGetSubTitlesLocalPath)}: No subtitles were found for file = {mrl}");
            return (possibleSubTitlePath, filename);
        }

        private ServerFileItem MapToServerFileItem(FileItem file, FFProbeFileInfo fileInfo)
        {
            var totalSeconds = file.TotalSeconds > 0 && fileInfo == null
                ? file.TotalSeconds
                : fileInfo?.Format?.Duration ?? -1;
            var serverFileItem = new ServerFileItem(_fileService, fileInfo)
            {
                Path = file.Path,
                PlayListId = file.PlayListId,
                Description = file.Description,
                Id = file.Id,
                Name = file.Name,
                Position = file.Position,
                PlayedPercentage = file.PlayedPercentage,
                TotalSeconds = totalSeconds
            };

            if (!serverFileItem.Exists)
            {
                serverFileItem.TotalSeconds = 0;
                serverFileItem.Duration = "Missing";
                return serverFileItem;
            }
            serverFileItem.TotalSeconds = totalSeconds;
            if (totalSeconds <= 0)
            {
                serverFileItem.Duration = "N/A";
                return serverFileItem;
            }
            serverFileItem.Duration = FileFormatConstants.FormatDuration(totalSeconds);
            return serverFileItem;
        }

        public void DisableLoopForAllFiles(long exceptFileId = -1)
        {
            var files = PlayLists.SelectMany(pl => pl.Files).Where(f => f.Loop && f.Id != exceptFileId).ToList();

            foreach (var file in files)
                file.Loop = false;

            //TODO: TRIGGER CHANGE ?
        }

        private ServerPlayList InternalGetPlayList(long id)
        {
            var playList = PlayLists.Find(pl => pl.Id == id);
            if (playList == null)
            {
                _logger.LogWarning($"{nameof(InternalGetPlayList)}: PlaylistId = {id} doesn't exists");
                OnServerMessage?.Invoke(AppMessageType.PlayListNotFound);
                throw new PlayListNotFoundException($"PlayListId = {id} does not exist");
            }

            return playList;
        }
    }
}
