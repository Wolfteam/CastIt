using CastIt.Common;
using CastIt.Common.Utils;
using CastIt.GoogleCast;
using CastIt.GoogleCast.Enums;
using CastIt.GoogleCast.Interfaces;
using CastIt.GoogleCast.Models.Events;
using CastIt.GoogleCast.Models.Media;
using CastIt.Interfaces;
using CastIt.Server;
using EmbedIO;
using MvvmCross.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CastIt.Services
{
    public class CastService : ICastService
    {
        private const int SubTitleDefaultTrackId = 1;

        private readonly IMvxLog _logger;
        private readonly IFFMpegService _ffmpegService;
        private readonly IYoutubeUrlDecoder _youtubeUrlDecoder;
        private readonly WebServer _webServer;
        private readonly Player _player;
        private readonly CancellationTokenSource _webServerCancellationToken = new CancellationTokenSource();
        private readonly Track _subtitle;
        private readonly TextTrackStyle _subtitlesStyle;

        private bool _renderWasSet;
        private string _currentFilePath;

        public IList<IReceiver> AvailableDevices { get; } = new List<IReceiver>();
        public OnCastRendererSetHandler OnCastRendererSet { get; set; }
        public OnCastableDeviceAddedHandler OnCastableDeviceAdded { get; set; }
        public OnCastableDeviceDeletedHandler OnCastableDeviceDeleted { get; set; }
        public OnFileLoaded OnFileLoaded { get; set; }
        public OnPositionChangedHandler OnPositionChanged { get; set; }
        public OnTimeChangedHandler OnTimeChanged { get; set; }
        public OnEndReachedHandler OnEndReached { get; set; }
        public OnQualitiesChanged QualitiesChanged { get; set; }
        public OnPaused OnPaused { get; set; }
        public OnDisconnected OnDisconnected { get; set; }
        public Func<string> GetSubTitles { get; set; }

        public CastService(
            IMvxLogProvider logProvider,
            IFFMpegService ffmpegService,
            IYoutubeUrlDecoder youtubeUrlDecoder,
            WebServer webServer)
        {
            _logger = logProvider.GetLogFor<CastService>();
            _ffmpegService = ffmpegService;
            _youtubeUrlDecoder = youtubeUrlDecoder;
            _webServer = webServer;
            _player = new Player(logProvider.GetLogFor<Player>(), logMsgs: true);
            _subtitle = new Track
            {
                TrackId = SubTitleDefaultTrackId,
                SubType = TextTrackType.Subtitles,
                Type = TrackType.Text,
                Name = "English",
                Language = "en-US"
            };
            _subtitlesStyle = new TextTrackStyle
            {
                BackgroundColor = Color.Transparent,
                EdgeColor = Color.Black,
                FontScale = 1.2F,
                WindowType = TextTrackWindowType.Normal,
                EdgeType = TextTrackEdgeType.None,
                FontStyle = TextTrackFontStyleType.Normal,
                FontGenericFamily = TextTrackFontGenericFamilyType.Casual,
            };
        }

        public void Init()
        {
            _logger.Info($"{nameof(Init)}: Initializing all...");
            _player.DeviceAdded += RendererDiscovererItemAdded;
            _player.EndReached += EndReached;
            _player.TimeChanged += TimeChanged;
            _player.PositionChanged += PositionChanged;
            _player.Paused += Paused;
            _player.Disconnected += Disconnected;

            _player.Init();

            _webServer.Start(_webServerCancellationToken.Token);
            _logger.Info($"{nameof(Init)}: Initialize completed");
        }

        public async Task StartPlay(
            string mrl,
            int videoStreamIndex,
            int audioStreamIndex,
            int subtitleStreamIndex,
            int quality,
            double seconds = 0)
        {
            _ffmpegService.KillTranscodeProcess();
            bool isLocal = FileUtils.IsLocalFile(mrl);
            bool isUrlFile = FileUtils.IsUrlFile(mrl);
            bool isVideoFile = FileUtils.IsVideoFile(mrl);
            bool isMusicFile = FileUtils.IsMusicFile(mrl);

            if (!isLocal && !isUrlFile)
            {
                _logger.Warn($"{nameof(StartPlay)}: Invalid = {mrl}. Its not a local file and its not a url file");
                return;
            }

            if (AvailableDevices.Count == 0)
            {
                _logger.Warn($"{nameof(StartPlay)}: No renders were found, file = {mrl}");
                return;
            }

            if (!_renderWasSet && AvailableDevices.Count > 0)
            {
                await SetCastRenderer(AvailableDevices.First());
            }
            // create new media
            _currentFilePath = mrl;
            string title = isLocal ? Path.GetFileName(mrl) : mrl;
            string url = isLocal
                ? AppWebServer.GetMediaUrl(_webServer, mrl, videoStreamIndex, audioStreamIndex, seconds)
                : mrl;

            var metadata = isVideoFile ? new MovieMetadata
            {
                Title = title,
            } : new GenericMediaMetadata
            {
                Title = title,
            };
            var media = new MediaInformation
            {
                ContentId = url,
                Metadata = metadata,
                //You have to set the contenttype before hand, with that, the album art of a music file will be shown
                ContentType = _ffmpegService.GetOutputTranscodeMimeType(mrl)
            };

            var activeTrackIds = new List<int>();
            bool useSubTitleStream = subtitleStreamIndex >= 0;
            if (useSubTitleStream || !string.IsNullOrEmpty(GetSubTitles?.Invoke()))
            {
                _logger.Info($"{nameof(StartPlay)}: Subtitles were specified, generating a compatible one...");
                string subtitleLocation = useSubTitleStream ? mrl : GetSubTitles.Invoke();
                string subTitleFilePath = FileUtils.GetSubTitleFilePath();
                await _ffmpegService.GenerateSubTitles(
                    subtitleLocation,
                    subTitleFilePath,
                    seconds,
                    useSubTitleStream ? subtitleStreamIndex : 0,
                    default);

                _subtitle.TrackContentId = AppWebServer.GetSubTitlePath(_webServer, subTitleFilePath);
                _logger.Info($"{nameof(StartPlay)}: Subtitles were generated");
                media.Tracks.Add(_subtitle);
                media.TextTrackStyle = _subtitlesStyle;
                activeTrackIds.Add(SubTitleDefaultTrackId);
            }

            string firstThumbnail = GetFirstThumbnail();
            string imgUrl = string.Empty;
            if (isLocal)
            {
                _logger.Info($"{nameof(StartPlay)}: File is a local one, generating metadata...");
                imgUrl = AppWebServer.GetPreviewPath(_webServer, firstThumbnail);

                if (isVideoFile)
                    media.StreamType = StreamType.Live;

                var fileInfo = await _ffmpegService.GetFileInfo(mrl, default);
                media.Duration = fileInfo.Format.Duration;
                if (isMusicFile)
                {
                    media.Metadata = new MusicTrackMediaMetadata
                    {
                        Title = title,
                        AlbumName = fileInfo.Format.Tag?.Album,
                        Artist = fileInfo.Format.Tag?.Artist,
                    };
                }
            }
            else if (_youtubeUrlDecoder.IsYoutubeUrl(media.ContentId))
            {
                _logger.Info($"{nameof(StartPlay)}: File is a youtube link, parsing it...");
                var youtubeMedia = await _youtubeUrlDecoder.Parse(media.ContentId, quality);
                QualitiesChanged?.Invoke(youtubeMedia.SelectedQuality, youtubeMedia.Qualities);

                imgUrl = youtubeMedia.ThumbnailUrl;
                media.ContentId = youtubeMedia.Url;
                media.Metadata.Title = youtubeMedia.Title;
                media.Metadata.Subtitle = youtubeMedia.Description;
            }

            if (!string.IsNullOrEmpty(imgUrl))
            {
                media.Metadata.Images.Add(new GoogleCast.Models.Image
                {
                    Url = imgUrl
                });
            }

            _logger.Info($"{nameof(StartPlay)}: Trying to load file = {mrl} on seconds = {seconds}");
            await _player.LoadAsync(media, true, seekedSeconds: seconds, activeTrackIds.ToArray());

            _logger.Info($"{nameof(StartPlay)}: File was succesfully loaded");
            OnFileLoaded?.Invoke(_currentFilePath, metadata.Title, isLocal ? firstThumbnail : imgUrl, _player.CurrentMediaDuration);
        }

        public string GetFirstThumbnail()
            => GetThumbnail(10);

        public string GetFirstThumbnail(string filePath)
            => GetThumbnail(filePath, 10);

        public string GetThumbnail(int second)
            => GetThumbnail(_currentFilePath, second);

        public string GetThumbnail(string filePath, int second)
            => _ffmpegService.GetThumbnail(filePath, second);

        public void GenerateThumbmnails()
            => GenerateThumbmnails(_currentFilePath);

        public async void GenerateThumbmnails(string filePath)
        {
            await Task.Run(() =>
            {
                _ffmpegService.KillThumbnailProcess();
                _ffmpegService.GenerateThumbmnails(filePath);
            }).ConfigureAwait(false);
        }

        public Task TogglePlayback()
        {
            if (_player.IsPlaying)
            {
                return _player.PauseAsync();
            }

            return _player.PlayAsync();
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
            double totalSeconds)
        {
            if (position >= 0 && position <= 100)
            {
                double seconds = position * totalSeconds / 100;
                if (FileUtils.IsLocalFile(filePath))
                    return StartPlay(filePath, videoStreamIndex, audioStreamIndex, subtitleStreamIndex, quality, seconds);

                return _player.SeekAsync(seconds);
            }

            _logger.Warn($"{nameof(GoToPosition)} Cant go to position = {position}");
            return Task.CompletedTask;
        }

        public Task GoToSeconds(
            int videoStreamIndex,
            int audioStreamIndex,
            int subtitleStreamIndex,
            int quality,
            double seconds)
        {
            if (FileUtils.IsLocalFile(_currentFilePath))
                return StartPlay(_currentFilePath, videoStreamIndex, audioStreamIndex, subtitleStreamIndex, quality, seconds);

            return _player.SeekAsync(seconds);
        }

        public Task AddSeconds(
            int videoStreamIndex,
            int audioStreamIndex,
            int subtitleStreamIndex,
            int quality,
            double seconds)
        {
            var current = _player.ElapsedSeconds + seconds;
            if (FileUtils.IsLocalFile(_currentFilePath))
            {
                if (current < 0)
                {
                    _logger.Warn($"{nameof(AddSeconds)}: The seconds to add are = {current}. They will be set to 0");
                    current = 0;
                }
                return StartPlay(_currentFilePath, videoStreamIndex, audioStreamIndex, subtitleStreamIndex, quality, current);
            }
            return _player.SeekAsync(current);
        }

        public void StopRunningProcess()
        {
            _ffmpegService.KillThumbnailProcess();
            _ffmpegService.KillTranscodeProcess();
        }

        public void CleanThemAll()
        {
            try
            {
                _player.DeviceAdded -= RendererDiscovererItemAdded;
                _player.EndReached -= EndReached;
                _player.TimeChanged -= TimeChanged;
                _player.PositionChanged -= PositionChanged;
                _player.Paused -= Paused;
                _player.Disconnected -= Disconnected;

                _webServerCancellationToken.Cancel();
                StopRunningProcess();

                _webServer.Dispose();
                _player.Dispose();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"{nameof(CleanThemAll)}: An unknown error ocurred");
            }
        }

        public string GetFileName(string mrl)
        {
            if (FileUtils.IsUrlFile(mrl))
                return mrl;
            return Path.GetFileName(mrl);
        }

        public string GetExtension(string mrl)
        {
            if (FileUtils.IsLocalFile(mrl))
                return Path.GetExtension(mrl).ToUpper();
            if (FileUtils.IsUrlFile(mrl))
                return "WEB";
            return "N/A";
        }

        public string GetFileSizeString(string mrl)
        {
            if (!FileUtils.IsLocalFile(mrl))
                return "N/A";
            var fileInfo = new FileInfo(mrl);
            return GetBytesReadable(fileInfo.Length);
        }

        public Task SetCastRenderer(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return SetNullCastRenderer();
            }
            var renderer = AvailableDevices.FirstOrDefault(d => d.Id == id);
            if (renderer is null)
            {
                return SetNullCastRenderer();
            }

            return SetCastRenderer(renderer);
        }

        private void EndReached(object sender, EventArgs e)
        {
            OnEndReached?.Invoke();
        }

        private void PositionChanged(object sender, double position)
        {
            if (position > 100)
            {
                position = 100;
            }
            OnPositionChanged?.Invoke(position);
        }

        private void TimeChanged(object sender, double seconds)
        {
            OnTimeChanged?.Invoke(seconds);
        }

        private void Paused(object sender, EventArgs e)
        {
            OnPaused?.Invoke();
        }

        private void Disconnected(object sender, EventArgs e)
        {
            _renderWasSet = false;
            OnDisconnected?.Invoke();
        }

        private void RendererDiscovererItemAdded(object sender, DeviceAddedArgs e)
        {
            _logger.Info(
                $"{nameof(RendererDiscovererItemAdded)}: New item discovered: " +
                $"{e.Receiver.FriendlyName} - Ip = {e.Receiver.Host}:{e.Receiver.Port}");
            AvailableDevices.Add(e.Receiver);

            OnCastableDeviceAdded?.Invoke(e.Receiver);
        }

        //TODO: CHECK IF WE CAN KNOW WHEN A DEVICE IS REMOVED
        //private void RendererDiscovererItemDeleted(object sender, RendererDiscovererItemDeletedEventArgs e)
        //{
        //    _logger.Info(
        //        $"{nameof(RendererDiscovererItemAdded)}: Item removed: " +
        //        $"{e.RendererItem.Name} of type {e.RendererItem.Type}");
        //    _rendererItems.Remove(e.RendererItem);
        //    OnCastableDeviceDeleted?.Invoke(new CastableDevice
        //    {
        //        Name = e.RendererItem.Name,
        //        Type = e.RendererItem.Type
        //    });
        //}

        private Task SetCastRenderer(IReceiver receiver)
        {
            _renderWasSet = true;
            OnCastRendererSet?.Invoke(receiver.Id);
            return _player.ConnectAsync(receiver);
        }

        private Task SetNullCastRenderer()
        {
            _renderWasSet = false;
            return _player.DisconnectAsync();
        }

        private string GetBytesReadable(long i)
        {
            // Get absolute value
            long absolute_i = i < 0 ? -i : i;
            // Determine the suffix and readable value
            string suffix;
            double readable;
            if (absolute_i >= 0x1000000000000000) // Exabyte
            {
                suffix = "EB";
                readable = i >> 50;
            }
            else if (absolute_i >= 0x4000000000000) // Petabyte
            {
                suffix = "PB";
                readable = i >> 40;
            }
            else if (absolute_i >= 0x10000000000) // Terabyte
            {
                suffix = "TB";
                readable = i >> 30;
            }
            else if (absolute_i >= 0x40000000) // Gigabyte
            {
                suffix = "GB";
                readable = i >> 20;
            }
            else if (absolute_i >= 0x100000) // Megabyte
            {
                suffix = "MB";
                readable = i >> 10;
            }
            else if (absolute_i >= 0x400) // Kilobyte
            {
                suffix = "KB";
                readable = i;
            }
            else
            {
                return i.ToString("0 B"); // Byte
            }
            // Divide by 1024 to get fractional value
            readable /= 1024;
            // Return formatted number with suffix
            return readable.ToString("0.## ") + suffix;
        }
    }
}
