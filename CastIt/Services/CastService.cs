using CastIt.Common;
using CastIt.Common.Utils;
using CastIt.GoogleCast;
using CastIt.GoogleCast.Enums;
using CastIt.GoogleCast.Interfaces;
using CastIt.GoogleCast.Models;
using CastIt.GoogleCast.Models.Events;
using CastIt.GoogleCast.Models.Media;
using CastIt.Interfaces;
using CastIt.Server;
using EmbedIO;
using MvvmCross.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CastIt.Services
{
    public class CastService : ICastService
    {
        private readonly IMvxLog _logger;
        private readonly IFFMpegService _ffmpegService;
        private readonly WebServer _webServer;
        private readonly Player _player;
        private readonly CancellationTokenSource _webServerCancellationToken = new CancellationTokenSource();

        private bool _renderWasSet;
        private string _currentFilePath;

        public IList<IReceiver> AvailableDevices { get; } = new List<IReceiver>();
        public OnCastRendererSetHandler OnCastRendererSet { get; set; }
        public OnCastableDeviceAddedHandler OnCastableDeviceAdded { get; set; }
        public OnCastableDeviceDeletedHandler OnCastableDeviceDeleted { get; set; }
        public OnPositionChangedHandler OnPositionChanged { get; set; }
        public OnTimeChangedHandler OnTimeChanged { get; set; }
        public OnEndReachedHandler OnEndReached { get; set; }

        public CastService(IMvxLogProvider logProvider, IFFMpegService ffmpegService, WebServer webServer)
        {
            _logger = logProvider.GetLogFor<CastService>();
            _ffmpegService = ffmpegService;
            _webServer = webServer;
            _player = new Player(logProvider.GetLogFor<Player>(), logMsgs: false);
        }

        public void Init()
        {
            _logger.Info($"{nameof(Init)}: Initializing all...");
            _player.DeviceAdded += RendererDiscovererItemAdded;
            _player.EndReached += EndReached;
            _player.TimeChanged += TimeChanged;
            _player.PositionChanged += PositionChanged;

            _player.Init();

            _webServer.Start(_webServerCancellationToken.Token);
            _logger.Info($"{nameof(Init)}: Initialize completed");
        }

        public async Task<MediaStatus> StartPlay(string mrl, double seconds = 0)
        {
            _ffmpegService.KillTranscodeProcess();
            bool isLocal = FileUtils.IsLocalFile(mrl);
            bool isUrlFile = FileUtils.IsUrlFile(mrl);
            bool isVideoFile = FileUtils.IsVideoFile(mrl);
            bool isMusicFile = FileUtils.IsMusicFile(mrl);

            if (!isLocal && !isUrlFile)
            {
                _logger.Warn($"{nameof(StartPlay)}: Invalid = {mrl}. Its not a local file and its not a url file");
                return null;
            }

            if (AvailableDevices.Count == 0)
            {
                _logger.Warn($"{nameof(StartPlay)}: No renders were found, file = {mrl}");
                return null;
            }

            if (!_renderWasSet && AvailableDevices.Count > 0)
            {
                await SetCastRenderer(AvailableDevices.First());
            }
            // create new media
            _currentFilePath = mrl;
            string title = isLocal ? Path.GetFileName(mrl) : mrl;
            string url = isLocal ? AppWebServer.GetMediaUrl(_webServer, mrl, seconds) : mrl;

            //TODO: IMPLEMENT YOUTUBE LOGIC
            //TODO: SUBTITLES
            //TODO: CHANGE THE AUDIO TRACK
            var metdata = isVideoFile ? new MovieMetadata
            {
                Title = title,
            } : isMusicFile ? new MusicTrackMediaMetadata
            {
                //TODO: OBTAIN MEDIA INFORMATION
                Title = title,
                AlbumArtist = "Hatsune Miku",
                AlbumName = "Dying tomorrow",
                Artist = "Hatsune Miku",
                SongName = "Unninstall",
            } : new GenericMediaMetadata
            {
                Title = title,
            };
            var media = new MediaInformation
            {
                ContentId = url,
                Metadata = metdata,
                //You have to set the contenttype before hand, with that, the album art of a music file will be shown
                ContentType = _ffmpegService.GetOutputTranscodeMimeType(mrl)
            };

            if (isLocal)
            {
                string imgUrl = AppWebServer.GetPreviewPath(_webServer, GetFirstThumbnail());
                media.Metadata.Images = new List<Image>
                {
                    new Image
                    {
                        Url = imgUrl
                    }
                };
                if (isVideoFile)
                    media.StreamType = StreamType.Live;
                media.Duration = await _ffmpegService.GetFileDuration(mrl, default);
            }

            _logger.Info($"{nameof(StartPlay)}: Trying to load file = {mrl} on seconds = {seconds}");
            var status = await _player.LoadAsync(media, seekedSeconds: seconds);
            _logger.Info($"{nameof(StartPlay)}: File loaded");

            return status;
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

        public void GenerateThumbmnails(string filePath)
        {
            _ffmpegService.KillThumbnailProcess();
            _ffmpegService.GenerateThumbmnails(filePath);
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
            _ffmpegService.KillThumbnailProcess();
            _ffmpegService.KillTranscodeProcess();
            return _player.StopPlaybackAsync();
        }

        public void CleanThemAll()
        {
            Clean();
        }

        public Task<MediaStatus> GoToPosition(string filePath, double position, double totalSeconds)
        {
            if (position >= 0 && position <= 100)
            {
                double seconds = position * totalSeconds / 100;
                if (FileUtils.IsLocalFile(_currentFilePath))
                    return StartPlay(filePath, seconds);

                return _player.SeekAsync(seconds);
            }

            _logger.Warn($"{nameof(GoToPosition)} Cant go to position = {position}");
            return Task.FromResult<MediaStatus>(null);
        }

        public Task<MediaStatus> GoToSeconds(double seconds)
        {
            if (FileUtils.IsLocalFile(_currentFilePath))
                return StartPlay(_currentFilePath, seconds);

            return _player.SeekAsync(seconds);
        }

        public Task<MediaStatus> AddSeconds(double seconds)
        {
            var current = _player.ElapsedSeconds + seconds;
            if (FileUtils.IsLocalFile(_currentFilePath))
            {
                if (current < 0)
                {
                    _logger.Warn($"{nameof(AddSeconds)}: The seconds to add are = {current}. They will be set to 0");
                    current = 0;
                }
                return StartPlay(_currentFilePath, current);
            }
            return _player.SeekAsync(current);
        }

        public Task<double> GetDuration(string mrl, CancellationToken token)
        {
            return _ffmpegService.GetFileDuration(mrl, token);
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

        private void Clean()
        {
            try
            {
                _player.DeviceAdded -= RendererDiscovererItemAdded;
                _player.EndReached -= EndReached;
                _player.TimeChanged -= TimeChanged;
                _player.PositionChanged -= PositionChanged;

                _webServerCancellationToken.Cancel();
                _ffmpegService.KillThumbnailProcess();
                _ffmpegService.KillTranscodeProcess();

                _webServer.Dispose();
                _player.Dispose();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"{nameof(Clean)}: An unknown error ocurred");
            }
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

        public string GetFileName(string mrl)
        {
            if (FileUtils.IsLocalFile(mrl))
                return Path.GetFileName(mrl);
            return mrl;
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

        private Task SetCastRenderer(IReceiver receiver)
        {
            _renderWasSet = true;
            OnCastRendererSet?.Invoke(receiver.Id);
            return _player.ConnectAsync(receiver);
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

        private Task SetNullCastRenderer()
        {
            _renderWasSet = false;
            return _player.DisconnectAsync();
        }

        public string GetBytesReadable(long i)
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
