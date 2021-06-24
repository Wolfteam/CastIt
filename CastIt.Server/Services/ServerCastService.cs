using AutoMapper;
using CastIt.Application.Common;
using CastIt.Application.Common.Extensions;
using CastIt.Application.Common.Utils;
using CastIt.Application.Interfaces;
using CastIt.Application.Server;
using CastIt.Domain.Dtos.Responses;
using CastIt.Domain.Entities;
using CastIt.Domain.Enums;
using CastIt.Domain.Exceptions;
using CastIt.Domain.Extensions;
using CastIt.Domain.Models;
using CastIt.Domain.Models.FFmpeg.Info;
using CastIt.GoogleCast.Interfaces;
using CastIt.Infrastructure.Models;
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

    public delegate void OnFilesAddedHandler(long playListId, params FileItem[] files);

    public class ServerCastService : BaseCastService, IServerCastService
    {
        #region Members
        private readonly IAppDataService _appDataService;
        private readonly IMapper _mapper;
        private readonly IImageProviderService _imageProviderService;

        private readonly Dictionary<Range<long>, byte[]> _previewThumbnails = new Dictionary<Range<long>, byte[]>();
        private readonly List<FileThumbnailRangeResponseDto> _thumbnailRanges = new List<FileThumbnailRangeResponseDto>();

        private bool _onSkipOrPrevious;

        public CancellationTokenSource FileCancellationTokenSource { get; } = new CancellationTokenSource();
        private readonly SemaphoreSlim _thumbnailSemaphoreSlim = new SemaphoreSlim(3, 3);
        #endregion

        #region Delegates
        public OnFilesAddedHandler OnFilesAdded { get; set; }
        public OnFileLoadingOrLoadedHandler OnFileLoading { get; set; }
        public OnFileLoadingOrLoadedHandler OnFileLoaded { get; set; }
        public OnStoppedPlayback OnStoppedPlayback { get; set; }
        #endregion

        #region Properties
        public List<ServerPlayList> PlayLists { get; } = new List<ServerPlayList>();

        public ServerPlayList CurrentPlayList { get; private set; }
        public ServerFileItem CurrentPlayedFile { get; private set; }
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
            IMapper mapper,
            IImageProviderService imageProviderService)
            : base(logger, appWebServer, ffmpegService, youtubeUrlDecoder, telemetryService, appSettings, fileService, player)
        {
            _appDataService = appDataService;
            _mapper = mapper;
            _imageProviderService = imageProviderService;
        }

        public override async Task Init()
        {
            var playLists = await _appDataService.GetAllPlayLists();
            var tasks = playLists.ConvertAll(async pl =>
            {
                var files = await _appDataService.GetAllFiles(pl.Id);
                var mapped = _mapper.Map<ServerPlayList>(pl);
                mapped.Files = files
                    .Select(f => ServerFileItem.From(FileService, f))
                    .OrderBy(f => f.Position)
                    .ToList();
                return mapped;
            });

            var mappedPlayLists = await Task.WhenAll(tasks);

            PlayLists.AddRange(mappedPlayLists.OrderBy(pl => pl.Position));
            await base.Init();
        }

        public override async Task CleanThemAll()
        {
            FileCancellationTokenSource.Cancel();

            Logger.LogInformation($"{nameof(CleanThemAll)}: Saving the changes made to the play lists + files...");
            var files = PlayLists.SelectMany(pl => pl.Files)
                .Where(f => f.WasPlayed || f.PositionChanged)
                .ToList();
            await _appDataService.SavePlayListChanges(PlayLists);
            await _appDataService.SaveFileChanges(files);
            _appDataService.Close();

            Logger.LogInformation($"{nameof(CleanThemAll)}: Changes were saved");
            await base.CleanThemAll();
        }

        public Task PlayFile(ServerFileItem file, bool force = false)
        {
            return PlayFile(file, force, false);
        }

        public Task PlayFile(long playListId, long id, bool force, bool fileOptionsChanged)
        {
            var playList = InternalGetPlayList(playListId);
            var file = playList.Files.Find(f => f.Id == id);
            if (file == null)
            {
                Logger.LogWarning($"{nameof(PlayFile)}: FileId = {id} was not found");
            }
            return PlayFile(file, force, fileOptionsChanged);
        }

        public async Task PlayFile(ServerFileItem file, bool force, bool fileOptionsChanged)
        {
            if (file == null)
            {
                Logger.LogWarning($"{nameof(PlayFile)}: The provided file won't be played cause it is null");
                OnServerMessage?.Invoke(AppMessageType.FileNotFound);
                throw new Domain.Exceptions.FileNotFoundException("The provided file won't be played cause it is null");
            }
            //TODO: REMOVE ALL THE ONSERVERMESSAGE AND HANDLE THOSE IN THE MIDDLEWARE
            //NOT SURE IF I SHOULD REMOVE THESE SINCE THE MIDDLEWARE ISNT INVOKED BY THE HUB OR IS IT ?
            var type = FileService.GetFileType(file.Path);

            if (type.DoesNotExist())
            {
                Logger.LogWarning($"{nameof(PlayFile)}: The provided file = {file.Path} does not exist");
                OnServerMessage?.Invoke(AppMessageType.FileNotFound);
                throw new Domain.Exceptions.FileNotFoundException($"The provided file = {file.Path} does not exist");
            }

            bool fileIsBeingPlayed = file.Id == CurrentPlayedFile?.Id && !force && !fileOptionsChanged && !file.Loop;
            if (fileIsBeingPlayed)
            {
                Logger.LogInformation($"{nameof(PlayFile)}: The provided file = {file.Path} is already being played");
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
                Logger.LogInformation($"{nameof(PlayFile)}: File = {file.Path} won't be played cause there are any devices available");
                OnServerMessage?.Invoke(AppMessageType.NoDevicesFound);
                throw new NoDevicesException($"File = {file.Path} won't be played cause there are any devices available");
            }

            if (type.IsUrl() && !NetworkUtils.IsInternetAvailable())
            {
                Logger.LogInformation($"{nameof(PlayFile)}: File = {file.Path} won't be played cause there is no internet connection available");
                OnServerMessage?.Invoke(AppMessageType.NoInternetConnection);
                throw new InvalidRequestException($"File = {file.Path} won't be played cause there is no internet connection available", AppMessageType.NoInternetConnection);
            }

            var playList = PlayLists.Find(f => f.Id == file.PlayListId);
            if (playList == null)
            {
                Logger.LogInformation($"{nameof(PlayFile)}: File = {file.Path} won't be played cause the playListId = {file.PlayListId} does not exist");
                OnServerMessage?.Invoke(AppMessageType.PlayListNotFound);
                throw new PlayListNotFoundException($"File = {file.Path} won't be played cause the playListId = {file.PlayListId} does not exist");
            }
            //playList.SelectedItem = file;

            try
            {
                bool differentFile = CurrentPlayedFile?.Id != file.Id;
                CurrentPlayList = playList;
                CurrentPlayedFile?.BeingPlayed(false);
                CurrentPlayedFile = file;
                FileLoading();
                await StopPlayback(false, false);
                DisableLoopForAllFiles(file.Id);
                var fileInfo = await GetFileInfo(file.Path, FileCancellationTokenSource.Token);
                if (fileInfo == null)
                {
                    var msg = $"Couldn't retrieve the file info for file = {file.Filename}";
                    Logger.LogWarning($"{nameof(PlayFile)}: {msg}");
                    throw new ErrorLoadingFileException(msg);
                }

                Logger.LogInformation($"{nameof(PlayFile)}: Setting video, audios and subs streams...");
                CurrentPlayedFile = file
                    .BeingPlayed()
                    .UpdateFileInfo(fileInfo);

                if (differentFile)
                {
                    GenerateThumbnailRanges();
                }

                //This should only be called if we are changing the current played file
                if (!fileOptionsChanged)
                {
                    Logger.LogInformation($"{nameof(PlayFile)}: Setting the video and audio streams...");
                    CurrentPlayedFile
                        .CleanAllStreams()
                        .SetVideoStreams()
                        .SetAudioStreams();
                }

                if (CurrentPlayedFile.Type.IsLocalVideo() && !fileOptionsChanged)
                {
                    Logger.LogInformation($"{nameof(PlayFile)}: Setting the sub streams...");
                    var (localSubsPath, filename) = TryGetSubTitlesLocalPath(CurrentPlayedFile.Path);
                    CurrentPlayedFile.SetSubtitleStreams(localSubsPath, filename, AppSettings.LoadFirstSubtitleFoundAutomatically);
                }

                bool resume = file.CanStartPlayingFromCurrentPercentage && !type.IsUrl() && !force && !AppSettings.StartFilesFromTheStart;
                if (resume)
                {
                    Logger.LogInformation($"{nameof(PlayFile)}: File will be resumed from = {file.PlayedPercentage}% ...");
                    await GoToPosition(
                        file.Path,
                        file.CurrentFileVideoStreamIndex,
                        file.CurrentFileAudioStreamIndex,
                        file.CurrentFileSubTitleStreamIndex,
                        file.CurrentFileQuality,
                        file.PlayedPercentage,
                        file.TotalSeconds,
                        fileInfo);
                }
                else
                {
                    Logger.LogInformation($"{nameof(PlayFile)}: Playing file from the start...");
                    await StartPlay(
                        file.Path,
                        file.CurrentFileVideoStreamIndex,
                        file.CurrentFileAudioStreamIndex,
                        file.CurrentFileSubTitleStreamIndex,
                        file.CurrentFileQuality,
                        fileInfo);
                }

                Logger.LogInformation($"{nameof(PlayFile)}: File is being played...");
            }
            catch (Exception e)
            {
                await HandlePlayException(e);
            }
            finally
            {
                _onSkipOrPrevious = false;
            }
        }

        protected override void FileLoading()
        {
            if (CurrentPlayedFile != null)
                OnFileLoading?.Invoke(GetCurrentPlayedFile());
        }

        protected override void FileLoaded(object sender, EventArgs e)
        {
            if (CurrentPlayedFile == null)
                return;

            if (CurrentPlayedFile.IsUrlFile)
            {
                CurrentPlayedFile.UpdateFileInfo(CurrentFileInfo, Player.CurrentMediaDuration);
            }
            OnFileLoaded?.Invoke(GetCurrentPlayedFile());
        }

        protected override Task OnQualitiesLoaded(int selectedQuality, List<int> qualities)
        {
            CurrentPlayedFile?.SetQualitiesStreams(selectedQuality, qualities);
            return Task.CompletedTask;
        }

        protected override async Task<string> SavePreviewImageFromUrl(string url)
        {
            if (CurrentPlayedFile != null)
            {
                var path = await FileService.DownloadAndSavePreviewImage(CurrentPlayedFile.Id, url);
                return AppWebServer.GetChromeCastPreviewUrl(path);
            }

            return null;
        }

        protected override Task<string> GetSelectedSubtitlePath()
        {
            var path = CurrentPlayedFile?.CurrentFileSubTitles.Find(f => f.IsSelected)?.Path;
            return Task.FromResult(path);
        }

        public FileItemResponseDto GetCurrentPlayedFile()
        {
            if (CurrentPlayedFile == null)
                return null;
            var file = _mapper.Map<FileItemResponseDto>(CurrentPlayedFile);
            file.ThumbnailUrl = CurrentThumbnailUrl;
            return file;
        }

        public override async Task GoTo(bool nextTrack, bool isAnAutomaticCall = false)
        {
            if (CurrentPlayedFile == null || _onSkipOrPrevious)
                return;

            _onSkipOrPrevious = true;

            if (CurrentPlayedFile.Loop)
            {
                await PlayFile(CurrentPlayedFile, true, false);
                _onSkipOrPrevious = false;
                return;
            }

            var playList = InternalGetPlayList(CurrentPlayedFile.PlayListId, false);
            if (playList == null)
            {
                Logger.LogInformation($"{nameof(GoTo)}: PlaylistId = {CurrentPlayedFile.PlayListId} does not exist. It may have been deleted. Playback will stop now");
                await StopPlayback();
                _onSkipOrPrevious = false;
                return;
            }
            var files = playList.Files;
            if (!files.Any())
            {
                Logger.LogInformation($"{nameof(GoTo)}: PlaylistId = {CurrentPlayedFile.PlayListId} does not have any file to play. Playback will stop now");
                await StopPlayback();
                _onSkipOrPrevious = false;
                return;
            }

            Logger.LogInformation($"{nameof(GoTo)}: Getting the next / previous file to play.... Going to next file = {nextTrack}");
            int increment = nextTrack ? 1 : -1;
            var fileIndex = files.FindIndex(f => f.Id == CurrentPlayedFile.Id);
            int newIndex = fileIndex + increment;
            bool random = playList.Shuffle && files.Count > 1;
            if (random)
                Logger.LogInformation($"{nameof(GoTo)}: Random is active for playListId = {playList.Id}, picking a random file ...");

            if (!isAnAutomaticCall && !random && files.Count > 1 && files.ElementAtOrDefault(newIndex) == null)
            {
                Logger.LogInformation($"{nameof(GoTo)}: The new index = {newIndex} does not exist in the playlist, falling back to the first or last item in the list");
                var nextOrPreviousFile = nextTrack ? files.First() : files.Last();
                await PlayFile(nextOrPreviousFile);
                _onSkipOrPrevious = false;
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
                _onSkipOrPrevious = false;
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
                _onSkipOrPrevious = false;
                return;
            }
            Logger.LogInformation(
                $"{nameof(GoTo)}: File at index = {fileIndex} in playListId {playList.Id} was not found. " +
                "Probably an end of playlist");

            if (!playList.Loop)
            {
                Logger.LogInformation($"{nameof(GoTo)}: Since no file was found and playlist is not marked to loop, the playback of this playlist will end here");
                await StopPlayback();
                _onSkipOrPrevious = false;
                return;
            }

            Logger.LogInformation($"{nameof(GoTo)}: Looping playlistId = {playList.Id}");
            await PlayFile(files.FirstOrDefault());
            _onSkipOrPrevious = false;
        }

        //TODO: MAYBE USE A LOCK TO AVOID PROBLEMS  WHILE UPDATING PLAYLISTS

        public PlayListItemResponseDto GetPlayList(long playListId)
        {
            var playList = InternalGetPlayList(playListId);
            RefreshPlayListImage(playList);
            var mapped = _mapper.Map<PlayListItemResponseDto>(playList);
            return mapped;
        }

        public async Task UpdatePlayList(long playListId, string newName)
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

            await _appDataService.UpdatePlayList(playListId, newName, playList.Position);
            SendPlayListChanged(_mapper.Map<GetAllPlayListResponseDto>(playList));
        }

        public void UpdatePlayListPosition(long playListId, int newIndex)
        {
            var playList = InternalGetPlayList(playListId, false);
            if (playList == null || PlayLists.Count - 1 < newIndex)
            {
                Logger.LogWarning($"{nameof(UpdatePlayListPosition)}: PlaylistId = {playListId} doesn't exist or the newIndex = {newIndex} is not valid");
                return;
            }
            var currentIndex = PlayLists.IndexOf(playList);

            Logger.LogInformation($"{nameof(UpdatePlayListPosition)}: Moving playlist from index = {currentIndex} to newIndex = {newIndex}");
            PlayLists.RemoveAt(currentIndex);
            PlayLists.Insert(newIndex, playList);

            for (int i = 0; i < PlayLists.Count; i++)
            {
                var item = PlayLists[i];
                item.Position = i;
            }
            SendPlayListsChanged(_mapper.Map<List<GetAllPlayListResponseDto>>(PlayLists));
        }

        public void UpdateFilePosition(long playListId, long id, int newIndex)
        {
            var playList = InternalGetPlayList(playListId, false);
            if (playList == null)
            {
                Logger.LogWarning($"{nameof(UpdateFilePosition)}: PlaylistId = {playListId} doesn't exist");
                return;
            }

            var file = playList.Files.Find(f => f.Id == id);
            if (file == null || playList.Files.Count - 1 < newIndex)
            {
                Logger.LogWarning($"{nameof(UpdateFilePosition)}: FileId = {id} doesn't exist or the newIndex = {newIndex} is not valid");
                return;
            }

            var currentIndex = playList.Files.IndexOf(file);

            Logger.LogInformation($"{nameof(UpdatePlayListPosition)}: Moving file from index = {currentIndex} to newIndex = {newIndex}");
            playList.Files.RemoveAt(currentIndex);
            playList.Files.Insert(newIndex, file);
            SetFilePositionIfChanged(playListId);
        }

        public async Task RemoveFilesThatStartsWith(long playListId, string path)
        {
            var separators = new[] { '/', '\\' };
            var playlist = InternalGetPlayList(playListId);
            var fileIds = playlist.Files
                .Where(f =>
                {
                    if (string.IsNullOrWhiteSpace(f.Path))
                        return false;
                    var filePath = string.Concat(f.Path.Split(separators, StringSplitOptions.RemoveEmptyEntries));
                    var startsWith = string.Concat(path.Split(separators, StringSplitOptions.RemoveEmptyEntries));
                    return filePath.StartsWith(startsWith, StringComparison.OrdinalIgnoreCase);
                })
                .Select(f => f.Id)
                .ToArray();
            if (!fileIds.Any())
                return;

            await RemoveFiles(playListId, fileIds);
        }

        public async Task RemoveAllMissingFiles(long playListId)
        {
            var playList = InternalGetPlayList(playListId);

            var fileIds = playList.Files.Where(f => !f.Exists).Select(f => f.Id).ToArray();
            if (!fileIds.Any())
                return;

            await RemoveFiles(playListId, fileIds);
        }

        public async Task RemoveFiles(long playListId, params long[] ids)
        {
            SendPlayListBusy(playListId, true);
            var playList = InternalGetPlayList(playListId);

            await _appDataService.DeleteFiles(ids.ToList());
            playList.Files.RemoveAll(f => ids.Contains(f.Id));
            foreach (var id in ids)
            {
                SendFileDeleted(playListId, id);
            }
            await AfterRemovingFiles(playListId);
            SendPlayListBusy(playListId, false);
        }

        public Task<List<GetAllPlayListResponseDto>> GetAllPlayLists()
        {
            RefreshPlayListImages();
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

            SendPlayListAdded(_mapper.Map<GetAllPlayListResponseDto>(serverPlayList));
            return _mapper.Map<PlayListItemResponseDto>(serverPlayList);
        }

        public async Task DeletePlayList(long playListId)
        {
            await _appDataService.DeletePlayList(playListId);
            PlayLists.RemoveAll(pl => pl.Id == playListId);
            SendPlayListDeleted(playListId);
            //InitializeOrUpdateFileWatcher(true);
        }

        public async Task DeleteAllPlayLists(long exceptId = -1)
        {
            if (PlayLists.Count <= 1)
                return;
            var ids = PlayLists
                .Where(pl => pl.Id != exceptId)
                .Select(pl => pl.Id)
                .ToList();
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
            var playList = InternalGetPlayList(playListId, false);
            if (playList == null)
            {
                return;
            }

            if ((sortBy == SortModeType.DurationAsc || sortBy == SortModeType.DurationDesc) &&
                playList.Files.Any(f => string.IsNullOrEmpty(f.Duration)))
            {
                OnServerMessage?.Invoke(AppMessageType.OneOrMoreFilesAreNotReadyYet);
                return;
            }

            playList.Files.Sort(new WindowsExplorerComparerForServerFileItem(sortBy));
            SetFilePositionIfChanged(playListId);
        }

        public void SetFilePositionIfChanged(long playListId)
        {
            var playList = InternalGetPlayList(playListId);
            for (int i = 0; i < playList.Files.Count; i++)
            {
                var item = playList.Files[i];
                var newIndex = i + 1;
                if (item.Position == newIndex)
                    continue;
                item.Position = newIndex;
                item.PositionChanged = true;
            }
            SendFilesChanged(_mapper.Map<List<FileItemResponseDto>>(playList.Files));
        }

        private Task AfterRemovingFiles(long playListId)
        {
            var playList = InternalGetPlayList(playListId);
            SetFilePositionIfChanged(playListId);
            SendPlayListChanged(_mapper.Map<GetAllPlayListResponseDto>(playList));
            return Task.CompletedTask;
        }

        public Task AddFolder(long playListId, bool includeSubFolders, string[] folders)
        {
            var files = new List<string>();
            foreach (var folder in folders)
            {
                Logger.LogInformation($"{nameof(AddFolder)}: Getting all the media files from folder = {folder}");
                var filesInDir = Directory.EnumerateFiles(folder, "*.*", includeSubFolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
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

            var serverFileItem = ServerFileItem.From(FileService, file);
            await UpdateFileItem(serverFileItem);
            playList.Files.Add(serverFileItem);
            SendFileAdded(_mapper.Map<FileItemResponseDto>(serverFileItem));
            SendPlayListChanged(_mapper.Map<GetAllPlayListResponseDto>(playList));
        }

        public async Task AddUrl(long playListId, string url, bool onlyVideo)
        {
            if (!NetworkUtils.IsInternetAvailable())
            {
                var msg = $"Can't add file = {url} to playListId = {playListId} cause there's no internet connection";
                Logger.LogInformation($"{nameof(AddUrl)}: {msg}");
                throw new InvalidRequestException(msg, AppMessageType.NoInternetConnection);
            }

            bool isUrlFile = FileService.IsUrlFile(url);
            if (!isUrlFile || !YoutubeUrlDecoder.IsYoutubeUrl(url))
            {
                Logger.LogInformation($"{nameof(AddUrl)}: Url = {url} is not supported");
                OnServerMessage?.Invoke(AppMessageType.UrlNotSupported);
                return;
            }

            try
            {
                SendPlayListBusy(playListId, true);
                Logger.LogInformation($"{nameof(AddUrl)}: Trying to parse url = {url}");
                if (!YoutubeUrlDecoder.IsPlayList(url) || YoutubeUrlDecoder.IsPlayListAndVideo(url) && onlyVideo)
                {
                    Logger.LogInformation(
                        $"{nameof(AddUrl)}: Url is either not a playlist or we just want the video, parsing it...");
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
            finally
            {
                SendPlayListBusy(playListId, false);
            }
        }

        public ServerPlayerStatusResponseDto GetPlayerStatus()
        {
            var mapped = CurrentPlayList != null ? _mapper.Map<GetAllPlayListResponseDto>(CurrentPlayList) : null;
            return new ServerPlayerStatusResponseDto
            {
                Player = _mapper.Map<PlayerStatusResponseDto>(Player.State),
                PlayList = mapped,
                PlayedFile = GetCurrentPlayedFile(),
                ThumbnailRanges = _thumbnailRanges
            };
        }

        //TODO: REMOVE THIS METHOD ?
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

        public Task SetFileInfoForPendingFiles()
        {
            var items = PlayLists.ConvertAll(SetFileInfoForPendingFiles);
            return Task.WhenAll(items);
        }

        public async Task SetFileInfoForPendingFiles(ServerPlayList playList)
        {
            foreach (var item in playList.Files)
            {
                await UpdateFileItem(item, false);
            }
        }

        public async Task UpdateFileItem(ServerFileItem file, bool force = true)
        {
            Logger.LogTrace($"{nameof(UpdateFileItem)}: Checking if we need to update fileId = {file.Id}... Force = {force}");
            bool update = false;
            if (file.IsUrlFile)
            {
                file.UpdateFileInfo(new FFProbeFileInfo
                {
                    Format = new FileInfoFormat()
                }, file.TotalSeconds > 0 ? file.TotalSeconds : -1);
            }
            else if (file.IsCached && !force)
            {
                var maxDate = DateTime.Now.AddDays(-3);
                update = file.Exists && File.GetLastAccessTime(file.Path) < maxDate && file.UpdatedAt <= maxDate;
            }
            else
            {
                var fileInfo = await GetFileInfo(file.Path, FileCancellationTokenSource.Token);
                file.UpdateFileInfo(fileInfo, fileInfo.Format?.Duration ?? -1);
                update = true;
            }

            if (update)
            {
                Logger.LogTrace($"{nameof(UpdateFileItem)}: FileId = {file.Id} will be updated...");
                await _appDataService.UpdateFile(file.Id, file.Filename, file.SubTitle, file.TotalSeconds);
                SendFileChanged(_mapper.Map<FileItemResponseDto>(file));
            }

            Logger.LogTrace($"{nameof(UpdateFileItem)}: FileId = {file.Id} was successfully processed");
        }

        public void SetPlayListOptions(long id, bool loop, bool shuffle)
        {
            var playlist = InternalGetPlayList(id);
            if (playlist.Loop == loop && playlist.Shuffle == shuffle)
            {
                return;
            }
            playlist.Loop = loop;
            playlist.Shuffle = shuffle;
            SendPlayListChanged(_mapper.Map<GetAllPlayListResponseDto>(playlist));
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

        public void DisableLoopForAllFiles(long exceptFileId = -1)
        {
            var files = PlayLists.SelectMany(pl => pl.Files).Where(f => f.Loop && f.Id != exceptFileId).ToList();

            foreach (var file in files.Where(file => file.Loop))
            {
                file.Loop = false;
                SendFileChanged(_mapper.Map<FileItemResponseDto>(file));
            }
        }

        public void RefreshPlayListImages()
        {
            foreach (var playList in PlayLists)
            {
                RefreshPlayListImage(playList);
            }
        }

        public void RefreshPlayListImage(ServerPlayList playList)
        {
            var imageUrl = _imageProviderService.GetPlayListImageUrl(playList, CurrentPlayedFile);
            bool changed = playList.ImageUrl != imageUrl;
            playList.ImageUrl = imageUrl;
            if (changed)
                SendPlayListChanged(_mapper.Map<GetAllPlayListResponseDto>(playList));
        }

        public async Task StopPlayback(bool nullPlayedFile = true, bool disableLoopForAll = true)
        {
            if (IsPlayingOrPaused)
            {
                await base.StopPlayback();
            }

            CleanPlayedFile(nullPlayedFile);
            await StopRunningProcess();
            if (disableLoopForAll)
                DisableLoopForAllFiles();
            OnStoppedPlayback?.Invoke();
        }

        public void CleanPlayedFile(bool nullPlayedFile = true)
        {
            CurrentPlayedFile?.BeingPlayed(false);
            if (nullPlayedFile)
            {
                CurrentPlayList = null;
                CurrentPlayedFile = null;
                _previewThumbnails.Clear();
            }
        }

        public void LoopFile(long playListId, long id, bool loop)
        {
            var playList = InternalGetPlayList(playListId, false);
            var file = playList?.Files.Find(f => f.Id == id);
            if (file == null)
            {
                Logger.LogWarning($"{nameof(LoopFile)}: FileId = {id} associated to playListId = {playListId} does not exist");
                return;
            }

            bool triggerChange = file.Loop != loop;
            file.Loop = loop;

            DisableLoopForAllFiles(file.Id);

            if (triggerChange)
                SendFileChanged(_mapper.Map<FileItemResponseDto>(file));
        }

        public Task SetCurrentPlayedFileOptions(int streamIndex, bool isAudio, bool isSubTitle, bool isQuality)
        {
            if (!isAudio && !isSubTitle && !isQuality)
            {
                Logger.LogWarning(
                    $"{nameof(SetCurrentPlayedFileOptions)}: The provided file options are not valid " +
                    $"for currentPlayedFileId = {CurrentPlayedFile?.Id}");
                return Task.CompletedTask;
            }

            if (CurrentPlayedFile == null)
            {
                Logger.LogWarning(
                    $"{nameof(SetCurrentPlayedFileOptions)}: Options cannot be change since there isn't a file being played");
                return Task.CompletedTask;
            }

            var options = CurrentPlayedFile.GetFileOption(streamIndex, isAudio, isSubTitle);
            return FileOptionsChanged(options);
        }

        public Task SetFileSubtitlesFromPath(string filePath)
        {
            if (CurrentPlayedFile == null)
            {
                return Task.CompletedTask;
            }
            Logger.LogInformation($"{nameof(SetFileSubtitlesFromPath)}: Trying to set subs from path = {filePath}...");
            var (isSub, filename) = FileService.IsSubtitle(filePath);
            if (!isSub || CurrentPlayedFile.CurrentFileSubTitles.Any(f => f.Text == filename))
            {
                Logger.LogInformation($"{nameof(SetFileSubtitlesFromPath)}: Subtitle = {filePath} is not valid or is already in the current sub files");
                return Task.CompletedTask;
            }

            foreach (var item in CurrentPlayedFile.CurrentFileSubTitles)
            {
                item.IsSelected = false;
                if (item.Id == AppWebServerConstants.NoStreamSelectedId)
                    item.IsEnabled = true;
            }

            CurrentPlayedFile.CurrentFileSubTitles.Add(new FileItemOptionsResponseDto
            {
                Id = CurrentPlayedFile.CurrentFileSubTitles.Min(f => f.Id) - 1,
                IsSubTitle = true,
                IsSelected = true,
                Text = filename,
                Path = filePath,
                IsEnabled = true
            });

            return PlayFile(CurrentPlayedFile, false, true);
        }

        public async Task<byte[]> GetClosestPreviewThumbnail(long tentativeSecond)
        {
            try
            {
                await _thumbnailSemaphoreSlim.WaitAsync(FileCancellationTokenSource.Token);

                var belongsToRange = _previewThumbnails.FirstOrDefault(kvp => kvp.Key.ContainsValue(tentativeSecond));

                if (belongsToRange.Value != null)
                {
                    return belongsToRange.Value;
                }

                string path = CurrentPlayedFile?.Path;
                if (string.IsNullOrWhiteSpace(path) || tentativeSecond > CurrentPlayedFile?.TotalSeconds || tentativeSecond < 0)
                {
                    var pathTo = _imageProviderService.GetNoImagePath();
                    return await File.ReadAllBytesAsync(pathTo);
                }

                if ((CurrentPlayedFile.Type.IsLocalMusic() || CurrentPlayedFile.Type.IsUrl()) &&
                    _previewThumbnails.Any(kvp => kvp.Value != null))
                {
                    return _previewThumbnails.First().Value;
                }

                byte[] bytes;
                if (CurrentPlayedFile.Type.IsLocalMusic())
                {
                    //Images for music files don't change, that's why we return the original image here
                    var pathTo = FFmpegService.GetThumbnail(CurrentPlayedFile.Path);
                    bytes = await File.ReadAllBytesAsync(pathTo);
                }
                else if (CurrentPlayedFile.Type.IsUrl() && !string.IsNullOrWhiteSpace(CurrentThumbnailUrl))
                {
                    //In this case, we download and save the image if needed
                    var pathTo =
                        await FileService.DownloadAndSavePreviewImage(CurrentPlayedFile.Id, CurrentThumbnailUrl, false);
                    bytes = await File.ReadAllBytesAsync(pathTo);
                }
                else
                {
                    //TODO: maybe spawn 2 extra process to generate thumbnails below and above of what gets generated here
                    var fps = CurrentPlayedFile.FileInfo?.Videos.FirstOrDefault()?.AverageFrameRate ?? 24;
                    bytes = await FFmpegService.GetThumbnailTile(CurrentPlayedFile.Path, belongsToRange.Key.Minimum, fps);
                }

                //The bytes are null, that's I set them here
                _previewThumbnails[belongsToRange.Key] = bytes;
                return bytes;
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"{nameof(GetClosestPreviewThumbnail)}: Unknown error");
            }
            finally
            {
                _thumbnailSemaphoreSlim.Release();
            }

            return null;
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

        private void GenerateThumbnailRanges()
        {
            _previewThumbnails.Clear();
            _thumbnailRanges.Clear();

            if (CurrentPlayedFile.Type.IsLocalVideo())
            {
                var rangesToGenerate = (int)Math.Ceiling(CurrentPlayedFile.TotalSeconds / AppWebServerConstants.ThumbnailsPerImage);
                var min = 0;
                var max = AppWebServerConstants.ThumbnailsPerImage;
                for (int d = 0; d < rangesToGenerate; d++)
                {
                    var fix = d == rangesToGenerate - 1 ? 0 : -1;
                    var range = new Range<long>(min, max + fix);
                    _previewThumbnails.Add(range, null);
                    min += AppWebServerConstants.ThumbnailsPerImage;
                    max += AppWebServerConstants.ThumbnailsPerImage;
                }
            }
            else
            {
                long max = CurrentPlayedFile.TotalSeconds > 0 ? (long)CurrentPlayedFile.TotalSeconds : long.MaxValue;
                var range = new Range<long>(0, max);
                _previewThumbnails.Add(range, null);
            }

            _thumbnailRanges.AddRange(_previewThumbnails.Select(t => new FileThumbnailRangeResponseDto
            {
                ThumbnailRange = t.Key
            }));

            //there's no point in generating the thumbnail matrix for other file types, since the image will be the same
            if (CurrentPlayedFile.Type.IsLocalVideo())
            {
                foreach (var thumbnailRange in _thumbnailRanges)
                {
                    //the +1 is required to make sure we are not in the range limits
                    thumbnailRange.PreviewThumbnailUrl = AppWebServer.GetThumbnailPreviewUrl(thumbnailRange.ThumbnailRange.Minimum);
                    thumbnailRange.SetMatrixOfSeconds(AppWebServerConstants.ThumbnailsPerImageRow);
                }
            }
        }

        private async Task AddYoutubeUrl(long playListId, string url)
        {
            var media = await YoutubeUrlDecoder.Parse(url, null, false);
            if (media == null)
            {
                Logger.LogWarning($"{nameof(AddYoutubeUrl)}: Couldn't parse url = {url}");
                OnServerMessage?.Invoke(AppMessageType.UrlCouldntBeParsed);
                return;
            }
            if (!string.IsNullOrEmpty(media.Title) && media.Title.Length > AppWebServerConstants.MaxCharsPerString)
            {
                media.Title = media.Title.Substring(0, AppWebServerConstants.MaxCharsPerString);
            }
            var playList = InternalGetPlayList(playListId);
            var createdFile = await _appDataService.AddFile(playListId, url, playList.Files.Count + 1, media.Title);
            var serverFileItem = ServerFileItem.From(FileService, createdFile);
            await UpdateFileItem(serverFileItem);
            playList.Files.Add(serverFileItem);
            SendFileAdded(_mapper.Map<FileItemResponseDto>(serverFileItem));
            SendPlayListChanged(_mapper.Map<GetAllPlayListResponseDto>(playList));
        }

        private async Task HandlePlayException(Exception e)
        {
            //playList.SelectedItem = null;
            await StopPlayback();

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

        private Task FileOptionsChanged(FileItemOptionsResponseDto selectedItem)
        {
            if (selectedItem == null)
            {
                Logger.LogWarning($"{nameof(FileOptionsChanged)}: Selected option is null");
                return Task.CompletedTask;
            }

            if (selectedItem.IsSelected)
            {
                Logger.LogInformation($"{nameof(FileOptionsChanged)}: The selected options is already selected");
                return Task.CompletedTask;
            }

            if (CurrentPlayedFile == null)
            {
                Logger.LogWarning($"{nameof(FileOptionsChanged)}: No file is being played");
                return Task.CompletedTask;
            }

            CurrentPlayedFile.SetSelectedFileOption(selectedItem);

            return PlayFile(CurrentPlayedFile, false, true);
        }

    }
}
