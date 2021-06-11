using CastIt.Application.Common;
using CastIt.Application.Common.Extensions;
using CastIt.Application.Interfaces;
using CastIt.Application.Server;
using CastIt.Domain.Enums;
using CastIt.Domain.Exceptions;
using CastIt.Domain.Extensions;
using CastIt.Domain.Interfaces;
using CastIt.Domain.Models.Device;
using CastIt.Domain.Models.FFmpeg.Info;
using CastIt.GoogleCast.Enums;
using CastIt.GoogleCast.Interfaces;
using CastIt.GoogleCast.Models.Events;
using CastIt.GoogleCast.Models.Media;
using CastIt.Server.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CastIt.Server.Services
{
    public abstract class BaseCastService : ICastService
    {
        private const int SubTitleDefaultTrackId = 1;
        protected readonly ILogger<BaseCastService> Logger;
        protected readonly IBaseWebServer AppWebServer;
        protected readonly IFFmpegService FFmpegService;
        protected readonly IYoutubeUrlDecoder YoutubeUrlDecoder;
        protected readonly ITelemetryService TelemetryService;
        protected readonly IServerAppSettingsService AppSettings;
        protected readonly IFileService FileService;

        protected readonly IPlayer Player;
        private readonly Track _subtitle;

        private bool _renderWasSet;
        private string _currentFilePath;
        private bool _connecting;

        public List<IReceiver> AvailableDevices { get; } = new List<IReceiver>();
        #region Player Delegates
        //TODO: MOVE THESE TO THE FAKEWEBSERVER ?
        public OnCastRendererSetHandler OnCastRendererSet { get; set; }
        public OnCastableDeviceAddedHandler OnCastableDeviceAdded { get; set; }
        public OnCastableDeviceDeletedHandler OnCastableDeviceDeleted { get; set; }
        public OnFileLoadedHandler OnFileLoaded { get; set; }
        public OnPositionChangedHandler OnPositionChanged { get; set; }
        public OnTimeChangedHandler OnTimeChanged { get; set; }
        public OnEndReachedHandler OnEndReached { get; set; }
        public OnQualitiesChangedHandler QualitiesChanged { get; set; }
        public OnPausedHandler OnPaused { get; set; }
        public OnDisconnectedHandler OnDisconnected { get; set; }
        public OnVolumeChangedHandler OnVolumeChanged { get; set; }
        public OnServerMessageHandler OnServerMessage { get; set; }
        #endregion

        #region Server Delegates
        //TODO: MIGRATE TO EVENTS ?
        public OnFileLoadingHandler OnFileLoading { get; set; }
        public OnAppClosingHandler OnAppClosing { get; set; }
        public OnAppSettingsChangedHandler OnAppSettingsChanged { get; set; }

        public OnPlayListAddedHandler OnPlayListAdded { get; set; }
        public OnPlayListChangedHandler OnPlayListChanged { get; set; }
        public OnPlayListDeletedHandler OnPlayListDeleted { get; set; }

        public OnFileAddedHandler OnFileAdded { get; set; }
        public OnFileChangedHandler OnFileChanged { get; set; }
        public OnFileDeletedHandler OnFileDeleted { get; set; }
        #endregion

        public Func<string> GetSubTitles { get; set; }

        public bool IsPlayingOrPaused => Player.IsPlayingOrPaused;
        public int CurrentVideoStreamIndex { get; private set; }
        public int CurrentAudioStreamIndex { get; private set; }
        public int CurrentSubtitleStreamIndex { get; private set; }
        public int CurrentVideoQuality { get; private set; }
        public string CurrentThumbnailUrl { get; set; }
        public FFProbeFileInfo CurrentFileInfo { get; set; }

        protected BaseCastService(
            ILogger<BaseCastService> logger,
            IBaseWebServer appWebServer,
            IFFmpegService ffmpegService,
            IYoutubeUrlDecoder youtubeUrlDecoder,
            ITelemetryService telemetryService,
            IServerAppSettingsService appSettings,
            IFileService fileService,
            IPlayer player)
        {
            Logger = logger;
            AppWebServer = appWebServer;
            FFmpegService = ffmpegService;
            YoutubeUrlDecoder = youtubeUrlDecoder;
            TelemetryService = telemetryService;
            AppSettings = appSettings;
            FileService = fileService;
            Player = player;
            _subtitle = new Track
            {
                TrackId = SubTitleDefaultTrackId,
                SubType = TextTrackType.Subtitles,
                Type = TrackType.Text,
                Name = "English",
                Language = "en-US"
            };
        }

        #region Methods
        public virtual Task Init()
        {
            Logger.LogInformation($"{nameof(Init)}: Initializing all...");
            Player.FileLoading += FileLoading;
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

        public async Task StartPlay(
            string mrl,
            int videoStreamIndex,
            int audioStreamIndex,
            int subtitleStreamIndex,
            int quality,
            FFProbeFileInfo fileInfo,
            double seconds = 0)
        {
            var type = FileService.GetFileType(mrl);
            Logger.LogInformation($"{nameof(StartPlay)}: Doing some checks before playing...");
            DoChecksBeforePlaying(mrl, type, fileInfo);

            Logger.LogInformation($"{nameof(StartPlay)}: Setting default renderer if needed...");
            await SetDefaultRenderIfNeeded();

            _currentFilePath = mrl;

            var media = type.IsUrl()
                ? await BuildMetadataForUrl(mrl, seconds, quality)
                : await BuildMetadataForLocalFile(mrl, type, videoStreamIndex, audioStreamIndex, seconds);

            Logger.LogInformation($"{nameof(StartPlay)}: Setting subtitles if needed...");
            var activeTrackIds = new List<int>();
            await SetSubtitlesIfAny(mrl, media, activeTrackIds, subtitleStreamIndex, seconds);

            Logger.LogInformation($"{nameof(StartPlay)}: Trying to load url = {media.ContentId}");
            var status = await Player.LoadAsync(media, true, seconds, activeTrackIds.ToArray());
            if (status is null)
            {
                var msg = $"Couldn't load url = {media.ContentId}";
                Logger.LogWarning($"{nameof(StartPlay)}: {msg}");
                throw new ErrorLoadingFileException(msg);
            }

            Logger.LogInformation($"{nameof(StartPlay)}: Url was successfully loaded");

            FileLoaded(media.Metadata.Title);
        }

        private void DoChecksBeforePlaying(string mrl, AppFileType type, FFProbeFileInfo fileInfo)
        {
            CurrentFileInfo = fileInfo;
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

        private async Task<MediaInformation> BuildMetadataForLocalFile(
            string filePath,
            AppFileType type,
            int videoStreamIndex,
            int audioStreamIndex,
            double seconds)
        {
            Logger.LogInformation($"{nameof(BuildMetadataForLocalFile)}: File is a local one, generating metadata...");
            bool videoNeedsTranscode = type.IsVideo() && FFmpegService.VideoNeedsTranscode(
                videoStreamIndex, AppSettings.ForceVideoTranscode,
                AppSettings.VideoScale, CurrentFileInfo);
            bool audioNeedsTranscode = FFmpegService.AudioNeedsTranscode(
                audioStreamIndex, AppSettings.ForceAudioTranscode,
                CurrentFileInfo, type.IsMusic());
            var hwAccelToUse = type.IsVideo()
                ? FFmpegService.GetHwAccelToUse(videoStreamIndex, CurrentFileInfo, AppSettings.EnableHardwareAcceleration)
                : HwAccelDeviceType.None;

            string title = Path.GetFileName(filePath);
            string url = AppWebServer.GetMediaUrl(
                filePath,
                videoStreamIndex,
                audioStreamIndex,
                seconds,
                videoNeedsTranscode,
                audioNeedsTranscode,
                hwAccelToUse,
                AppSettings.VideoScale,
                CurrentFileInfo.Videos.Find(f => f.Index == videoStreamIndex)?.WidthAndHeightText);

            var media = new MediaInformation
            {
                ContentId = url,
                Metadata = new MovieMetadata
                {
                    Title = title,
                },
                //You have to set the content type before hand, with that, the album art of a music file will be shown
                ContentType = FFmpegService.GetOutputTranscodeMimeType(filePath),
                Duration = CurrentFileInfo.Format.Duration
            };

            if (type.IsVideo())
            {
                media.StreamType = StreamType.Live;
            }
            else if (type.IsMusic())
            {
                media.Metadata = new MusicTrackMediaMetadata
                {
                    Title = title,
                    AlbumName = CurrentFileInfo.Format.Tag?.Album,
                    Artist = CurrentFileInfo.Format.Tag?.Artist,
                };
            }

            Logger.LogInformation($"{nameof(BuildMetadataForLocalFile)}: Retrieving img url to use...");
            string firstThumbnail = await GetFirstThumbnail();
            string imgUrl = AppWebServer.GetPreviewPath(firstThumbnail);
            if (!string.IsNullOrEmpty(imgUrl))
            {
                media.Metadata.Images.Add(new GoogleCast.Models.Image
                {
                    Url = imgUrl
                });
            }

            CurrentVideoStreamIndex = videoStreamIndex;
            CurrentAudioStreamIndex = audioStreamIndex;
            CurrentThumbnailUrl = imgUrl;

            Logger.LogInformation($"{nameof(BuildMetadataForLocalFile)}: Metadata was successfully created");
            return media;
        }

        private async Task<MediaInformation> BuildMetadataForUrl(string url, double seconds, int quality)
        {
            if (!YoutubeUrlDecoder.IsYoutubeUrl(url))
            {
                var msg = $"Url = {url} is not supported";
                Logger.LogInformation($"{nameof(BuildMetadataForUrl)}: {url}");
                throw new FileNotSupportedException(msg);
            }

            Logger.LogInformation($"{nameof(BuildMetadataForUrl)}: File is a youtube link, parsing it...");
            var ytMedia = await YoutubeUrlDecoder.Parse(url, quality);

            //TODO: IS THIS REQUIRED ?
            QualitiesChanged?.Invoke(ytMedia.SelectedQuality, ytMedia.Qualities);

            //TODO: SHOULD I SET THE STREAM TYPE ?
            var media = new MediaInformation
            {
                ContentId = ytMedia.Url,
                Metadata = new MovieMetadata
                {
                    Title = ytMedia.Title,
                    Subtitle = ytMedia.Description
                },
                //You have to set the content type before hand, with that, the album art of a music file will be shown
                ContentType = FFmpegService.GetOutputTranscodeMimeType(url)
            };
            if (!string.IsNullOrEmpty(ytMedia.ThumbnailUrl))
            {
                media.Metadata.Images.Add(new GoogleCast.Models.Image
                {
                    Url = ytMedia.ThumbnailUrl
                });
            }

            CurrentVideoQuality = quality;
            CurrentThumbnailUrl = ytMedia.ThumbnailUrl;

            if (!ytMedia.IsHls)
            {
                return media;
            }

            Logger.LogInformation($"{nameof(BuildMetadataForUrl)}: Url is a yt hls, getting its file info...");
            var fileInfo = await FFmpegService.GetFileInfo(ytMedia.Url, default);
            if (fileInfo == null)
            {
                Logger.LogWarning($"{nameof(BuildMetadataForUrl)}: Couldn't get the file info for url = {ytMedia.Url}");
                throw new Exception($"File info is null for yt hls = {ytMedia.Url}");
            }

            var closestQuality = fileInfo.HlsVideos
                .Select(v => v.Height)
                .GetClosest(quality);
            var videoInfo = fileInfo.HlsVideos.First(v => v.Height == closestQuality);
            int videoStreamIndex = videoInfo.Index;
            int audioStreamIndex = fileInfo.Audios.Any()
                ? fileInfo.Audios.Select(a => a.Index).GetClosest(videoStreamIndex)
                : -1;

            bool videoNeedsTranscode = FFmpegService.VideoNeedsTranscode(
                videoStreamIndex, AppSettings.ForceVideoTranscode,
                AppSettings.VideoScale, fileInfo);
            bool audioNeedsTranscode = FFmpegService.AudioNeedsTranscode(
                audioStreamIndex, AppSettings.ForceAudioTranscode, fileInfo);

            media.Duration = -1;
            media.StreamType = StreamType.Live;
            media.ContentId = AppWebServer.GetMediaUrl(
                ytMedia.Url,
                videoStreamIndex,
                audioStreamIndex,
                seconds,
                videoNeedsTranscode,
                audioNeedsTranscode,
                HwAccelDeviceType.None,
                //TODO: IS IT OKAY TO USE ORIGINAL HERE ?
                VideoScaleType.Original,
                videoInfo.WidthAndHeightText);
            media.ContentType = FFmpegService.GetOutputTranscodeMimeType(media.ContentId);

            CurrentVideoStreamIndex = videoStreamIndex;
            CurrentAudioStreamIndex = audioStreamIndex;

            return media;
        }

        private async Task SetSubtitlesIfAny(string mrl, MediaInformation media, List<int> activeTrackIds, int subtitleStreamIndex, double seconds)
        {
            bool useSubTitleStream = subtitleStreamIndex >= 0;
            if (useSubTitleStream || !string.IsNullOrEmpty(GetSubTitles?.Invoke()))
            {
                Logger.LogInformation(
                    $"{nameof(StartPlay)}: Subtitles were specified, generating a compatible one...");
                string subtitleLocation = useSubTitleStream ? mrl : GetSubTitles.Invoke();
                string subTitleFilePath = FileService.GetSubTitleFilePath();
                await FFmpegService.GenerateSubTitles(
                    subtitleLocation,
                    subTitleFilePath,
                    seconds,
                    useSubTitleStream ? subtitleStreamIndex : 0,
                    AppSettings.SubtitleDelayInSeconds,
                    default);

                _subtitle.TrackContentId = AppWebServer.GetSubTitlePath(subTitleFilePath);
                Logger.LogInformation($"{nameof(StartPlay)}: Subtitles were generated");
                media.Tracks.Add(_subtitle);
                media.TextTrackStyle = GetSubtitleStyle();
                activeTrackIds.Add(SubTitleDefaultTrackId);
            }
            CurrentSubtitleStreamIndex = subtitleStreamIndex;
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

        public async Task StopPlayback()
        {
            await StopRunningProcess();
            await Player.StopPlaybackAsync();
        }

        public Task GoToPosition(
            string filePath,
            int videoStreamIndex,
            int audioStreamIndex,
            int subtitleStreamIndex,
            int quality,
            double position,
            double totalSeconds,
            FFProbeFileInfo fileInfo)
        {
            if (position >= 0 && position <= 100)
            {
                double seconds = position * totalSeconds / 100;
                if (FileService.IsLocalFile(filePath))
                    return StartPlay(filePath, videoStreamIndex, audioStreamIndex, subtitleStreamIndex, quality, fileInfo, seconds);

                return Player.SeekAsync(seconds);
            }

            Logger.LogWarning($"{nameof(GoToPosition)} Cant go to position = {position}");
            return Task.CompletedTask;
        }

        public Task GoToPosition(double position)
        {
            return GoToPosition(position, Player.CurrentMediaDuration);
        }

        public Task GoToPosition(double position, double totalSeconds)
        {
            if (!Player.IsPlayingOrPaused)
            {
                Logger.LogWarning($"{nameof(GoToPosition)}: Can't go to position = {position} because nothing is being played");
                return Task.CompletedTask;
            }

            return GoToPosition(
                _currentFilePath,
                CurrentVideoStreamIndex, CurrentAudioStreamIndex,
                CurrentSubtitleStreamIndex, CurrentVideoQuality,
                position, totalSeconds, CurrentFileInfo);
        }

        public Task GoToSeconds(
            int videoStreamIndex,
            int audioStreamIndex,
            int subtitleStreamIndex,
            int quality,
            double seconds,
            FFProbeFileInfo fileInfo)
        {
            Logger.LogInformation($"{nameof(GoToSeconds)}: Trying to go to seconds = {seconds}");
            //if (_currentFilePath == null)
            //{
            //    _logger.LogWarning($"{nameof(GoToSeconds)}: Can't go to seconds = {seconds} because the current played file is null");
            //    return Task.CompletedTask;
            //}

            if (!Player.IsPlayingOrPaused)
            {
                Logger.LogWarning($"{nameof(GoToSeconds)}: Can't go to seconds = {seconds} because nothing is being played");
                return Task.CompletedTask;
            }

            if (seconds >= Player.CurrentMediaDuration)
            {
                Logger.LogWarning(
                    $"{nameof(GoToSeconds)}: Cant go to = {seconds} because is bigger or equal than " +
                    $"the media duration = {Player.CurrentMediaDuration}");
                return Task.CompletedTask;
            }
            if (seconds < 0)
            {
                Logger.LogWarning($"{nameof(GoToSeconds)}: Wont go to = {seconds}, instead we will go to 0");
                seconds = 0;
            }

            //TODO: MOVE THE _currentFilePath TO A PARAM LIKE THE GOTOPOSITION METHOD
            SendFileLoading();
            if (FileService.IsLocalFile(_currentFilePath))
                return StartPlay(_currentFilePath, videoStreamIndex, audioStreamIndex, subtitleStreamIndex, quality, fileInfo, seconds);

            return Player.SeekAsync(seconds);
        }

        public Task GoToSeconds(double seconds)
        {
            return GoToSeconds(
                CurrentVideoStreamIndex, CurrentAudioStreamIndex, CurrentSubtitleStreamIndex,
                CurrentVideoQuality, seconds, CurrentFileInfo);
        }

        public Task AddSeconds(
            int videoStreamIndex,
            int audioStreamIndex,
            int subtitleStreamIndex,
            int quality,
            double seconds,
            FFProbeFileInfo fileInfo)
        {
            if (_currentFilePath == null)
            {
                Logger.LogWarning($"{nameof(AddSeconds)}: Can't go skip seconds = {seconds} because the current played file is null");
                return Task.CompletedTask;
            }

            if (seconds >= Player.CurrentMediaDuration || Player.CurrentMediaDuration + seconds < 0)
            {
                Logger.LogWarning(
                    $"{nameof(AddSeconds)}: Cant add seconds = {seconds} because is bigger or equal than " +
                    $"the media duration = {Player.CurrentMediaDuration} or the diff is less than 0");
                return Task.CompletedTask;
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
            //TODO: MOVE THE _currentFilePath TO A PARAM
            SendFileLoading();
            if (!FileService.IsLocalFile(_currentFilePath))
                return Player.SeekAsync(newValue);
            return StartPlay(_currentFilePath, videoStreamIndex, audioStreamIndex, subtitleStreamIndex, quality, fileInfo, newValue);
        }

        public Task AddSeconds(double seconds)
        {
            if (!Player.IsPlayingOrPaused)
            {
                Logger.LogWarning($"{nameof(AddSeconds)}: Can't add seconds = {seconds} because nothing is being played");
                return Task.CompletedTask;
            }

            return AddSeconds(
                CurrentVideoStreamIndex, CurrentAudioStreamIndex, CurrentSubtitleStreamIndex,
                CurrentVideoQuality, seconds, CurrentFileInfo);
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

        public virtual async Task CleanThemAll()
        {
            try
            {
                Logger.LogInformation($"{nameof(CleanThemAll)} Clean them all started...");
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

                AppWebServer.Dispose();
                Player.Dispose();
                Logger.LogInformation($"{nameof(CleanThemAll)} Clean them all completed");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"{nameof(CleanThemAll)}: An unknown error occurred");
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

        public abstract Task GoTo(bool nextTrack, bool isAnAutomaticCall = false);
        #endregion

        #region Events handlers
        public void FileLoading(object sender, EventArgs e)
            => SendFileLoading();

        private void FileLoaded(string title)
            => OnFileLoaded?.Invoke(title, CurrentThumbnailUrl, Player.CurrentMediaDuration, Player.CurrentVolumeLevel, Player.IsMuted);

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
        public void SendFileLoading()
            => OnFileLoading?.Invoke();

        public void SendEndReached()
            => OnEndReached?.Invoke();

        public void SendPositionChanged(double position)
        {
            if (position > 100)
            {
                position = 100;
            }
            OnPositionChanged?.Invoke(position);
        }

        public void SendTimeChanged(double seconds)
            => OnTimeChanged?.Invoke(seconds);

        public void SendPaused()
            => OnPaused?.Invoke();

        public void SendDisconnected()
        {
            _renderWasSet = false;
            OnDisconnected?.Invoke();
        }

        public void SendVolumeLevelChanged(double newValue)
            => OnVolumeChanged?.Invoke(newValue, Player.IsMuted);

        public void SendIsMutedChanged(bool isMuted)
            => OnVolumeChanged?.Invoke(Player.CurrentVolumeLevel, isMuted);

        public void SendRendererDiscovererItemAdded(IReceiver item)
        {
            Logger.LogInformation(
                $"{nameof(RendererDiscovererItemAdded)}: New item discovered: " +
                $"{item.FriendlyName} - Ip = {item.Host}:{item.Port}");
            AvailableDevices.Add(item);
            //TODO: SEND THE WHOLE RECEIVER
            OnCastableDeviceAdded?.Invoke(item.Id, item.FriendlyName, item.Type, item.Host, item.Port);
        }

        public void SendErrorLoadingFile()
            => OnServerMessage?.Invoke(AppMessageType.UnknownErrorLoadingFile);

        public void SendNoDevicesFound()
            => OnServerMessage?.Invoke(AppMessageType.NoDevicesFound);

        public void SendNoInternetConnection()
            => OnServerMessage?.Invoke(AppMessageType.NoInternetConnection);

        public void SendPlayListNotFound()
            => OnServerMessage?.Invoke(AppMessageType.PlayListNotFound);

        public void SendFileNotFound()
            => OnServerMessage?.Invoke(AppMessageType.FileNotFound);

        public void SendInvalidRequest()
            => OnServerMessage?.Invoke(AppMessageType.InvalidRequest);

        public void SendServerIsClosing()
            => OnServerMessage?.Invoke(AppMessageType.ServerIsClosing);

        public void SendPlayListAdded(long id)
            => OnPlayListAdded?.Invoke(id);

        public void SendPlayListChanged(long id)
            => OnPlayListChanged?.Invoke(id);

        public void SendPlayListDeleted(long id)
            => OnPlayListDeleted?.Invoke(id);

        public void SendFileAdded(long playListId, long id)
            => OnFileAdded?.Invoke(playListId, id);

        public void SendFileChanged(long playListId, long id)
            => OnFileChanged?.Invoke(playListId, id);

        public void SendFileDeleted(long playListId, long id)
            => OnFileDeleted?.Invoke(playListId, id);

        public void SendServerMsg(AppMessageType type)
            => OnServerMessage?.Invoke(type);
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
                OnCastRendererSet?.Invoke(receiver.Id);
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
