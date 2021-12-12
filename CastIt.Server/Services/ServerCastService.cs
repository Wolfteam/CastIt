using AutoMapper;
using CastIt.Domain;
using CastIt.Domain.Dtos.Responses;
using CastIt.Domain.Enums;
using CastIt.Domain.Exceptions;
using CastIt.Domain.Extensions;
using CastIt.Domain.Models;
using CastIt.Domain.Models.FFmpeg.Info;
using CastIt.Domain.Utils;
using CastIt.FFmpeg;
using CastIt.GoogleCast.Interfaces;
using CastIt.GoogleCast.Youtube;
using CastIt.Server.Common.Extensions;
using CastIt.Server.Interfaces;
using CastIt.Shared.FilePaths;
using CastIt.Shared.Models;
using CastIt.Shared.Telemetry;
using CastIt.Youtube;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CastIt.Server.Services
{
    public partial class ServerCastService : BaseCastService, IServerCastService
    {
        #region Members

        private readonly IYoutubeUrlDecoder _youtubeUrlDecoder;
        private readonly IAppDataService _appDataService;
        private readonly IMapper _mapper;
        private readonly IImageProviderService _imageProviderService;
        private readonly IMediaRequestGeneratorFactory _mediaRequestGeneratorFactory;

        private readonly Dictionary<Range<long>, byte[]> _previewThumbnails = new Dictionary<Range<long>, byte[]>();

        private readonly List<FileThumbnailRangeResponseDto> _thumbnailRanges =
            new List<FileThumbnailRangeResponseDto>();

        private bool _onSkipOrPrevious;

        public CancellationTokenSource FileCancellationTokenSource { get; } = new CancellationTokenSource();
        private readonly SemaphoreSlim _thumbnailSemaphoreSlim = new SemaphoreSlim(1, 1);

        #endregion

        #region Properties

        public List<ServerPlayList> PlayLists { get; } = new List<ServerPlayList>();
        public ServerPlayList CurrentPlayList { get; private set; }
        public ServerFileItem CurrentPlayedFile { get; private set; }

        #endregion

        public ServerCastService(
            ILogger<ServerCastService> logger,
            IServerService appWebServer,
            IFFmpegService ffmpegService,
            IYoutubeUrlDecoder youtubeUrlDecoder,
            ITelemetryService telemetryService,
            IServerAppSettingsService appSettings,
            IFileService fileService,
            IPlayer player,
            IAppDataService appDataService,
            IMapper mapper,
            IImageProviderService imageProviderService,
            IMediaRequestGeneratorFactory mediaRequestGeneratorFactory)
            : base(logger, appWebServer, ffmpegService, telemetryService, appSettings, fileService,
                player)
        {
            _youtubeUrlDecoder = youtubeUrlDecoder;
            _appDataService = appDataService;
            _mapper = mapper;
            _imageProviderService = imageProviderService;
            _mediaRequestGeneratorFactory = mediaRequestGeneratorFactory;
        }

        public override async Task Init()
        {
            await _appDataService.DeleteOldTinyCodes();
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

        public override async Task StopAsync()
        {
            FileCancellationTokenSource.Cancel();

            Logger.LogInformation($"{nameof(StopAsync)}: Saving the changes made to the play lists + files...");
            var files = PlayLists.SelectMany(pl => pl.Files)
                .Where(f => f.WasPlayed || f.PositionChanged)
                .ToList();
            await _appDataService.SavePlayListChanges(PlayLists);
            await _appDataService.SaveFileChanges(files);
            _appDataService.Close();

            Logger.LogInformation($"{nameof(StopAsync)}: Changes were saved");
            await base.StopAsync();
        }

        protected override void FileLoading()
        {
            if (CurrentPlayedFile != null)
                ServerService.OnFileLoading?.Invoke(GetCurrentPlayedFile());
        }

        protected override void FileLoaded(object sender, EventArgs e)
        {
            if (CurrentPlayedFile == null)
                return;

            if (CurrentPlayedFile.IsUrlFile)
            {
                CurrentPlayedFile.UpdateFileInfo(CurrentRequest.FileInfo, Player.CurrentMediaDuration);
            }

            ServerService.OnFileLoaded?.Invoke(GetCurrentPlayedFile());
        }

        protected override Task OnQualitiesLoaded(int selectedQuality, List<int> qualities)
        {
            CurrentPlayedFile?.SetQualitiesStreams(selectedQuality, qualities);
            return Task.CompletedTask;
        }

        protected override Task<string> GetSelectedSubtitlePath()
        {
            var path = CurrentPlayedFile?.CurrentFileSubTitles.Find(f => f.IsSelected)?.Path;
            return Task.FromResult(path);
        }

        public Task PlayFile(string fileName, bool force, bool isAnAutomaticCall)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                Logger.LogWarning($"{nameof(PlayFile)}: The provided filename is null");
                throw new InvalidRequestException("The filename cannot be null");
            }
            var file = PlayLists
                .SelectMany(s => s.Files)
                .FirstOrDefault(s => !string.IsNullOrWhiteSpace(s.Name) &&
                                     s.Name.Contains(fileName.Trim(), StringComparison.OrdinalIgnoreCase));
            return PlayFile(file, force, isAnAutomaticCall);
        }

        public Task PlayFile(ServerFileItem file, bool force = false, bool isAnAutomaticCall = false)
        {
            return PlayFile(file, force, false, isAnAutomaticCall);
        }

        public Task PlayFile(long playListId, long id, bool force, bool fileOptionsChanged, bool isAnAutomaticCall)
        {
            var playList = GetPlayListInternal(playListId);
            var file = playList.Files.Find(f => f.Id == id);
            if (file == null)
            {
                Logger.LogWarning($"{nameof(PlayFile)}: FileId = {id} was not found");
            }
            return PlayFile(file, force, fileOptionsChanged, isAnAutomaticCall);
        }

        public async Task PlayFile(ServerFileItem file, bool force, bool fileOptionsChanged, bool isAnAutomaticCall)
        {
            if (file == null)
            {
                string msg = "The provided file won't be played cause it is null";
                Logger.LogWarning($"{nameof(PlayFile)}: {msg}");
                throw new Domain.Exceptions.FileNotFoundException(msg);
            }

            var playList = PlayLists.Find(f => f.Id == file.PlayListId);
            if (playList == null)
            {
                Logger.LogInformation(
                    $"{nameof(PlayFile)}: File = {file.Path} won't be played cause " +
                    $"the playListId = {file.PlayListId} does not exist");
                return;
            }

            AppFileType type = FileService.GetFileType(file.Path);
            try
            {
                if (type.DoesNotExist())
                {
                    string msg = $"The provided file = {file.Path} does not exist";
                    Logger.LogInformation($"{nameof(PlayFile)}: {msg}");
                    throw new Domain.Exceptions.FileNotFoundException(msg);
                }

                bool fileIsBeingPlayed = file.Id == CurrentPlayedFile?.Id && !force && !fileOptionsChanged && !file.Loop;
                if (fileIsBeingPlayed)
                {
                    string msg = $"The provided file = {file.Path} is already being played";
                    Logger.LogInformation($"{nameof(PlayFile)}: {msg}");
                    throw new InvalidRequestException(msg, AppMessageType.FileIsAlreadyBeingPlayed);
                }

                if (string.IsNullOrEmpty(file.Duration))
                {
                    Logger.LogInformation(
                        $"{nameof(PlayFile)}: Cant play file = {file.Filename} yet, " +
                        "because im still setting the duration for some files.");
                    throw new FileNotReadyException($"File = {file.Filename} is not ready yet");
                }

                if (!AvailableDevices.Any())
                {
                    string msg = $"File = {file.Path} won't be played cause there are any devices available";
                    Logger.LogInformation($"{nameof(PlayFile)}: {msg}");
                    throw new NoDevicesException(msg);
                }

                if (type.IsUrl() && !NetworkUtils.IsInternetAvailable())
                {
                    string msg = $"File = {file.Path} won't be played cause there is no internet connection available";
                    Logger.LogInformation($"{nameof(PlayFile)}: {msg}");
                    throw new InvalidRequestException(msg, AppMessageType.NoInternetConnection);
                }

                bool canHandleFile = await _mediaRequestGeneratorFactory.CanHandleRequest(file.Path, type);
                if (!canHandleFile)
                {
                    string msg = $"File = {file.Path} cannot be handle by this app";
                    Logger.LogInformation($"{nameof(PlayFile)}: {msg}");
                    throw new InvalidRequestException(msg);
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e,
                    $"{nameof(PlayFile)}: A validation error occurred, " +
                    $"isAnAutomaticCall = {isAnAutomaticCall}");
                if (!isAnAutomaticCall)
                    throw;

                if (e is Domain.Exceptions.FileNotFoundException)
                {
                    bool allWereNotFound = playList.Files.Count == 0 ||
                                           playList.Files.All(f => FileService.GetFileType(f.Path).DoesNotExist());
                    if (allWereNotFound)
                    {
                        Logger.LogInformation(
                            $"{nameof(PlayFile)}: All the files inside playListId = {playList.Id} were not found, " +
                            "there's nothing to play here");
                        await StopPlayback();
                        return;
                    }

                    await GoTo(file.PlayListId, file.Id, file.Position, true, true, true);
                    return;
                }

                e.HandleCastException(this, TelemetryService);
                await StopPlayback();
                return;
            }

            try
            {
                bool differentFile = CurrentPlayedFile?.Id != file.Id;
                if (differentFile)
                {
                    CleanupBeforePlaying();
                }
                CurrentPlayList = playList;
                CurrentPlayedFile?.BeingPlayed(false);
                CurrentPlayedFile = file;
                FileLoading();
                await StopPlayback(false, false);
                DisableLoopForAllFiles(file.Id);

                CurrentPlayedFile = file.BeingPlayed();

                bool resume = file.CanStartPlayingFromCurrentPercentage &&
                              !type.IsUrl() &&
                              !force &&
                              (!AppSettings.StartFilesFromTheStart || fileOptionsChanged);

                double seekSeconds = !resume ? 0 : file.GetSecondsToSeek();
                var request = await _mediaRequestGeneratorFactory.BuildRequest(
                    file, AppSettings.Settings,
                    seekSeconds, fileOptionsChanged, FileCancellationTokenSource.Token);

                if (differentFile)
                {
                    GenerateThumbnailRanges(type, CurrentPlayedFile.TotalSeconds);
                }

                if (type.IsLocalVideo() && !fileOptionsChanged)
                {
                    Logger.LogInformation($"{nameof(PlayFile)}: Setting the sub streams...");
                    var (localSubsPath, filename) = TryGetSubTitlesLocalPath(CurrentPlayedFile.Path);
                    CurrentPlayedFile.SetSubtitleStreams(localSubsPath, filename, AppSettings.LoadFirstSubtitleFoundAutomatically);
                }

                if (request is YoutubePlayMediaRequest ytRequest)
                {
                    await OnQualitiesLoaded(ytRequest.SelectedQuality, ytRequest.Qualities);
                }

                if (request.NeedsTinyCode)
                {
                    string tinyCode = await _appDataService.GenerateTinyCode(request.Base64);
                    request.MediaInfo.ContentId = ServerService.GetPlayUrl(tinyCode);
                }

                if (resume)
                {
                    Logger.LogInformation($"{nameof(PlayFile)}: File will be resumed from = {file.PlayedPercentage}% ...");
                    await GoToPosition(request);
                }
                else
                {
                    Logger.LogInformation($"{nameof(PlayFile)}: Playing file from the start...");
                    await StartPlay(request);
                }

                RefreshPlayListImage(CurrentPlayList);

                Logger.LogInformation($"{nameof(PlayFile)}: File is being played...");
            }
            catch (Exception e)
            {
                if (!(e is BaseAppException))
                {
                    Logger.LogError(e, $"{nameof(PlayFile)}: An unknown error occurred");
                }
                await StopPlayback();
                if (!isAnAutomaticCall)
                {
                    throw;
                }
            }
            finally
            {
                _onSkipOrPrevious = false;
            }
        }

        public override async Task GoTo(bool nextTrack, bool isAnAutomaticCall = false)
        {
            if (CurrentPlayedFile == null)
                return;

            if (CurrentPlayedFile.Loop)
            {
                _onSkipOrPrevious = true;
                await PlayFile(CurrentPlayedFile, true, false, isAnAutomaticCall);
                _onSkipOrPrevious = false;
                return;
            }

            await GoTo(
                CurrentPlayedFile.PlayListId, CurrentPlayedFile.Id, CurrentPlayedFile.Position,
                nextTrack, isAnAutomaticCall);
        }

        public async Task GoTo(
            long currentPlayListId,
            long currentFileId,
            int currentFilePosition,
            bool nextTrack,
            bool isAnAutomaticCall = false,
            bool force = false)
        {
            if (_onSkipOrPrevious && !force)
            {
                return;
            }

            _onSkipOrPrevious = true;
            var playList = GetPlayListInternal(currentPlayListId, false);
            if (playList == null)
            {
                Logger.LogInformation(
                    $"{nameof(GoTo)}: PlaylistId = {currentPlayListId} does not exist. " +
                    "It may have been deleted. Playback will stop now");
                await StopPlayback();
                _onSkipOrPrevious = false;
                return;
            }
            var files = playList.Files;
            if (!files.Any())
            {
                Logger.LogInformation(
                    $"{nameof(GoTo)}: PlaylistId = {currentPlayListId} does not have any file to play. " +
                    "Playback will stop now");
                await StopPlayback();
                _onSkipOrPrevious = false;
                return;
            }

            Logger.LogInformation($"{nameof(GoTo)}: Getting the next / previous file to play.... Going to next file = {nextTrack}");
            int increment = nextTrack ? 1 : -1;
            var fileIndex = files.FindIndex(f => f.Id == currentFileId);
            int newIndex = fileIndex + increment;
            bool random = playList.Shuffle && files.Count > 1;
            if (random)
                Logger.LogInformation($"{nameof(GoTo)}: Random is active for playListId = {playList.Id}, picking a random file ...");

            if (!isAnAutomaticCall && !random && files.Count > 1 && files.ElementAtOrDefault(newIndex) == null)
            {
                Logger.LogInformation(
                    $"{nameof(GoTo)}: The new index = {newIndex} does not exist in the playlist, " +
                    "falling back to the first or last item in the list");
                var nextOrPreviousFile = nextTrack ? files.First() : files.Last();
                await PlayFile(nextOrPreviousFile);
                _onSkipOrPrevious = false;
                return;
            }

            if (fileIndex < 0)
            {
                Logger.LogInformation(
                    $"{nameof(GoTo)}: FileId = {currentFileId} is no longer present in the playlist, " +
                    "it may have been deleted, getting the closest one...");

                int nextPosition = currentFilePosition + increment;
                int closestPosition = files
                    .Select(f => f.Position)
                    .GetClosest(nextPosition);

                var closestFile = files.Find(f => f.Position == closestPosition);

                Logger.LogInformation($"{nameof(GoTo)}: Closest file is = {closestFile?.Path}, trying to play it");
                if (closestFile?.Id != currentFileId)
                    await PlayFile(closestFile, isAnAutomaticCall: isAnAutomaticCall);
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
                await PlayFile(file, isAnAutomaticCall: isAnAutomaticCall);
                _onSkipOrPrevious = false;
                return;
            }
            Logger.LogInformation(
                $"{nameof(GoTo)}: File at index = {fileIndex} in playListId {playList.Id} was not found. " +
                "Probably an end of playlist");

            if (!playList.Loop)
            {
                Logger.LogInformation(
                    $"{nameof(GoTo)}: Since no file was found and playlist is not marked to loop, " +
                    "the playback of this playlist will end here");
                await StopPlayback();
                _onSkipOrPrevious = false;
                return;
            }

            Logger.LogInformation($"{nameof(GoTo)}: Looping playlistId = {playList.Id}");
            await PlayFile(files.FirstOrDefault(), isAnAutomaticCall: isAnAutomaticCall);
            _onSkipOrPrevious = false;
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
            ServerService.OnStoppedPlayback?.Invoke();
        }

        public FileItemResponseDto GetCurrentPlayedFile()
        {
            if (CurrentPlayedFile == null)
                return null;
            var file = _mapper.Map<FileItemResponseDto>(CurrentPlayedFile);
            file.ThumbnailUrl = CurrentRequest?.ThumbnailUrl;
            return file;
        }

        //TODO: MAYBE USE A LOCK TO AVOID PROBLEMS  WHILE UPDATING PLAYLISTS
        public ServerPlayerStatusResponseDto GetPlayerStatus()
        {
            var currentPlayedFile = GetCurrentPlayedFile();
            var currentPlayList = CurrentPlayList != null ? _mapper.Map<GetAllPlayListResponseDto>(CurrentPlayList) : null;
            if (currentPlayList != null && !string.IsNullOrWhiteSpace(currentPlayedFile?.ThumbnailUrl))
            {
                currentPlayList.ImageUrl = currentPlayedFile.ThumbnailUrl;
            }

            return new ServerPlayerStatusResponseDto
            {
                Player = _mapper.Map<PlayerStatusResponseDto>(Player.State),
                PlayList = currentPlayList,
                PlayedFile = currentPlayedFile,
                ThumbnailRanges = _thumbnailRanges
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
                update = file.Exists && System.IO.File.GetLastAccessTime(file.Path) < maxDate && file.UpdatedAt <= maxDate;
            }
            else
            {
                var fileInfo = await GetFileInfo(file.Path, FileCancellationTokenSource.Token);
                file.UpdateFileInfo(fileInfo, fileInfo?.Format?.Duration ?? -1);
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

        public void CleanPlayedFile(bool nullPlayedFile = true)
        {
            CurrentPlayedFile?.BeingPlayed(false);
            if (nullPlayedFile)
            {
                CurrentPlayList = null;
                CurrentPlayedFile = null;
                _previewThumbnails.Clear();
                _thumbnailRanges.Clear();
            }
        }

        public Task SetCurrentPlayedFileOptions(int audioStreamIndex, int subsStreamIndex, int qualityIndex)
        {
            if (CurrentPlayedFile == null)
            {
                Logger.LogWarning(
                    $"{nameof(SetCurrentPlayedFileOptions)}: Options cannot be changed since there isn't a file being played");
                return Task.CompletedTask;
            }

            var audioOptions = CurrentPlayedFile.GetAudioFileOption(audioStreamIndex);
            var subsOptions = CurrentPlayedFile.GetSubsFileOption(subsStreamIndex);
            var qualityOptions = CurrentPlayedFile.GetQualityFileOption(qualityIndex);
            if (audioOptions != null)
                CurrentPlayedFile.SetSelectedFileOption(audioOptions);

            if (subsOptions != null)
                CurrentPlayedFile.SetSelectedFileOption(subsOptions);

            if (qualityOptions != null)
                CurrentPlayedFile.SetSelectedFileOption(qualityOptions);

            return PlayFile(CurrentPlayedFile, false, true, false);
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
                    $"{nameof(SetCurrentPlayedFileOptions)}: Options cannot be changed since there isn't a file being played");
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

            return PlayFile(CurrentPlayedFile, false, true, false);
        }

        public async Task<byte[]> GetClosestPreviewThumbnail(long tentativeSecond)
        {
            try
            {
                await _thumbnailSemaphoreSlim.WaitAsync(FileCancellationTokenSource.Token);

                var (range, imgBytes) = _previewThumbnails.FirstOrDefault(kvp => kvp.Key.ContainsValue(tentativeSecond));

                if (imgBytes != null)
                {
                    return imgBytes;
                }

                string path = CurrentPlayedFile?.Path;
                if (string.IsNullOrWhiteSpace(path) ||
                    tentativeSecond > CurrentPlayedFile?.TotalSeconds ||
                    tentativeSecond < 0 || range == null)
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
                else if (CurrentPlayedFile.Type.IsUrl() && !string.IsNullOrWhiteSpace(CurrentRequest.ThumbnailUrl))
                {
                    //In this case, we download and save the image if needed
                    var pathTo =
                        await FileService.DownloadAndSavePreviewImage(CurrentPlayedFile.Id, CurrentRequest.ThumbnailUrl, false);
                    bytes = await File.ReadAllBytesAsync(pathTo);
                }
                else
                {
                    var fps = CurrentPlayedFile.FileInfo?.Videos.FirstOrDefault()?.AverageFrameRate ?? 24;
                    bytes = await FFmpegService.GetThumbnailTile(CurrentPlayedFile.Path, range.Minimum, fps);

                    //previous range
                    if (_previewThumbnails.Any(kvp => kvp.Key.Index == range.Index - 1 && kvp.Value == null))
                    {
                        var (previousRange, _) = _previewThumbnails.FirstOrDefault(kvp => kvp.Key.Index == range.Index - 1);
                        _previewThumbnails[previousRange] = await FFmpegService.GetThumbnailTile(CurrentPlayedFile.Path, previousRange.Minimum, fps);
                    }

                    //next range
                    if (_previewThumbnails.Any(kvp => kvp.Key.Index == range.Index + 1 && kvp.Value == null))
                    {
                        var (nextRange, _) = _previewThumbnails.FirstOrDefault(kvp => kvp.Key.Index == range.Index + 1);
                        _previewThumbnails[nextRange] = await FFmpegService.GetThumbnailTile(CurrentPlayedFile.Path, nextRange.Minimum, fps);
                    }
                }

                //The bytes are null, that's I set them here
                _previewThumbnails[range] = bytes;
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

        private void GenerateThumbnailRanges(AppFileType type, double totalSeconds)
        {
            _previewThumbnails.Clear();
            _thumbnailRanges.Clear();

            if (type.IsLocalVideo())
            {
                var rangesToGenerate = (int)Math.Ceiling(totalSeconds / AppWebServerConstants.ThumbnailsPerImage);
                var min = 0;
                var max = AppWebServerConstants.ThumbnailsPerImage;
                for (int d = 0; d < rangesToGenerate; d++)
                {
                    var fix = d == rangesToGenerate - 1 ? 0 : -1;
                    var range = new Range<long>(min, max + fix, d);
                    _previewThumbnails.Add(range, null);
                    min += AppWebServerConstants.ThumbnailsPerImage;
                    max += AppWebServerConstants.ThumbnailsPerImage;
                }
            }
            else
            {
                long max = totalSeconds > 0 ? (long)totalSeconds : long.MaxValue;
                var range = new Range<long>(0, max, 0);
                _previewThumbnails.Add(range, null);
            }

            _thumbnailRanges.AddRange(_previewThumbnails.Select(t => new FileThumbnailRangeResponseDto
            {
                ThumbnailRange = t.Key
            }));

            foreach (var thumbnailRange in _thumbnailRanges)
            {
                thumbnailRange.PreviewThumbnailUrl = ServerService.GetThumbnailPreviewUrl(thumbnailRange.ThumbnailRange.Minimum);
                //there's no point in generating the thumbnail matrix for other file types, since the image will be the same
                if (!type.IsLocalVideo())
                    continue;
                thumbnailRange.SetMatrixOfSeconds(AppWebServerConstants.ThumbnailsPerImageRow);
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

            return PlayFile(CurrentPlayedFile, false, true, false);
        }
    }
}
