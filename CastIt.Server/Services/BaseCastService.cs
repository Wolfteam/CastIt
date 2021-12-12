using CastIt.Domain.Dtos.Responses;
using CastIt.Domain.Enums;
using CastIt.Domain.Exceptions;
using CastIt.Domain.Extensions;
using CastIt.Domain.Models.FFmpeg.Info;
using CastIt.FFmpeg;
using CastIt.GoogleCast.Interfaces;
using CastIt.GoogleCast.Models.Events;
using CastIt.GoogleCast.Models.Media;
using CastIt.GoogleCast.Models.Play;
using CastIt.GoogleCast.Shared.Device;
using CastIt.GoogleCast.Shared.Enums;
using CastIt.Server.Interfaces;
using CastIt.Shared.FilePaths;
using CastIt.Shared.Telemetry;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace CastIt.Server.Services
{
    public abstract class BaseCastService : ICastService
    {
        private const int SubTitleDefaultTrackId = 1;
        protected readonly ILogger Logger;
        protected readonly IServerService ServerService;
        protected readonly IFFmpegService FFmpegService;
        protected readonly ITelemetryService TelemetryService;
        protected readonly IServerAppSettingsService AppSettings;
        protected readonly IFileService FileService;

        protected readonly IPlayer Player;
        private readonly Track _subtitle = new Track
        {
            TrackId = SubTitleDefaultTrackId,
            SubType = TextTrackType.Subtitles,
            Type = TrackType.Text,
            Name = "English",
            Language = "en-US"
        };

        private bool _renderWasSet;
        private string _currentFilePath;
        private bool _connecting;

        public List<IReceiver> AvailableDevices { get; } = new List<IReceiver>();
        public bool IsPlayingOrPaused => Player.IsPlayingOrPaused;
        public PlayMediaRequest CurrentRequest { get; private set; }

        protected BaseCastService(
            ILogger logger,
            IServerService serverService,
            IFFmpegService ffmpegService,
            ITelemetryService telemetryService,
            IServerAppSettingsService appSettings,
            IFileService fileService,
            IPlayer player)
        {
            Logger = logger;
            ServerService = serverService;
            FFmpegService = ffmpegService;
            TelemetryService = telemetryService;
            AppSettings = appSettings;
            FileService = fileService;
            Player = player;
        }

        #region Methods
        public virtual Task Init()
        {
            Logger.LogInformation($"{nameof(Init)}: Initializing all...");
            Player.FileLoading += FileLoading;
            Player.FileLoaded += FileLoaded;
            Player.DeviceAdded += RendererDiscovererItemAdded;
            Player.EndReached += EndReached;
            Player.TimeChanged += TimeChanged;
            Player.PositionChanged += PositionChanged;
            Player.Paused += Paused;
            Player.Disconnected += Disconnected;
            Player.VolumeLevelChanged += VolumeLevelChanged;
            Player.IsMutedChanged += IsMutedChanged;
            Player.LoadFailed += LoadFailed;
            Player.ListenForDevices();

            Logger.LogInformation($"{nameof(Init)}: Initialize completed");

            return Task.CompletedTask;
        }

        public async Task StartPlay(PlayMediaRequest options)
        {
            //TODO: VALIDATE THE OPTIONS OBJECT
            CleanupBeforePlaying();
            var type = FileService.GetFileType(options.MediaInfo.ContentId);
            Logger.LogInformation($"{nameof(StartPlay)}: Doing some checks before playing...");
            DoChecksBeforePlaying(options.MediaInfo.ContentId, type, options.FileInfo);

            Logger.LogInformation($"{nameof(StartPlay)}: Setting default renderer if needed...");
            await SetDefaultRenderIfNeeded();

            //TODO: PERHAPS SETS ALL OF THESE IN THE SERVERCASTSERVICE
            _currentFilePath = options.MediaInfo.ContentId;

            Logger.LogInformation($"{nameof(StartPlay)}: Setting subtitles if needed...");
            await SetSubtitlesIfAny(
                options.MediaInfo.ContentId, options.MediaInfo, options.ActiveTrackIds,
                options.SubtitleStreamIndex, options.SeekSeconds);

            Logger.LogInformation($"{nameof(StartPlay)}: Trying to load url = {options.MediaInfo.ContentId}");
            CurrentRequest = options;
            var status = await Player.LoadAsync(
                options.MediaInfo, true, options.SeekSeconds,
                options.ActiveTrackIds.ToArray()).ConfigureAwait(false);

            if (status is null)
            {
                var msg = $"Couldn't load url = {options.MediaInfo.ContentId}";
                Logger.LogWarning($"{nameof(StartPlay)}: {msg}");
                throw new ErrorLoadingFileException(msg);
            }

            Logger.LogInformation($"{nameof(StartPlay)}: Url was successfully loaded");
        }

        protected virtual void CleanupBeforePlaying()
        {
            CurrentRequest = null;
        }

        private void DoChecksBeforePlaying(string mrl, AppFileType type, FFProbeFileInfo fileInfo)
        {
            //CurrentFileInfo = fileInfo;
            FFmpegService.KillTranscodeProcess();
            if (fileInfo == null)
            {
                Logger.LogWarning($"{nameof(DoChecksBeforePlaying)}: No file info was provided for mrl = {mrl}");
                throw new ArgumentNullException(nameof(fileInfo), "A file info must be provided");
            }

            if (type.DoesNotExist())
            {
                var msg = $"Invalid = {mrl}. Its not a local file and its not a url file";
                Logger.LogWarning($"{nameof(DoChecksBeforePlaying)}: {msg}");
                throw new FileNotSupportedException(msg);
            }

            if (AvailableDevices.Count == 0)
            {
                Logger.LogWarning($"{nameof(DoChecksBeforePlaying)}: No renders were found, file = {mrl}");
                throw new NoDevicesException($"No renders were found, file = {mrl}");
            }

            if (_connecting)
            {
                Logger.LogWarning($"{nameof(DoChecksBeforePlaying)}: We are in the middle of connecting to a device, can't play file = {mrl} right now");
                throw new ConnectingException("We are in the middle of connecting to a device, can't play file = {mrl} right now");
            }
        }

        private async Task SetDefaultRenderIfNeeded()
        {
            if (!_renderWasSet && AvailableDevices.Count > 0)
            {
                Logger.LogInformation($"{nameof(SetDefaultRenderIfNeeded)}: No renderer has been set, setting the first one...");
                await SetCastRenderer(AvailableDevices.First()).ConfigureAwait(false);
            }
        }

        protected abstract Task OnQualitiesLoaded(int selectedQuality, List<int> qualities);

        protected abstract Task<string> GetSelectedSubtitlePath();

        private async Task SetSubtitlesIfAny(
            string mrl,
            MediaInformation media,
            List<int> activeTrackIds,
            int subtitleStreamIndex,
            double seconds)
        {
            bool useSubTitleStream = subtitleStreamIndex >= 0;
            var selectedSubtitlePath = await GetSelectedSubtitlePath();
            if (useSubTitleStream || !string.IsNullOrEmpty(selectedSubtitlePath))
            {
                Logger.LogInformation($"{nameof(StartPlay)}: Subtitles were specified, generating a compatible one...");
                string subtitleLocation = useSubTitleStream ? mrl : selectedSubtitlePath;
                string subTitleFilePath = FileService.GetSubTitleFilePath();
                await FFmpegService.GenerateSubTitles(
                    subtitleLocation,
                    subTitleFilePath,
                    seconds,
                    useSubTitleStream ? subtitleStreamIndex : 0,
                    AppSettings.SubtitleDelayInSeconds,
                    default);

                _subtitle.TrackContentId = ServerService.GetSubTitleUrl();
                Logger.LogInformation($"{nameof(StartPlay)}: Subtitles were generated");
                media.Tracks.Add(_subtitle);
                media.TextTrackStyle = GetSubtitleStyle();
                activeTrackIds.Add(SubTitleDefaultTrackId);
            }
        }

        public Task<string> GetFirstThumbnail()
                => GetThumbnail();

        public Task<string> GetFirstThumbnail(string filePath)
            => GetThumbnail(filePath);

        public Task<string> GetThumbnail()
            => GetThumbnail(_currentFilePath);

        public Task<string> GetThumbnail(string filePath)
        {
            return Task.Run(() => FFmpegService.GetThumbnail(filePath));
        }

        public void GenerateThumbnails()
            => GenerateThumbnails(_currentFilePath);

        public async void GenerateThumbnails(string filePath)
        {
            await FFmpegService.KillThumbnailProcess();
            await FFmpegService.GenerateThumbnails(filePath, AppSettings.EnableHardwareAcceleration);
        }

        public Task TogglePlayback()
        {
            return Player.IsPlaying ? Player.PauseAsync() : Player.PlayAsync();
        }

        protected virtual async Task StopPlayback()
        {
            await Player.StopPlaybackAsync();
            await StopRunningProcess();
        }

        public async Task GoToPosition(double position, double totalSeconds)
        {
            if (!Player.IsPlayingOrPaused)
            {
                Logger.LogWarning($"{nameof(GoToPosition)}: Can't go to position = {position} because nothing is being played");
                return;
            }

            if (position >= 0 && position <= 100)
            {
                FileLoading();
                double seconds = position * totalSeconds / 100;
                CurrentRequest.SeekSeconds = seconds;
                if (CurrentRequest.IsHandledByServer)
                {
                    CurrentRequest.SeekSeconds = seconds;
                    await StartPlay(CurrentRequest);
                }
                else
                {
                    await Player.SeekAsync(seconds);
                }

                FileLoaded(this, EventArgs.Empty);
            }

            Logger.LogWarning($"{nameof(GoToPosition)} Cant go to position = {position}");
        }

        public async Task GoToPosition(PlayMediaRequest options)
        {
            FileLoading();
            //TODO: WHY AM I CALLING THE SEEK METHOD INSTEAD OF RETURNING IN THE BELOW METHODS
            if (options.IsHandledByServer)
            {
                await StartPlay(options);
            }
            else
            {
                await Player.SeekAsync(options.SeekSeconds);
            }

            FileLoaded(this, EventArgs.Empty);
        }

        public Task GoToPosition(double position)
        {
            return GoToPosition(position, Player.CurrentMediaDuration);
        }

        public async Task GoToSeconds(double seconds)
        {
            Logger.LogInformation($"{nameof(GoToSeconds)}: Trying to go to seconds = {seconds}");
            if (!Player.IsPlayingOrPaused || CurrentRequest == null)
            {
                Logger.LogWarning($"{nameof(GoToSeconds)}: Can't go to seconds = {seconds} because nothing is being played");
                return;
            }

            if (seconds >= Player.CurrentMediaDuration)
            {
                Logger.LogWarning(
                    $"{nameof(GoToSeconds)}: Cant go to = {seconds} because is bigger or equal than " +
                    $"the media duration = {Player.CurrentMediaDuration}");
                return;
            }
            if (seconds < 0)
            {
                Logger.LogWarning($"{nameof(GoToSeconds)}: Wont go to = {seconds}, instead we will go to 0");
                seconds = 0;
            }

            FileLoading();
            if (CurrentRequest.IsHandledByServer)
            {
                CurrentRequest.SeekSeconds = seconds;
                await StartPlay(CurrentRequest);
            }
            else
            {
                await Player.SeekAsync(seconds);
            }

            FileLoaded(this, EventArgs.Empty);
        }

        public async Task AddSeconds(double seconds)
        {
            if (!Player.IsPlayingOrPaused)
            {
                Logger.LogWarning($"{nameof(AddSeconds)}: Can't add seconds = {seconds} because nothing is being played");
                return;
            }

            if (_currentFilePath == null || CurrentRequest == null)
            {
                Logger.LogWarning($"{nameof(AddSeconds)}: Can't go skip seconds = {seconds} because the current played file is null");
                return;
            }

            if (seconds >= Player.CurrentMediaDuration || Player.CurrentMediaDuration + seconds < 0)
            {
                Logger.LogWarning(
                    $"{nameof(AddSeconds)}: Cant add seconds = {seconds} because is bigger or equal than " +
                    $"the media duration = {Player.CurrentMediaDuration} or the diff is less than 0");
                return;
            }

            var newValue = Player.ElapsedSeconds + seconds;
            if (newValue < 0)
            {
                Logger.LogWarning($"{nameof(AddSeconds)}: The seconds to add are = {newValue}. They will be set to 0");
                newValue = 0;
            }
            else if (newValue >= Player.CurrentMediaDuration)
            {
                Logger.LogWarning(
                    $"{nameof(AddSeconds)}: The seconds to add exceeds the media duration, " +
                    $"they will be set to = {Player.CurrentMediaDuration}");
                newValue = Player.CurrentMediaDuration;
            }
            FileLoading();
            if (CurrentRequest.IsHandledByServer)
            {
                CurrentRequest.SeekSeconds = newValue;
                await StartPlay(CurrentRequest);
            }
            else
            {
                await Player.SeekAsync(newValue);
            }

            FileLoaded(this, EventArgs.Empty);
        }

        public async Task<double> SetVolume(double level)
        {
            if (string.IsNullOrEmpty(_currentFilePath) || Player.CurrentVolumeLevel == level)
                return Player.CurrentVolumeLevel;

            Logger.LogInformation($"{nameof(SetVolume)}: Setting volume level to = {level}...");
            if (level > 1)
            {
                level /= 100;
                Logger.LogInformation($"{nameof(SetVolume)}: Since volume level is greater than 1, the new level will be = {level}");
            }
            else if (level < 0)
            {
                Logger.LogWarning($"{nameof(SetVolume)}: Since volume level is less than 0, the new level will be = 0");
                level = 0;
            }
            var status = await Player.SetVolumeAsync((float)level).ConfigureAwait(false);
            return status?.Volume?.Level ?? level;
        }

        public async Task<bool> SetIsMuted(bool isMuted)
        {
            if (string.IsNullOrEmpty(_currentFilePath) || Player.IsMuted == isMuted)
                return Player.IsMuted;
            var status = await Player.SetIsMutedAsync(isMuted).ConfigureAwait(false);
            return status?.Volume?.IsMuted ?? isMuted;
        }

        public async Task StopRunningProcess()
        {
            await FFmpegService.KillThumbnailProcess();
            await FFmpegService.KillTranscodeProcess();
        }

        public virtual async Task StopAsync()
        {
            try
            {
                Logger.LogInformation($"{nameof(StopAsync)} Clean them all started...");
                Player.FileLoading -= FileLoading;
                Player.DeviceAdded -= RendererDiscovererItemAdded;
                Player.EndReached -= EndReached;
                Player.TimeChanged -= TimeChanged;
                Player.PositionChanged -= PositionChanged;
                Player.Paused -= Paused;
                Player.Disconnected -= Disconnected;
                Player.VolumeLevelChanged -= VolumeLevelChanged;
                Player.IsMutedChanged -= IsMutedChanged;
                Player.LoadFailed -= LoadFailed;

                await StopRunningProcess();

                Player.Dispose();
                Logger.LogInformation($"{nameof(StopAsync)} Clean them all completed");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"{nameof(StopAsync)}: An unknown error occurred");
                TelemetryService.TrackError(ex);
            }
        }

        public Task SetCastRenderer(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return SetNullCastRenderer();
            }
            var renderer = AvailableDevices.Find(d => d.Id == id);
            return renderer is null ? SetNullCastRenderer() : SetCastRenderer(renderer);
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
            var devices = await Player.GetDevicesAsync(ts.Value);
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

        public abstract Task GoTo(bool nextTrack, bool isAnAutomaticCall = false);
        #endregion

        #region Events handlers
        public void FileLoading(object sender, EventArgs e)
            => FileLoading();

        protected abstract void FileLoaded(object sender, EventArgs e);

        protected abstract void FileLoading();

        private async void EndReached(object sender, EventArgs e)
        {
            SendEndReached();
            await GoTo(true, true);
        }

        private void PositionChanged(object sender, double position)
            => SendPositionChanged(position);

        private void TimeChanged(object sender, double seconds)
            => SendTimeChanged(seconds);

        private void Paused(object sender, EventArgs e)
            => SendPaused();

        private void Disconnected(object sender, EventArgs e)
            => SendDisconnected();

        private void VolumeLevelChanged(object sender, double e)
            => SendVolumeLevelChanged(e);

        private void IsMutedChanged(object sender, bool isMuted)
            => SendIsMutedChanged(isMuted);

        private void RendererDiscovererItemAdded(object sender, DeviceAddedArgs e)
            => SendRendererDiscovererItemAdded(e.Receiver);

        private void LoadFailed(object sender, EventArgs e)
            => SendErrorLoadingFile();
        #endregion

        #region Event Senders

        public void SendEndReached()
            => ServerService.OnEndReached?.Invoke();

        public void SendPositionChanged(double position)
        {
            if (position > 100)
            {
                position = 100;
            }
            ServerService.OnPositionChanged?.Invoke(position);
        }

        public void SendTimeChanged(double seconds)
            => ServerService.OnTimeChanged?.Invoke(seconds);

        public void SendPaused()
            => ServerService.OnPaused?.Invoke();

        public void SendDevicesChanged()
            => ServerService.OnCastDevicesChanged?.Invoke(AvailableDevices);

        public void SendDisconnected()
        {
            _renderWasSet = false;
            foreach (var device in AvailableDevices)
            {
                device.IsConnected = false;
            }
            SendDevicesChanged();
            ServerService.OnDisconnected?.Invoke();
        }

        public void SendVolumeLevelChanged(double newValue)
            => ServerService.OnVolumeChanged?.Invoke(newValue, Player.IsMuted);

        public void SendIsMutedChanged(bool isMuted)
            => ServerService.OnVolumeChanged?.Invoke(Player.CurrentVolumeLevel, isMuted);

        public void SendRendererDiscovererItemAdded(IReceiver device)
        {
            Logger.LogInformation(
                $"{nameof(RendererDiscovererItemAdded)}: New device discovered: " +
                $"{device.FriendlyName} - Ip = {device.Host}:{device.Port}");
            if (AvailableDevices.All(d => d.Id != device.Id))
            {
                AvailableDevices.Add(device);
            }
            ServerService.OnCastableDeviceAdded?.Invoke(device);
        }

        public void SendErrorLoadingFile()
            => ServerService.OnServerMessage?.Invoke(AppMessageType.UnknownErrorLoadingFile);

        public void SendNoDevicesFound()
            => ServerService.OnServerMessage?.Invoke(AppMessageType.NoDevicesFound);

        public void SendNoInternetConnection()
            => ServerService.OnServerMessage?.Invoke(AppMessageType.NoInternetConnection);

        public void SendPlayListNotFound()
            => ServerService.OnServerMessage?.Invoke(AppMessageType.PlayListNotFound);

        public void SendFileNotFound()
            => ServerService.OnServerMessage?.Invoke(AppMessageType.FileNotFound);

        public void SendInvalidRequest()
            => ServerService.OnServerMessage?.Invoke(AppMessageType.InvalidRequest);

        public void SendServerIsClosing()
            => ServerService.OnServerMessage?.Invoke(AppMessageType.ServerIsClosing);

        public void SendPlayListAdded(GetAllPlayListResponseDto playList)
            => ServerService.OnPlayListAdded?.Invoke(playList);

        public void SendPlayListChanged(GetAllPlayListResponseDto playList)
            => ServerService.OnPlayListChanged?.Invoke(playList);

        public void SendPlayListsChanged(List<GetAllPlayListResponseDto> playLists)
            => ServerService.OnPlayListsChanged?.Invoke(playLists);

        public void SendPlayListBusy(long id, bool isBusy)
            => ServerService.OnPlayListBusy?.Invoke(id, isBusy);

        public void SendPlayListDeleted(long id)
            => ServerService.OnPlayListDeleted?.Invoke(id);

        public void SendFileAdded(FileItemResponseDto file)
            => ServerService.OnFileAdded?.Invoke(file);

        public void SendFileChanged(FileItemResponseDto file)
            => ServerService.OnFileChanged?.Invoke(file);

        public void SendFilesChanged(List<FileItemResponseDto> files)
            => ServerService.OnFilesChanged?.Invoke(files);

        public void SendFileDeleted(long playListId, long id)
            => ServerService.OnFileDeleted?.Invoke(playListId, id);

        public void SendServerMsg(AppMessageType type)
            => ServerService.OnServerMessage?.Invoke(type);
        #endregion

        //TODO: CHECK IF WE CAN KNOW WHEN A DEVICE IS REMOVED
        //private void RendererDiscovererItemDeleted(object sender, RendererDiscovererItemDeletedEventArgs e)
        //{
        //    _logger.LogInformation(
        //        $"{nameof(RendererDiscovererItemAdded)}: Item removed: " +
        //        $"{e.RendererItem.Name} of type {e.RendererItem.Type}");
        //    _rendererItems.Remove(e.RendererItem);
        //    OnCastableDeviceDeleted?.Invoke(new CastableDevice
        //    {
        //        Name = e.RendererItem.Name,
        //        Type = e.RendererItem.Type
        //    });
        //}

        #region Helpers
        private async Task SetCastRenderer(IReceiver receiver)
        {
            _connecting = true;
            _renderWasSet = false;
            try
            {
                await Player.ConnectAsync(receiver);
                ServerService.OnCastRendererSet?.Invoke(receiver);
                _renderWasSet = true;
            }
            finally
            {
                _connecting = false;
            }
        }

        private Task SetNullCastRenderer()
        {
            _renderWasSet = false;
            return Player.DisconnectAsync();
        }

        private TextTrackStyle GetSubtitleStyle()
        {
            return new TextTrackStyle
            {
                ForegroundColor = AppSettings.CurrentSubtitleFgColor == SubtitleFgColorType.White
                    ? Color.WhiteSmoke
                    : Color.Yellow,
                BackgroundColor = Color.Transparent,
                EdgeColor = Color.Black,
                FontScale = (int)AppSettings.CurrentSubtitleFontScale / 100,
                WindowType = TextTrackWindowType.Normal,
                EdgeType = TextTrackEdgeType.Raised,
                FontStyle = AppSettings.CurrentSubtitleFontStyle,
                FontGenericFamily = AppSettings.CurrentSubtitleFontFamily
            };
        }
        #endregion
    }
}
