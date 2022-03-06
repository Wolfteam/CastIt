using AutoMapper;
using CastIt.Domain;
using CastIt.Domain.Dtos.Requests;
using CastIt.Domain.Dtos.Responses;
using CastIt.Domain.Enums;
using CastIt.Domain.Exceptions;
using CastIt.Domain.Extensions;
using CastIt.Domain.Models;
using CastIt.Domain.Models.FFmpeg.Info;
using CastIt.Domain.Utils;
using CastIt.FFmpeg;
using CastIt.GoogleCast.Interfaces;
using CastIt.GoogleCast.Models.Play;
using CastIt.GoogleCast.Shared.Device;
using CastIt.Server.Common.Extensions;
using CastIt.Server.Interfaces;
using CastIt.Shared.Extensions;
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
    public partial class ServerCastService : IServerCastService
    {
        #region Members

        private readonly ILogger<ServerCastService> _logger;
        private readonly IServerService _server;
        private readonly IFFmpegService _fFmpeg;
        private readonly ITelemetryService _telemetry;
        private readonly IServerAppSettingsService _settings;
        private readonly IFileService _fileService;
        private readonly IPlayer _player;
        private readonly IYoutubeUrlDecoder _youtubeUrlDecoder;
        private readonly IAppDataService _appDataService;
        private readonly IMapper _mapper;
        private readonly IImageProviderService _imageProviderService;
        private readonly IMediaRequestGeneratorFactory _mediaRequestGeneratorFactory;
        private readonly List<ThumbnailRange> _previewThumbnails = new List<ThumbnailRange>();

        private readonly List<FileThumbnailRangeResponseDto> _thumbnailRanges =
            new List<FileThumbnailRangeResponseDto>();

        public CancellationTokenSource FileCancellationTokenSource { get; } = new CancellationTokenSource();

        private bool _renderWasSet;
        private string _currentContentId;
        private bool _connecting;
        private bool _onSkipOrPrevious;
        #endregion

        #region Properties

        public List<IReceiver> AvailableDevices { get; } = new List<IReceiver>();
        public bool IsPlayingOrPaused => _player.IsPlayingOrPaused;
        public PlayMediaRequest CurrentRequest { get; private set; }
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
        {
            _logger = logger;
            _server = appWebServer;
            _fFmpeg = ffmpegService;
            _telemetry = telemetryService;
            _settings = appSettings;
            _fileService = fileService;
            _player = player;
            _youtubeUrlDecoder = youtubeUrlDecoder;
            _appDataService = appDataService;
            _mapper = mapper;
            _imageProviderService = imageProviderService;
            _mediaRequestGeneratorFactory = mediaRequestGeneratorFactory;
        }

        public async Task Init()
        {
            _logger.LogInformation($"{nameof(Init)}: Initializing all...");
            await _appDataService.DeleteOldTinyCodes();
            var playLists = await _appDataService.GetAllPlayLists();
            var tasks = playLists.ConvertAll(async pl =>
            {
                var files = await _appDataService.GetAllFiles(pl.Id);
                var mapped = _mapper.Map<ServerPlayList>(pl);
                mapped.Files = files
                    .Select(f => ServerFileItem.From(_fileService, f))
                    .OrderBy(f => f.Position)
                    .ToList();
                return mapped;
            });

            var mappedPlayLists = await Task.WhenAll(tasks);

            PlayLists.AddRange(mappedPlayLists.OrderBy(pl => pl.Position));

            _player.FileLoading += FileLoading;
            _player.FileLoaded += FileLoaded;
            _player.DeviceAdded += RendererDiscovererItemAdded;
            _player.EndReached += EndReached;
            _player.TimeChanged += TimeChanged;
            _player.PositionChanged += PositionChanged;
            _player.Paused += Paused;
            _player.Disconnected += Disconnected;
            _player.VolumeLevelChanged += VolumeLevelChanged;
            _player.IsMutedChanged += IsMutedChanged;
            _player.LoadFailed += LoadFailed;
            _player.ListenForDevices();

            await _imageProviderService.Init();

            _logger.LogInformation($"{nameof(Init)}: Initialize completed");
        }

        protected void FileLoading()
        {
            if (CurrentPlayedFile != null)
                _server.OnFileLoading?.Invoke(GetCurrentPlayedFile());
        }

        protected void FileLoaded(object sender, EventArgs e)
        {
            if (CurrentPlayedFile == null)
                return;

            if (CurrentPlayedFile.IsUrlFile)
            {
                CurrentPlayedFile.UpdateFileInfo(CurrentRequest.FileInfo, _player.CurrentMediaDuration);
            }

            _server.OnFileLoaded?.Invoke(GetCurrentPlayedFile());
        }

        public Task PlayFile(string fileName, bool force, bool isAnAutomaticCall)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                _logger.LogWarning($"{nameof(PlayFile)}: The provided filename is null");
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
                _logger.LogWarning($"{nameof(PlayFile)}: FileId = {id} was not found");
            }
            return PlayFile(file, force, fileOptionsChanged, isAnAutomaticCall);
        }

        public async Task PlayFile(ServerFileItem file, bool force, bool fileOptionsChanged, bool isAnAutomaticCall)
        {
            if (file == null)
            {
                string msg = "The provided file won't be played cause it is null";
                _logger.LogWarning($"{nameof(PlayFile)}: {msg}");
                throw new Domain.Exceptions.FileNotFoundException(msg);
            }

            var playList = PlayLists.Find(f => f.Id == file.PlayListId);
            if (playList == null)
            {
                _logger.LogInformation(
                    $"{nameof(PlayFile)}: File = {file.Path} won't be played cause " +
                    $"the playListId = {file.PlayListId} does not exist");
                return;
            }

            AppFileType type = _fileService.GetFileType(file.Path);
            try
            {
                if (type.DoesNotExist())
                {
                    string msg = $"The provided file = {file.Path} does not exist";
                    _logger.LogInformation($"{nameof(PlayFile)}: {msg}");
                    throw new Domain.Exceptions.FileNotFoundException(msg);
                }

                bool fileIsBeingPlayed = file.Id == CurrentPlayedFile?.Id && !force && !fileOptionsChanged && !file.Loop;
                if (fileIsBeingPlayed)
                {
                    string msg = $"The provided file = {file.Path} is already being played";
                    _logger.LogInformation($"{nameof(PlayFile)}: {msg}");
                    throw new InvalidRequestException(msg, AppMessageType.FileIsAlreadyBeingPlayed);
                }

                if (string.IsNullOrEmpty(file.Duration))
                {
                    _logger.LogInformation(
                        $"{nameof(PlayFile)}: Cant play file = {file.Filename} yet, " +
                        "because im still setting the duration for some files.");
                    throw new FileNotReadyException($"File = {file.Filename} is not ready yet");
                }

                if (!AvailableDevices.Any())
                {
                    string msg = $"File = {file.Path} won't be played cause there are any devices available";
                    _logger.LogInformation($"{nameof(PlayFile)}: {msg}");
                    throw new NoDevicesException(msg);
                }

                if (type.IsUrl() && !NetworkUtils.IsInternetAvailable())
                {
                    string msg = $"File = {file.Path} won't be played cause there is no internet connection available";
                    _logger.LogInformation($"{nameof(PlayFile)}: {msg}");
                    throw new InvalidRequestException(msg, AppMessageType.NoInternetConnection);
                }

                bool canHandleFile = await _mediaRequestGeneratorFactory.CanHandleRequest(file.Path, type);
                if (!canHandleFile)
                {
                    string msg = $"File = {file.Path} cannot be handle by this app";
                    _logger.LogInformation($"{nameof(PlayFile)}: {msg}");
                    throw new InvalidRequestException(msg);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e,
                    $"{nameof(PlayFile)}: A validation error occurred, " +
                    $"isAnAutomaticCall = {isAnAutomaticCall}");
                if (!isAnAutomaticCall)
                    throw;

                if (e is Domain.Exceptions.FileNotFoundException)
                {
                    bool allWereNotFound = playList.Files.Count == 0 ||
                                           playList.Files.All(f => _fileService.GetFileType(f.Path).DoesNotExist());
                    if (allWereNotFound)
                    {
                        _logger.LogInformation(
                            $"{nameof(PlayFile)}: All the files inside playListId = {playList.Id} were not found, " +
                            "there's nothing to play here");
                        await StopPlayback();
                        return;
                    }

                    await GoTo(file.PlayListId, file.Id, file.Position, true, true, true);
                    return;
                }

                e.HandleCastException(this, _telemetry);
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
                await StopPlayback(false, false);
                CurrentPlayList = playList;
                CurrentPlayedFile = file.BeingPlayed();
                FileLoading();
                DisableLoopForAllFiles(file.Id);

                bool resume = file.CanStartPlayingFromCurrentPercentage &&
                              !type.IsUrl() &&
                              !force &&
                              (!_settings.StartFilesFromTheStart || fileOptionsChanged);

                double seekSeconds = !resume ? 0 : file.GetSecondsToSeek();
                var request = await _mediaRequestGeneratorFactory.BuildRequest(
                    file, _settings.Settings,
                    seekSeconds, fileOptionsChanged, FileCancellationTokenSource.Token);

                if (differentFile)
                {
                    GenerateThumbnailRanges(type, CurrentPlayedFile.TotalSeconds);
                }

                if (request.NeedsTinyCode)
                {
                    await GenerateAndSetTinyCode(request);
                }

                if (resume)
                {
                    _logger.LogInformation($"{nameof(PlayFile)}: File will be resumed from = {file.PlayedPercentage}% ...");
                    await GoToPosition(request);
                }
                else
                {
                    _logger.LogInformation($"{nameof(PlayFile)}: Playing file from the start...");
                    await PlayFile(request);
                }

                RefreshPlayListImage(CurrentPlayList);

                _logger.LogInformation($"{nameof(PlayFile)}: File is being played...");
            }
            catch (Exception e)
            {
                if (!(e is BaseAppException))
                {
                    _logger.LogError(e, $"{nameof(PlayFile)}: An unknown error occurred");
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

        private async Task PlayFile(PlayMediaRequest options)
        {
            CleanupBeforePlaying();
            var type = _fileService.GetFileType(options.MediaInfo.ContentId);
            _logger.LogInformation($"{nameof(PlayFile)}: Doing some checks before playing...");
            DoChecksBeforePlaying(options.MediaInfo.ContentId, type, options);

            _logger.LogInformation($"{nameof(PlayFile)}: Setting default renderer if needed...");
            await SetDefaultRenderIfNeeded();

            _currentContentId = options.MediaInfo.ContentId;

            _logger.LogInformation($"{nameof(PlayFile)}: Trying to load url = {options.MediaInfo.ContentId}");
            CurrentRequest = options;
            var status = await _player.LoadAsync(
                options.MediaInfo, true, options.SeekSeconds,
                options.ActiveTrackIds.ToArray()).ConfigureAwait(false);

            if (status is null)
            {
                var msg = $"Couldn't load url = {options.MediaInfo.ContentId}";
                _logger.LogWarning($"{nameof(PlayFile)}: {msg}");
                throw new ErrorLoadingFileException(msg);
            }

            _logger.LogInformation($"{nameof(PlayFile)}: Url was successfully loaded");
        }

        private void CleanupBeforePlaying()
        {
            CurrentRequest = null;
        }

        private void DoChecksBeforePlaying(string mrl, AppFileType type, PlayMediaRequest options)
        {
            _fFmpeg.KillTranscodeProcess();
            if (string.IsNullOrWhiteSpace(mrl))
            {
                _logger.LogWarning($"{nameof(DoChecksBeforePlaying)}: No mrl was provided");
                throw new ArgumentNullException(nameof(options), "No mrl was provided");
            }

            if (options == null)
            {
                _logger.LogWarning($"{nameof(DoChecksBeforePlaying)}: No play media request was provided for mrl = {mrl}");
                throw new ArgumentNullException(nameof(options), "The play media request cannot be null");
            }

            if (options.FileInfo == null)
            {
                _logger.LogWarning($"{nameof(DoChecksBeforePlaying)}: No file info was provided for mrl = {mrl}");
                throw new ArgumentNullException(nameof(options.FileInfo), "A file info must be provided");
            }

            if (options.MediaInfo == null)
            {
                _logger.LogWarning($"{nameof(DoChecksBeforePlaying)}: No media info was provided for mrl = {mrl}");
                throw new ArgumentNullException(nameof(options.MediaInfo), "The media info cannot be null");
            }

            if (type.DoesNotExist())
            {
                var msg = $"Invalid = {mrl}. Its not a local file and its not a url file";
                _logger.LogWarning($"{nameof(DoChecksBeforePlaying)}: {msg}");
                throw new FileNotSupportedException(msg);
            }

            if (AvailableDevices.Count == 0)
            {
                _logger.LogWarning($"{nameof(DoChecksBeforePlaying)}: No renders were found, file = {mrl}");
                throw new NoDevicesException($"No renders were found, file = {mrl}");
            }

            if (_connecting)
            {
                _logger.LogWarning($"{nameof(DoChecksBeforePlaying)}: We are in the middle of connecting to a device, can't play file = {mrl} right now");
                throw new ConnectingException("We are in the middle of connecting to a device, can't play file = {mrl} right now");
            }
        }

        private async Task SetDefaultRenderIfNeeded()
        {
            if (!_renderWasSet && AvailableDevices.Count > 0)
            {
                _logger.LogInformation($"{nameof(SetDefaultRenderIfNeeded)}: No renderer has been set, setting the first one...");
                await SetCastRenderer(AvailableDevices.First()).ConfigureAwait(false);
            }
        }

        public Task TogglePlayback()
        {
            return _player.IsPlaying ? _player.PauseAsync() : _player.PlayAsync();
        }

        private async Task UpdateSecondsOfCurrent(double seconds)
        {
            if (!CurrentRequest.NeedsTinyCode)
                return;
            var dto = CurrentRequest.Base64.FromBase64<PlayAppFileRequestDto>();
            dto.Seconds = seconds;
            CurrentRequest.Base64 = dto.ToBase64();
            await GenerateAndSetTinyCode(CurrentRequest);
        }

        private async Task GenerateAndSetTinyCode(PlayMediaRequest request)
        {
            string tinyCode = await _appDataService.GenerateTinyCode(request.Base64);
            request.MediaInfo.ContentId = _server.GetPlayUrl(tinyCode);
        }

        public async Task GoToPosition(double position, double totalSeconds)
        {
            if (!_player.IsPlayingOrPaused || CurrentRequest == null)
            {
                _logger.LogWarning($"{nameof(GoToPosition)}: Can't go to position = {position} because nothing is being played");
                return;
            }

            if (!(position >= 0) || !(position <= 100))
            {
                _logger.LogWarning($"{nameof(GoToPosition)} Cant go to position = {position}");
                return;
            }

            try
            {
                //TODO: SUBS ARE BEING KEPT IN CACHE
                FileLoading();
                double seconds = position * totalSeconds / 100;
                await _mediaRequestGeneratorFactory.HandleSecondsChanged(
                    CurrentPlayedFile, _settings.Settings, CurrentRequest,
                    seconds, FileCancellationTokenSource.Token);
                if (CurrentRequest.IsHandledByServer)
                {
                    await UpdateSecondsOfCurrent(seconds);
                    await PlayFile(CurrentRequest);
                }
                else
                {
                    await _player.SeekAsync(seconds);
                }
                FileLoaded(this, EventArgs.Empty);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, $"{nameof(GoToPosition)} Unknown error occurred");
                await StopPlayback();
                throw;
            }
        }

        private async Task GoToPosition(PlayMediaRequest options)
        {
            try
            {
                FileLoading();
                if (options.IsHandledByServer)
                {
                    await PlayFile(options);
                }
                else
                {
                    await _player.SeekAsync(options.SeekSeconds);
                }
                FileLoaded(this, EventArgs.Empty);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, $"{nameof(GoToPosition)} Unknown error occurred");
                await StopPlayback();
                throw;
            }
        }

        public Task GoToPosition(double position)
        {
            return GoToPosition(position, _player.CurrentMediaDuration);
        }

        public async Task GoToSeconds(double seconds)
        {
            _logger.LogInformation($"{nameof(GoToSeconds)}: Trying to go to seconds = {seconds}");
            if (!_player.IsPlayingOrPaused || CurrentRequest == null)
            {
                _logger.LogWarning($"{nameof(GoToSeconds)}: Can't go to seconds = {seconds} because nothing is being played");
                return;
            }

            if (seconds >= _player.CurrentMediaDuration)
            {
                _logger.LogWarning(
                    $"{nameof(GoToSeconds)}: Cant go to = {seconds} because is bigger or equal than " +
                    $"the media duration = {_player.CurrentMediaDuration}");
                return;
            }
            if (seconds < 0)
            {
                _logger.LogWarning($"{nameof(GoToSeconds)}: Wont go to = {seconds}, instead we will go to 0");
                seconds = 0;
            }

            try
            {
                FileLoading();
                await _mediaRequestGeneratorFactory.HandleSecondsChanged(
                    CurrentPlayedFile, _settings.Settings, CurrentRequest,
                    seconds, FileCancellationTokenSource.Token);
                if (CurrentRequest.IsHandledByServer)
                {
                    await UpdateSecondsOfCurrent(seconds);
                    await PlayFile(CurrentRequest);
                }
                else
                {
                    await _player.SeekAsync(seconds);
                }
                FileLoaded(this, EventArgs.Empty);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, $"{nameof(GoToSeconds)} Unknown error occurred");
                await StopPlayback();
                throw;
            }
        }

        public async Task AddSeconds(double seconds)
        {
            if (!_player.IsPlayingOrPaused)
            {
                _logger.LogWarning($"{nameof(AddSeconds)}: Can't add seconds = {seconds} because nothing is being played");
                return;
            }

            if (_currentContentId == null || CurrentRequest == null)
            {
                _logger.LogWarning($"{nameof(AddSeconds)}: Can't go skip seconds = {seconds} because the current played file is null");
                return;
            }

            if (seconds >= _player.CurrentMediaDuration || _player.CurrentMediaDuration + seconds < 0)
            {
                _logger.LogWarning(
                    $"{nameof(AddSeconds)}: Cant add seconds = {seconds} because is bigger or equal than " +
                    $"the media duration = {_player.CurrentMediaDuration} or the diff is less than 0");
                return;
            }

            double newValue = _player.ElapsedSeconds + seconds;
            if (newValue < 0)
            {
                _logger.LogWarning($"{nameof(AddSeconds)}: The seconds to add are = {newValue}. They will be set to 0");
                newValue = 0;
            }
            else if (newValue >= _player.CurrentMediaDuration)
            {
                _logger.LogWarning(
                    $"{nameof(AddSeconds)}: The seconds to add exceeds the media duration, " +
                    $"they will be set to = {_player.CurrentMediaDuration}");
                newValue = _player.CurrentMediaDuration;
            }

            try
            {
                FileLoading();
                await _mediaRequestGeneratorFactory.HandleSecondsChanged(
                    CurrentPlayedFile, _settings.Settings, CurrentRequest,
                    newValue, FileCancellationTokenSource.Token);
                if (CurrentRequest.IsHandledByServer)
                {
                    await UpdateSecondsOfCurrent(newValue);
                    await PlayFile(CurrentRequest);
                }
                else
                {
                    await _player.SeekAsync(newValue);
                }
                FileLoaded(this, EventArgs.Empty);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, $"{nameof(AddSeconds)} Unknown error occurred");
                await StopPlayback();
                throw;
            }
        }

        public async Task<double> SetVolume(double level)
        {
            if (string.IsNullOrEmpty(_currentContentId) || _player.CurrentVolumeLevel == level)
                return _player.CurrentVolumeLevel;

            _logger.LogInformation($"{nameof(SetVolume)}: Setting volume level to = {level}...");
            if (level > 1)
            {
                level /= 100;
                _logger.LogInformation($"{nameof(SetVolume)}: Since volume level is greater than 1, the new level will be = {level}");
            }
            else if (level < 0)
            {
                _logger.LogWarning($"{nameof(SetVolume)}: Since volume level is less than 0, the new level will be = 0");
                level = 0;
            }
            var status = await _player.SetVolumeAsync((float)level).ConfigureAwait(false);
            return status?.Volume?.Level ?? level;
        }

        public async Task<bool> SetIsMuted(bool isMuted)
        {
            if (string.IsNullOrEmpty(_currentContentId) || _player.IsMuted == isMuted)
                return _player.IsMuted;
            var status = await _player.SetIsMutedAsync(isMuted).ConfigureAwait(false);
            return status?.Volume?.IsMuted ?? isMuted;
        }

        public async Task StopRunningProcess()
        {
            await _fFmpeg.KillThumbnailProcess();
            await _fFmpeg.KillTranscodeProcess();
        }

        public async Task StopAsync()
        {
            FileCancellationTokenSource.Cancel();

            _logger.LogInformation($"{nameof(StopAsync)}: Saving the changes made to the play lists + files...");
            var files = PlayLists.SelectMany(pl => pl.Files)
                .Where(f => f.WasPlayed || f.PositionChanged)
                .ToList();
            await _appDataService.SavePlayListChanges(PlayLists);
            await _appDataService.SaveFileChanges(files);
            _appDataService.Close();

            _logger.LogInformation($"{nameof(StopAsync)}: Changes were saved");
            try
            {
                _logger.LogInformation($"{nameof(StopAsync)} Clean them all started...");
                _player.FileLoading -= FileLoading;
                _player.DeviceAdded -= RendererDiscovererItemAdded;
                _player.EndReached -= EndReached;
                _player.TimeChanged -= TimeChanged;
                _player.PositionChanged -= PositionChanged;
                _player.Paused -= Paused;
                _player.Disconnected -= Disconnected;
                _player.VolumeLevelChanged -= VolumeLevelChanged;
                _player.IsMutedChanged -= IsMutedChanged;
                _player.LoadFailed -= LoadFailed;

                await StopRunningProcess();

                _player.Dispose();
                _logger.LogInformation($"{nameof(StopAsync)} Clean them all completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(StopAsync)}: An unknown error occurred");
                _telemetry.TrackError(ex);
            }
        }

        public Task SetCastRenderer(string id)
        {
            IReceiver selected = AvailableDevices.Find(f => f.IsConnected);
            if (string.IsNullOrEmpty(id))
            {
                return SetNullCastRenderer(selected);
            }
            var renderer = AvailableDevices.Find(d => d.Id == id);
            return renderer is null ? SetNullCastRenderer(selected) : SetCastRenderer(renderer);
        }

        public Task SetCastRenderer(string host, int port)
        {
            if (!AvailableDevices.Any(d => d.Host == host && d.Port == port))
            {
                AvailableDevices.Add(Receiver.Default(host, port));
            }

            var renderer = AvailableDevices.Find(d => d.Host == host && d.Port == port);
            return SetCastRenderer(renderer);
        }

        public async Task RefreshCastDevices(TimeSpan? ts = null)
        {
            if (!ts.HasValue || ts.Value.TotalSeconds > 30 || ts.Value.TotalSeconds <= 0)
            {
                ts = TimeSpan.FromSeconds(10);
            }
            var devices = await _player.GetDevicesAsync(ts.Value);
            var connectedTo = AvailableDevices.Find(t => t.IsConnected);
            AvailableDevices.Clear();
            AvailableDevices.AddRange(devices.OrderBy(a => a.FriendlyName));
            if (connectedTo != null && AvailableDevices.Any(d => d.Id == connectedTo.Id))
            {
                var connectedToDevice = AvailableDevices.First(d => d.Id == connectedTo.Id);
                connectedToDevice.IsConnected = true;
            }
            SendDevicesChanged();
        }

        public async Task GoTo(bool nextTrack, bool isAnAutomaticCall = false)
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
                _logger.LogInformation(
                    $"{nameof(GoTo)}: PlaylistId = {currentPlayListId} does not exist. " +
                    "It may have been deleted. Playback will stop now");
                await StopPlayback();
                _onSkipOrPrevious = false;
                return;
            }
            var files = playList.Files;
            if (!files.Any())
            {
                _logger.LogInformation(
                    $"{nameof(GoTo)}: PlaylistId = {currentPlayListId} does not have any file to play. " +
                    "Playback will stop now");
                await StopPlayback();
                _onSkipOrPrevious = false;
                return;
            }

            _logger.LogInformation($"{nameof(GoTo)}: Getting the next / previous file to play.... Going to next file = {nextTrack}");
            int increment = nextTrack ? 1 : -1;
            var fileIndex = files.FindIndex(f => f.Id == currentFileId);
            int newIndex = fileIndex + increment;
            bool random = playList.Shuffle && files.Count > 1;
            if (random)
                _logger.LogInformation($"{nameof(GoTo)}: Random is active for playListId = {playList.Id}, picking a random file ...");

            if (!isAnAutomaticCall && !random && files.Count > 1 && files.ElementAtOrDefault(newIndex) == null)
            {
                _logger.LogInformation(
                    $"{nameof(GoTo)}: The new index = {newIndex} does not exist in the playlist, " +
                    "falling back to the first or last item in the list");
                var nextOrPreviousFile = nextTrack ? files.First() : files.Last();
                await PlayFile(nextOrPreviousFile);
                _onSkipOrPrevious = false;
                return;
            }

            if (fileIndex < 0)
            {
                _logger.LogInformation(
                    $"{nameof(GoTo)}: FileId = {currentFileId} is no longer present in the playlist, " +
                    "it may have been deleted, getting the closest one...");

                int nextPosition = currentFilePosition + increment;
                int closestPosition = files
                    .Select(f => f.Position)
                    .GetClosest(nextPosition);

                var closestFile = files.Find(f => f.Position == closestPosition);

                _logger.LogInformation($"{nameof(GoTo)}: Closest file is = {closestFile?.Path}, trying to play it");
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
                _logger.LogInformation(
                    $"{nameof(GoTo)}: The next file to play is = {file.Path} and it's index is = {newIndex} " +
                    $"compared to the old one = {fileIndex}....");
                await PlayFile(file, isAnAutomaticCall: isAnAutomaticCall);
                _onSkipOrPrevious = false;
                return;
            }
            _logger.LogInformation(
                $"{nameof(GoTo)}: File at index = {fileIndex} in playListId {playList.Id} was not found. " +
                "Probably an end of playlist");

            if (!playList.Loop)
            {
                _logger.LogInformation(
                    $"{nameof(GoTo)}: Since no file was found and playlist is not marked to loop, " +
                    "the playback of this playlist will end here");
                await StopPlayback();
                _onSkipOrPrevious = false;
                return;
            }

            _logger.LogInformation($"{nameof(GoTo)}: Looping playlistId = {playList.Id}");
            await PlayFile(files.FirstOrDefault(), isAnAutomaticCall: isAnAutomaticCall);
            _onSkipOrPrevious = false;
        }

        public async Task StopPlayback(bool nullPlayedFile = true, bool disableLoopForAll = true)
        {
            if (IsPlayingOrPaused)
            {
                await _player.StopPlaybackAsync();
                await StopRunningProcess();
            }

            CleanPlayedFile(nullPlayedFile);
            await StopRunningProcess();
            if (disableLoopForAll)
                DisableLoopForAllFiles();
            _server.OnStoppedPlayback?.Invoke();
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
                Player = _mapper.Map<PlayerStatusResponseDto>(_player.State),
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
            _logger.LogTrace($"{nameof(UpdateFileItem)}: Checking if we need to update fileId = {file.Id}... Force = {force}");
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
                _logger.LogTrace($"{nameof(UpdateFileItem)}: FileId = {file.Id} will be updated...");
                await _appDataService.UpdateFile(file.Id, file.Filename, file.SubTitle, file.TotalSeconds);
                SendFileChanged(_mapper.Map<FileItemResponseDto>(file));
            }

            _logger.LogTrace($"{nameof(UpdateFileItem)}: FileId = {file.Id} was successfully processed");
        }

        public async Task<FFProbeFileInfo> GetFileInfo(string mrl, CancellationToken token)
        {
            if (_fileService.IsUrlFile(mrl))
            {
                return new FFProbeFileInfo
                {
                    Format = new FileInfoFormat()
                };
            }

            return await _fFmpeg.GetFileInfo(mrl, token);
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
                _logger.LogWarning(
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
                _logger.LogWarning(
                    $"{nameof(SetCurrentPlayedFileOptions)}: The provided file options are not valid " +
                    $"for currentPlayedFileId = {CurrentPlayedFile?.Id}");
                return Task.CompletedTask;
            }

            if (CurrentPlayedFile == null)
            {
                _logger.LogWarning(
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
            _logger.LogInformation($"{nameof(SetFileSubtitlesFromPath)}: Trying to set subs from path = {filePath}...");
            var (isSub, filename) = _fileService.IsSubtitle(filePath);
            if (!isSub || CurrentPlayedFile.CurrentFileSubTitles.Any(f => f.Text == filename))
            {
                _logger.LogInformation($"{nameof(SetFileSubtitlesFromPath)}: Subtitle = {filePath} is not valid or is already in the current sub files");
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
                if (_previewThumbnails.Count == 0 ||
                    string.IsNullOrWhiteSpace(CurrentPlayedFile?.Path) ||
                    tentativeSecond > CurrentPlayedFile?.TotalSeconds ||
                    tentativeSecond < 0)
                {
                    return _imageProviderService.NoImageBytes;
                }

                if ((CurrentPlayedFile.Type.IsLocalMusic() || CurrentPlayedFile.Type.IsUrl()) &&
                    _previewThumbnails.Any(t => t.Image != null))
                {
                    return _previewThumbnails.First().Image;
                }

                var preview = _previewThumbnails.Find(t => t.Range.ContainsValue(tentativeSecond));
                Range<long> range = preview.Range;
                if (preview.Image != null)
                {
                    return preview.Image;
                }

                if (preview.IsBeingGenerated)
                {
                    return _imageProviderService.TransparentImageBytes;
                }

                preview.Generating();
                if (CurrentPlayedFile.Type.IsLocalMusic())
                {
                    //Images for music files don't change, that's why we return the original image here
                    var pathTo = _fFmpeg.GetThumbnail(CurrentPlayedFile.Path);
                    var bytes = await File.ReadAllBytesAsync(pathTo);
                    return preview.SetImage(bytes).Generated().Image;
                }

                if (CurrentPlayedFile.Type.IsUrl() && !string.IsNullOrWhiteSpace(CurrentRequest.ThumbnailUrl))
                {
                    //In this case, we download and save the image if needed
                    var pathTo = await _fileService.DownloadAndSavePreviewImage(
                        CurrentPlayedFile.Id,
                        CurrentRequest.ThumbnailUrl, false);
                    var bytes = await File.ReadAllBytesAsync(pathTo);
                    return preview.SetImage(bytes).Generated().Image;
                }

                var fps = CurrentPlayedFile.FileInfo?.Videos.FirstOrDefault()?.AverageFrameRate ?? 24;
                var tasks = new List<Task>
                {
                    Task.Run(async () =>
                    {
                        var bytes = await _fFmpeg.GetThumbnailTile(CurrentPlayedFile.Path, range.Minimum, fps);
                        preview.SetImage(bytes);
                    })
                };
                var previousRange = _previewThumbnails.Find(kvp =>
                    kvp.Range.Index == range.Index - 1 &&
                    !kvp.HasImage &&
                    !kvp.IsBeingGenerated);
                var nextRange = _previewThumbnails.Find(kvp =>
                    kvp.Range.Index == range.Index + 1 &&
                    !kvp.HasImage &&
                    !kvp.IsBeingGenerated);

                //previous range
                if (previousRange != null)
                {
                    previousRange.Generating();
                    var task = Task.Run(async () =>
                    {
                        var image = await _fFmpeg.GetThumbnailTile(
                            CurrentPlayedFile.Path,
                            previousRange.Range.Minimum,
                            fps);
                        previousRange.SetImage(image).Generated();
                    });
                    tasks.Add(task);
                }

                //next range
                if (nextRange != null)
                {
                    nextRange.Generating();
                    var task = Task.Run(async () =>
                    {
                        var image = await _fFmpeg.GetThumbnailTile(
                            CurrentPlayedFile.Path,
                            nextRange.Range.Minimum,
                            fps);
                        nextRange.SetImage(image).Generated();
                    });
                    tasks.Add(task);
                }

                await Task.WhenAll(tasks);

                //At this point we should have the bytes
                return preview.Generated().Image;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{nameof(GetClosestPreviewThumbnail)}: Unknown error");
            }

            return _imageProviderService.NoImageBytes;
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
                    _previewThumbnails.Add(new ThumbnailRange(min, max + fix, d));
                    min += AppWebServerConstants.ThumbnailsPerImage;
                    max += AppWebServerConstants.ThumbnailsPerImage;
                }
            }
            else
            {
                long max = totalSeconds > 0 ? (long)totalSeconds : long.MaxValue;
                _previewThumbnails.Add(new ThumbnailRange(0, max, 0));
            }

            _thumbnailRanges.AddRange(_previewThumbnails.Select(t => new FileThumbnailRangeResponseDto
            {
                ThumbnailRange = t.Range
            }));

            //there's no point in generating the thumbnail matrix for other file types, since the image will be the same
            if (!type.IsLocalVideo())
                return;

            Parallel.ForEach(_thumbnailRanges, (thumbnailRange, _, __) =>
            {
                thumbnailRange.PreviewThumbnailUrl = _server.GetThumbnailPreviewUrl(thumbnailRange.ThumbnailRange.Minimum);
                thumbnailRange.SetMatrixOfSeconds(AppWebServerConstants.ThumbnailsPerImageRow);
            });
        }

        private Task FileOptionsChanged(FileItemOptionsResponseDto selectedItem)
        {
            if (selectedItem == null)
            {
                _logger.LogWarning($"{nameof(FileOptionsChanged)}: Selected option is null");
                return Task.CompletedTask;
            }

            if (selectedItem.IsSelected)
            {
                _logger.LogInformation($"{nameof(FileOptionsChanged)}: The selected options is already selected");
                return Task.CompletedTask;
            }

            if (CurrentPlayedFile == null)
            {
                _logger.LogWarning($"{nameof(FileOptionsChanged)}: No file is being played");
                return Task.CompletedTask;
            }

            CurrentPlayedFile.SetSelectedFileOption(selectedItem);

            return PlayFile(CurrentPlayedFile, false, true, false);
        }

        #region Helpers
        private async Task SetCastRenderer(IReceiver receiver)
        {
            _connecting = true;
            _renderWasSet = false;
            try
            {
                await _player.ConnectAsync(receiver);
                _server.OnCastRendererSet?.Invoke(receiver);
                _renderWasSet = true;
            }
            finally
            {
                _connecting = false;
            }
        }

        private async Task SetNullCastRenderer(IReceiver selected)
        {
            _renderWasSet = false;
            await _player.DisconnectAsync();
            if (selected != null)
            {
                _server.OnCastRendererSet?.Invoke(selected);
            }
        }
        #endregion
    }
}
