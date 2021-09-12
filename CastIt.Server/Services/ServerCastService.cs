using AutoMapper;
using CastIt.Application.Common.Extensions;
using CastIt.Application.Common.Utils;
using CastIt.Application.Interfaces;
using CastIt.Application.Server;
using CastIt.Domain.Dtos.Responses;
using CastIt.Domain.Enums;
using CastIt.Domain.Exceptions;
using CastIt.Domain.Extensions;
using CastIt.Domain.Models;
using CastIt.Domain.Models.FFmpeg.Info;
using CastIt.GoogleCast.Interfaces;
using CastIt.Infrastructure.Models;
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
    public partial class ServerCastService : BaseCastService, IServerCastService
    {
        #region Members
        private readonly IAppDataService _appDataService;
        private readonly IMapper _mapper;
        private readonly IImageProviderService _imageProviderService;

        private readonly Dictionary<Range<long>, byte[]> _previewThumbnails = new Dictionary<Range<long>, byte[]>();
        private readonly List<FileThumbnailRangeResponseDto> _thumbnailRanges = new List<FileThumbnailRangeResponseDto>();

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
                CurrentPlayedFile.UpdateFileInfo(CurrentFileInfo, Player.CurrentMediaDuration);
            }

            ServerService.OnFileLoaded?.Invoke(GetCurrentPlayedFile());
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
                return ServerService.GetChromeCastPreviewUrl(path);
            }

            return null;
        }

        protected override Task<string> GetSelectedSubtitlePath()
        {
            var path = CurrentPlayedFile?.CurrentFileSubTitles.Find(f => f.IsSelected)?.Path;
            return Task.FromResult(path);
        }

        public Task PlayFile(ServerFileItem file, bool force = false)
        {
            return PlayFile(file, force, false);
        }

        public Task PlayFile(long playListId, long id, bool force, bool fileOptionsChanged)
        {
            var playList = GetPlayListInternal(playListId);
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
                throw new Domain.Exceptions.FileNotFoundException("The provided file won't be played cause it is null");
            }

            var type = FileService.GetFileType(file.Path);
            if (type.DoesNotExist())
            {
                Logger.LogWarning($"{nameof(PlayFile)}: The provided file = {file.Path} does not exist");
                throw new Domain.Exceptions.FileNotFoundException($"The provided file = {file.Path} does not exist");
            }

            bool fileIsBeingPlayed = file.Id == CurrentPlayedFile?.Id && !force && !fileOptionsChanged && !file.Loop;
            if (fileIsBeingPlayed)
            {
                Logger.LogInformation($"{nameof(PlayFile)}: The provided file = {file.Path} is already being played");
                throw new InvalidRequestException($"The provided file = {file.Path} is already being played", AppMessageType.FileIsAlreadyBeingPlayed);
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
                Logger.LogInformation($"{nameof(PlayFile)}: File = {file.Path} won't be played cause there are any devices available");
                throw new NoDevicesException($"File = {file.Path} won't be played cause there are any devices available");
            }

            if (type.IsUrl() && !NetworkUtils.IsInternetAvailable())
            {
                Logger.LogInformation($"{nameof(PlayFile)}: File = {file.Path} won't be played cause there is no internet connection available");
                throw new InvalidRequestException($"File = {file.Path} won't be played cause there is no internet connection available", AppMessageType.NoInternetConnection);
            }

            var playList = PlayLists.Find(f => f.Id == file.PlayListId);
            if (playList == null)
            {
                Logger.LogInformation($"{nameof(PlayFile)}: File = {file.Path} won't be played cause the playListId = {file.PlayListId} does not exist");
                throw new PlayListNotFoundException($"File = {file.Path} won't be played cause the playListId = {file.PlayListId} does not exist");
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

                bool resume = file.CanStartPlayingFromCurrentPercentage &&
                              !type.IsUrl() &&
                              !force &&
                              (!AppSettings.StartFilesFromTheStart || fileOptionsChanged);
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
                throw;
            }
            finally
            {
                _onSkipOrPrevious = false;
            }
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

            var playList = GetPlayListInternal(CurrentPlayedFile.PlayListId, false);
            if (playList == null)
            {
                Logger.LogInformation(
                    $"{nameof(GoTo)}: PlaylistId = {CurrentPlayedFile.PlayListId} does not exist. " +
                    "It may have been deleted. Playback will stop now");
                await StopPlayback();
                _onSkipOrPrevious = false;
                return;
            }
            var files = playList.Files;
            if (!files.Any())
            {
                Logger.LogInformation(
                    $"{nameof(GoTo)}: PlaylistId = {CurrentPlayedFile.PlayListId} does not have any file to play. " +
                    "Playback will stop now");
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
                    $"{nameof(GoTo)}: File = {CurrentPlayedFile.Path} is no longer present in the playlist, " +
                    "it may have been deleted, getting the closest one...");

                int nextPosition = CurrentPlayedFile.Position + increment;
                int closestPosition = files
                    .Select(f => f.Position)
                    .GetClosest(nextPosition);

                var closestFile = files.Find(f => f.Position == closestPosition);

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
                Logger.LogInformation(
                    $"{nameof(GoTo)}: Since no file was found and playlist is not marked to loop, " +
                    "the playback of this playlist will end here");
                await StopPlayback();
                _onSkipOrPrevious = false;
                return;
            }

            Logger.LogInformation($"{nameof(GoTo)}: Looping playlistId = {playList.Id}");
            await PlayFile(files.FirstOrDefault());
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
            file.ThumbnailUrl = CurrentThumbnailUrl;
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
                update = file.Exists && File.GetLastAccessTime(file.Path) < maxDate && file.UpdatedAt <= maxDate;
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

                var (range, imgBytes) = _previewThumbnails.FirstOrDefault(kvp => kvp.Key.ContainsValue(tentativeSecond));

                if (imgBytes != null)
                {
                    return imgBytes;
                }

                string path = CurrentPlayedFile?.Path;
                if (string.IsNullOrWhiteSpace(path) || tentativeSecond > CurrentPlayedFile?.TotalSeconds || tentativeSecond < 0 || range == null)
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
                    var range = new Range<long>(min, max + fix, d);
                    _previewThumbnails.Add(range, null);
                    min += AppWebServerConstants.ThumbnailsPerImage;
                    max += AppWebServerConstants.ThumbnailsPerImage;
                }
            }
            else
            {
                long max = CurrentPlayedFile.TotalSeconds > 0 ? (long)CurrentPlayedFile.TotalSeconds : long.MaxValue;
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
                if (!CurrentPlayedFile.Type.IsLocalVideo())
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

            return PlayFile(CurrentPlayedFile, false, true);
        }
    }
}
