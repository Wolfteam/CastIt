using CastIt.Application.Common;
using CastIt.Application.Common.Extensions;
using CastIt.Application.Interfaces;
using CastIt.Domain.Dtos.Requests;
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
using CastIt.Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CastIt.Infrastructure.Services
{
    public class CastService : ICastService
    {
        private const int SubTitleDefaultTrackId = 1;
        //TODO: RENAME THIS
        protected readonly ILogger<CastService> _logger;
        protected readonly IBaseWebServer _appWebServer;
        protected readonly IFFmpegService _ffmpegService;
        protected readonly IYoutubeUrlDecoder _youtubeUrlDecoder;
        protected readonly ITelemetryService _telemetryService;
        protected readonly IAppSettingsService _appSettings;
        protected readonly IFileService _fileService;

        protected readonly IPlayer _player;
        private readonly Track _subtitle;

        private bool _renderWasSet;
        private string _currentFilePath;
        private bool _connecting;

        public List<IReceiver> AvailableDevices { get; } = new List<IReceiver>();
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
        public Func<string> GetSubTitles { get; set; }

        public bool IsPlayingOrPaused => _player.IsPlayingOrPaused;
        public int CurrentVideoStreamIndex { get; private set; }
        public int CurrentAudioStreamIndex { get; private set; }
        public int CurrentSubtitleStreamIndex { get; private set; }
        public int CurrentVideoQuality { get; private set; }
        public string CurrentThumbnailUrl { get; set; }
        public FFProbeFileInfo CurrentFileInfo { get; set; }

        public event OnFileLoadingHandler FileLoading;

        public CastService(
            ILogger<CastService> logger,
            IBaseWebServer appWebServer,
            IFFmpegService ffmpegService,
            IYoutubeUrlDecoder youtubeUrlDecoder,
            ITelemetryService telemetryService,
            IAppSettingsService appSettings,
            IFileService fileService,
            IPlayer player)
        {
            _logger = logger;
            _appWebServer = appWebServer;
            _ffmpegService = ffmpegService;
            _youtubeUrlDecoder = youtubeUrlDecoder;
            _telemetryService = telemetryService;
            _appSettings = appSettings;
            _fileService = fileService;
            _player = player;
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
            _logger.LogInformation($"{nameof(Init)}: Initializing all...");
            _player.FileLoading += OnFileLoading;
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

            _logger.LogInformation($"{nameof(Init)}: Initialize completed");

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
            var type = _fileService.GetFileType(mrl);
            _logger.LogInformation($"{nameof(StartPlay)}: Doing some checks before playing...");
            DoChecksBeforePlaying(mrl, type, fileInfo);

            _logger.LogInformation($"{nameof(StartPlay)}: Setting default renderer if needed...");
            await SetDefaultRenderIfNeeded();

            _currentFilePath = mrl;

            var media = type.IsUrl()
                ? await BuildMetadataForUrl(mrl, seconds, quality)
                : await BuildMetadataForLocalFile(mrl, type, videoStreamIndex, audioStreamIndex, seconds);

            _logger.LogInformation($"{nameof(StartPlay)}: Setting subtitles if needed...");
            var activeTrackIds = new List<int>();
            await SetSubtitlesIfAny(mrl, media, activeTrackIds, subtitleStreamIndex, seconds);

            _logger.LogInformation($"{nameof(StartPlay)}: Trying to load url = {media.ContentId}");
            var status = await _player.LoadAsync(media, true, seconds, activeTrackIds.ToArray());
            if (status is null)
            {
                var msg = $"Couldn't load url = {media.ContentId}";
                _logger.LogWarning($"{nameof(StartPlay)}: {msg}");
                throw new ErrorLoadingFileException(msg);
            }

            _logger.LogInformation($"{nameof(StartPlay)}: Url was successfully loaded");

            FileLoaded(media.Metadata.Title);
        }

        private void DoChecksBeforePlaying(string mrl, AppFileType type, FFProbeFileInfo fileInfo)
        {
            CurrentFileInfo = fileInfo;
            _ffmpegService.KillTranscodeProcess();
            if (fileInfo == null)
            {
                _logger.LogWarning($"{nameof(DoChecksBeforePlaying)}: No file info was provided for mrl = {mrl}");
                throw new ArgumentNullException(nameof(fileInfo), "A file info must be provided");
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

        private async Task<MediaInformation> BuildMetadataForLocalFile(
            string filePath,
            AppFileType type,
            int videoStreamIndex,
            int audioStreamIndex,
            double seconds)
        {
            _logger.LogInformation($"{nameof(BuildMetadataForLocalFile)}: File is a local one, generating metadata...");
            bool videoNeedsTranscode = type.IsVideo() && _ffmpegService.VideoNeedsTranscode(
                videoStreamIndex, _appSettings.ForceVideoTranscode,
                _appSettings.VideoScale, CurrentFileInfo);
            bool audioNeedsTranscode = _ffmpegService.AudioNeedsTranscode(
                audioStreamIndex, _appSettings.ForceAudioTranscode,
                CurrentFileInfo, type.IsMusic());
            var hwAccelToUse = type.IsVideo()
                ? _ffmpegService.GetHwAccelToUse(videoStreamIndex, CurrentFileInfo, _appSettings.EnableHardwareAcceleration)
                : HwAccelDeviceType.None;

            string title = Path.GetFileName(filePath);
            string url = _appWebServer.GetMediaUrl(
                filePath,
                videoStreamIndex,
                audioStreamIndex,
                seconds,
                videoNeedsTranscode,
                audioNeedsTranscode,
                hwAccelToUse,
                _appSettings.VideoScale,
                CurrentFileInfo.Videos.Find(f => f.Index == videoStreamIndex)?.WidthAndHeightText);

            var media = new MediaInformation
            {
                ContentId = url,
                Metadata = new MovieMetadata
                {
                    Title = title,
                },
                //You have to set the content type before hand, with that, the album art of a music file will be shown
                ContentType = _ffmpegService.GetOutputTranscodeMimeType(filePath),
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

            _logger.LogInformation($"{nameof(BuildMetadataForLocalFile)}: Retrieving img url to use...");
            string firstThumbnail = await GetFirstThumbnail();
            string imgUrl = _appWebServer.GetPreviewPath(firstThumbnail);
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

            _logger.LogInformation($"{nameof(BuildMetadataForLocalFile)}: Metadata was successfully created");
            return media;
        }

        private async Task<MediaInformation> BuildMetadataForUrl(string url, double seconds, int quality)
        {
            if (!_youtubeUrlDecoder.IsYoutubeUrl(url))
            {
                var msg = $"Url = {url} is not supported";
                _logger.LogInformation($"{nameof(BuildMetadataForUrl)}: {url}");
                throw new FileNotSupportedException(msg);
            }

            _logger.LogInformation($"{nameof(BuildMetadataForUrl)}: File is a youtube link, parsing it...");
            var ytMedia = await _youtubeUrlDecoder.Parse(url, quality);

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
                ContentType = _ffmpegService.GetOutputTranscodeMimeType(url)
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

            _logger.LogInformation($"{nameof(BuildMetadataForUrl)}: Url is a yt hls, getting its file info...");
            var fileInfo = await _ffmpegService.GetFileInfo(ytMedia.Url, default);
            if (fileInfo == null)
            {
                _logger.LogWarning($"{nameof(BuildMetadataForUrl)}: Couldn't get the file info for url = {ytMedia.Url}");
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

            bool videoNeedsTranscode = _ffmpegService.VideoNeedsTranscode(
                videoStreamIndex, _appSettings.ForceVideoTranscode,
                _appSettings.VideoScale, fileInfo);
            bool audioNeedsTranscode = _ffmpegService.AudioNeedsTranscode(
                audioStreamIndex, _appSettings.ForceAudioTranscode, fileInfo);

            media.Duration = -1;
            media.StreamType = StreamType.Live;
            media.ContentId = _appWebServer.GetMediaUrl(
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
            media.ContentType = _ffmpegService.GetOutputTranscodeMimeType(media.ContentId);

            CurrentVideoStreamIndex = videoStreamIndex;
            CurrentAudioStreamIndex = audioStreamIndex;

            return media;
        }

        private async Task SetSubtitlesIfAny(string mrl, MediaInformation media, List<int> activeTrackIds, int subtitleStreamIndex, double seconds)
        {
            bool useSubTitleStream = subtitleStreamIndex >= 0;
            if (useSubTitleStream || !string.IsNullOrEmpty(GetSubTitles?.Invoke()))
            {
                _logger.LogInformation(
                    $"{nameof(StartPlay)}: Subtitles were specified, generating a compatible one...");
                string subtitleLocation = useSubTitleStream ? mrl : GetSubTitles.Invoke();
                string subTitleFilePath = _fileService.GetSubTitleFilePath();
                await _ffmpegService.GenerateSubTitles(
                    subtitleLocation,
                    subTitleFilePath,
                    seconds,
                    useSubTitleStream ? subtitleStreamIndex : 0,
                    _appSettings.SubtitleDelayInSeconds,
                    default);

                _subtitle.TrackContentId = _appWebServer.GetSubTitlePath(subTitleFilePath);
                _logger.LogInformation($"{nameof(StartPlay)}: Subtitles were generated");
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
            return Task.Run(() => _ffmpegService.GetThumbnail(filePath));
        }

        public void GenerateThumbnails()
            => GenerateThumbnails(_currentFilePath);

        public async void GenerateThumbnails(string filePath)
        {
            await Task.Run(() =>
            {
                _ffmpegService.KillThumbnailProcess();
                _ffmpegService.GenerateThumbnails(filePath, _appSettings.EnableHardwareAcceleration);
            }).ConfigureAwait(false);
        }

        public Task TogglePlayback()
        {
            return _player.IsPlaying ? _player.PauseAsync() : _player.PlayAsync();
        }

        public Task StopPlayback()
        {
            StopRunningProcess();
            return _player.StopPlaybackAsync();
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
                if (_fileService.IsLocalFile(filePath))
                    return StartPlay(filePath, videoStreamIndex, audioStreamIndex, subtitleStreamIndex, quality, fileInfo, seconds);

                return _player.SeekAsync(seconds);
            }

            _logger.LogWarning($"{nameof(GoToPosition)} Cant go to position = {position}");
            return Task.CompletedTask;
        }

        public Task GoToPosition(double position)
        {
            return GoToPosition(position, _player.CurrentMediaDuration);
        }

        public Task GoToPosition(double position, double totalSeconds)
        {
            if (!_player.IsPlayingOrPaused)
            {
                _logger.LogWarning($"{nameof(GoToPosition)}: Can't go to position = {position} because nothing is being played");
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
            _logger.LogInformation($"{nameof(GoToSeconds)}: Trying to go to seconds = {seconds}");
            //if (_currentFilePath == null)
            //{
            //    _logger.LogWarning($"{nameof(GoToSeconds)}: Can't go to seconds = {seconds} because the current played file is null");
            //    return Task.CompletedTask;
            //}

            if (!_player.IsPlayingOrPaused)
            {
                _logger.LogWarning($"{nameof(GoToSeconds)}: Can't go to seconds = {seconds} because nothing is being played");
                return Task.CompletedTask;
            }

            if (seconds >= _player.CurrentMediaDuration)
            {
                _logger.LogWarning(
                    $"{nameof(GoToSeconds)}: Cant go to = {seconds} because is bigger or equal than " +
                    $"the media duration = {_player.CurrentMediaDuration}");
                return Task.CompletedTask;
            }
            if (seconds < 0)
            {
                _logger.LogWarning($"{nameof(GoToSeconds)}: Wont go to = {seconds}, instead we will go to 0");
                seconds = 0;
            }

            //TODO: MOVE THE _currentFilePath TO A PARAM LIKE THE GOTOPOSITION METHOD
            SendFileLoading();
            if (_fileService.IsLocalFile(_currentFilePath))
                return StartPlay(_currentFilePath, videoStreamIndex, audioStreamIndex, subtitleStreamIndex, quality, fileInfo, seconds);

            return _player.SeekAsync(seconds);
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
                _logger.LogWarning($"{nameof(AddSeconds)}: Can't go skip seconds = {seconds} because the current played file is null");
                return Task.CompletedTask;
            }

            if (seconds >= _player.CurrentMediaDuration || _player.CurrentMediaDuration + seconds < 0)
            {
                _logger.LogWarning(
                    $"{nameof(AddSeconds)}: Cant add seconds = {seconds} because is bigger or equal than " +
                    $"the media duration = {_player.CurrentMediaDuration} or the diff is less than 0");
                return Task.CompletedTask;
            }

            var newValue = _player.ElapsedSeconds + seconds;
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
            //TODO: MOVE THE _currentFilePath TO A PARAM
            SendFileLoading();
            if (!_fileService.IsLocalFile(_currentFilePath))
                return _player.SeekAsync(newValue);
            return StartPlay(_currentFilePath, videoStreamIndex, audioStreamIndex, subtitleStreamIndex, quality, fileInfo, newValue);
        }

        public Task AddSeconds(double seconds)
        {
            if (!_player.IsPlayingOrPaused)
            {
                _logger.LogWarning($"{nameof(AddSeconds)}: Can't add seconds = {seconds} because nothing is being played");
                return Task.CompletedTask;
            }

            return AddSeconds(
                CurrentVideoStreamIndex, CurrentAudioStreamIndex, CurrentSubtitleStreamIndex,
                CurrentVideoQuality, seconds, CurrentFileInfo);
        }

        public async Task<double> SetVolume(double level)
        {
            if (string.IsNullOrEmpty(_currentFilePath) || _player.CurrentVolumeLevel == level)
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
            if (string.IsNullOrEmpty(_currentFilePath) || _player.IsMuted == isMuted)
                return _player.IsMuted;
            var status = await _player.SetIsMutedAsync(isMuted).ConfigureAwait(false);
            return status?.Volume?.IsMuted ?? isMuted;
        }

        public void StopRunningProcess()
        {
            _ffmpegService.KillThumbnailProcess();
            _ffmpegService.KillTranscodeProcess();
        }

        public virtual Task CleanThemAll()
        {
            try
            {
                _logger.LogInformation($"{nameof(CleanThemAll)} Clean them all started...");
                _player.FileLoading -= OnFileLoading;
                _player.DeviceAdded -= RendererDiscovererItemAdded;
                _player.EndReached -= EndReached;
                _player.TimeChanged -= TimeChanged;
                _player.PositionChanged -= PositionChanged;
                _player.Paused -= Paused;
                _player.Disconnected -= Disconnected;
                _player.VolumeLevelChanged -= VolumeLevelChanged;
                _player.IsMutedChanged -= IsMutedChanged;
                _player.LoadFailed -= LoadFailed;

                StopRunningProcess();

                _appWebServer.Dispose();
                _player.Dispose();
                _logger.LogInformation($"{nameof(CleanThemAll)} Clean them all completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(CleanThemAll)}: An unknown error occurred");
                _telemetryService.TrackError(ex);
            }

            return Task.CompletedTask;
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

        public Task GoToSeconds(PlayCliFileRequestDto request, FFProbeFileInfo fileInfo)
            => GoToSeconds(request.VideoStreamIndex, request.AudioStreamIndex, request.SubtitleStreamIndex, request.Quality, request.Seconds, fileInfo);

        public Task StartPlay(PlayCliFileRequestDto request, FFProbeFileInfo fileInfo)
            => StartPlay(request.Mrl, request.VideoStreamIndex, request.AudioStreamIndex, request.SubtitleStreamIndex, request.Quality, fileInfo, request.Seconds);
        #endregion

        #region Events handlers
        public void OnFileLoading(object sender, EventArgs e)
            => SendFileLoading();

        private void FileLoaded(string title)
        {
            OnFileLoaded?.Invoke(title, CurrentThumbnailUrl, _player.CurrentMediaDuration, _player.CurrentVolumeLevel, _player.IsMuted);
        }

        private void EndReached(object sender, EventArgs e)
            => SendEndReached();

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
            => FileLoading?.Invoke();

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
            => OnVolumeChanged?.Invoke(newValue, _player.IsMuted);

        public void SendIsMutedChanged(bool isMuted)
            => OnVolumeChanged?.Invoke(_player.CurrentVolumeLevel, isMuted);

        public void SendRendererDiscovererItemAdded(IReceiver item)
        {
            _logger.LogInformation(
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
                await _player.ConnectAsync(receiver);
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
            return _player.DisconnectAsync();
        }

        private TextTrackStyle GetSubtitleStyle()
        {
            return new TextTrackStyle
            {
                ForegroundColor = _appSettings.CurrentSubtitleFgColor == SubtitleFgColorType.White
                    ? Color.WhiteSmoke
                    : Color.Yellow,
                BackgroundColor = Color.Transparent,
                EdgeColor = Color.Black,
                FontScale = (int)_appSettings.CurrentSubtitleFontScale / 100,
                WindowType = TextTrackWindowType.Normal,
                EdgeType = TextTrackEdgeType.Raised,
                FontStyle = _appSettings.CurrentSubtitleFontStyle,
                FontGenericFamily = _appSettings.CurrentSubtitleFontFamily
            };
        }
        #endregion
    }
}
