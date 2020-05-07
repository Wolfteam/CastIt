using CastIt.Common;
using CastIt.Common.Utils;
using CastIt.Interfaces;
using CastIt.Models;
using LibVLCSharp.Shared;
using MvvmCross.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CastIt.Services
{
    public class CastService : ICastService
    {
        private const string FfmpegPath = "C:/ffmpeg/ffmpeg.exe";

        private readonly IMvxLog _logger;
        private readonly HashSet<RendererItem> _rendererItems = new HashSet<RendererItem>();
        private LibVLC _libVLC;
        private MediaPlayer _mediaPlayer;
        private RendererDiscoverer _rendererDiscoverer;
        private Media _currentMedia;
        private bool _renderWasSet;
        private string _currentFilePath;

        public List<CastableDevice> AvailableDevices => _rendererItems.Select(r => new CastableDevice
        {
            Name = r.Name,
            Type = r.Type
        }).ToList();

        public OnCastableDeviceAddedHandler OnCastableDeviceAdded { get; set; }
        public OnPositionChangedHandler OnPositionChanged { get; set; }
        public OnTimeChangedHandler OnTimeChanged { get; set; }
        public OnEndReachedHandler OnEndReached { get; set; }

        public CastService(IMvxLogProvider logProvider)
        {
            _logger = logProvider.GetLogFor<CastService>();
        }

        public void Init()
        {
            _logger.Info($"{nameof(Init)}: Initializing all...");
            // load native libvlc libraries
            Core.Initialize();
            // create core libvlc object
            _libVLC = new LibVLC("--verbose=3");
            //_libVLC.Log += Log;

            DiscoverChromecasts();

            _mediaPlayer = new MediaPlayer(_libVLC);
            _mediaPlayer.Playing += Playing;
            _mediaPlayer.PositionChanged += PositionChanged;
            _mediaPlayer.TimeChanged += TimeChanged;
            _mediaPlayer.EndReached += EndReached;
            _mediaPlayer.EnableHardwareDecoding = true;

            _logger.Info($"{nameof(Init)}: Initialize completed");
        }

        private void Log(object sender, LogEventArgs args)
        {
            Debug.WriteLine(args.Message);
        }

        public async Task StartPlay(string mrl)
        {
            bool isLocal = IsLocalFile(mrl);
            bool isUrlFile = IsUrlFile(mrl);
            if (!isLocal && !isUrlFile)
            {
                Debug.WriteLine($"Invalid = {mrl}");
                return;
            }
            //TODO: SHOULD I CHECK IF _libVLC.RendererList IS EMPTY ?
            if (!_rendererItems.Any())
            {
                Debug.WriteLine($"No renders were found");
                //return;
            }
            //TODO: MOVE THIS SET TO A METHOD
            //TODO: YOU WILL PROBABLY NEED TO CALL THIS METHOD FROM ANOTHER THREAD
            if (!_renderWasSet && _rendererItems.Any())
            {
                _mediaPlayer.SetRenderer(_rendererItems.First());
                _renderWasSet = true;
            }

            // create new media
            _currentFilePath = mrl;
            _currentMedia?.Dispose();
            _currentMedia = new Media(
                _libVLC,
                mrl,
                isLocal ? FromType.FromPath : FromType.FromLocation);

            await _currentMedia.Parse(MediaParseOptions.ParseNetwork | MediaParseOptions.ParseLocal);

            // start the playback
            ThreadPool.QueueUserWorkItem(_ =>
            {
                if (IsLocalFile(mrl))
                    _mediaPlayer.Play(_currentMedia);
                else
                    _mediaPlayer.Play(_currentMedia.SubItems.First());
            });
        }

        public string GetFirstThumbnail()
            => GetThumbnail(3);

        public string GetFirstThumbnail(string filePath)
            => GetThumbnail(filePath, 3);

        public string GetThumbnail(int second)
        {
            return GetThumbnail(_currentFilePath, second);
        }

        public string GetThumbnail(string filePath, int second)
        {
            if (!IsLocalFile(filePath))
            {
                return null;
            }

            var filename = Path.GetFileName(filePath);
            var ext = Path.GetExtension(filePath);
            var thumbnailPath = FileUtils.GetThumbnailFilePath(filename, second);
            if (File.Exists(thumbnailPath))
            {
                return thumbnailPath;
            }

            //-i filename(the source video's file path)
            //- deinterlace(converts interlaced video to non - interlaced form)
            //- an(audio recording is disabled; we don't need it for thumbnails)
            //- ss position(input video is decoded and dumped until the timestamp reaches position, our thumbnail's position in seconds)
            //- t duration(the output file stops writing after duration seconds)
            //-y(overwrites the output file if it already exists)
            //-s widthxheight(size of the frame in pixels)
            string cmd;

            if (AppConstants.AllowedMusicFormats.Contains(ext.ToLower()))
            {
                cmd = $"{FfmpegPath} -y -i " +
                    '"' + filePath + '"'
                    + $" -an -filter:v scale=200:150 "
                    + '"' + thumbnailPath + '"';
            }
            else
            {
                cmd = $"{FfmpegPath} -y -i " +
                    '"' + filePath + '"'
                    + $" -an -ss {second} -vframes 1 -s 200x150 "
                    + '"' + thumbnailPath + '"';
            }

            var startInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                FileName = "cmd.exe",
                Arguments = "/C " + cmd
            };

            var process = new Process
            {
                StartInfo = startInfo
            };

            process.Start();
            process.WaitForExit();

            return thumbnailPath;
        }

        public void GenerateThumbmnails()
            => GenerateThumbmnails(_currentFilePath);

        public void GenerateThumbmnails(string filePath)
        {
            if (!IsLocalFile(filePath))
            {
                return;
            }
            var filename = Path.GetFileName(filePath);
            var ext = Path.GetExtension(filePath);

            if (AppConstants.AllowedMusicFormats.Contains(ext.ToLower()))
            {
                return;
            }
            var thumbnailPath = FileUtils.GetPreviewThumbnailFilePath(filename);
            var cmd = $"{FfmpegPath} -y -i " +
                    '"' + filePath + '"'
                    + $" -an -vf fps=1/5 -s 200x150 "
                    + '"' + thumbnailPath + '"';

            var startInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                FileName = "cmd.exe",
                Arguments = "/C " + cmd
            };

            var process = new Process
            {
                StartInfo = startInfo
            };

            process.Start();
            process.WaitForExit();
        }

        public void TogglePlayback()
        {
            if (_mediaPlayer.IsPlaying)
            {
                _mediaPlayer.Pause();
            }
            else
            {
                _mediaPlayer.Play();
            }
        }

        public void StopPlayback()
        {
            ThreadPool.QueueUserWorkItem(_ => _mediaPlayer.Stop());
        }

        public void CleanThemAll()
        {
            Clean();
        }

        public void GoToPosition(float position)
        {
            try
            {
                position /= 100;
                if (position >= 0 && position <= 1)
                {
                    ThreadPool.QueueUserWorkItem(_ => _mediaPlayer.Position = position);
                }
            }
            catch (Exception ex)
            {
            }
        }

        public void GoToSeconds(long seconds)
        {
            try
            {
                ThreadPool.QueueUserWorkItem(_ => _mediaPlayer.Time = seconds * 1000);
            }
            catch (Exception ex)
            {
            }
        }

        public void AddSeconds(long seconds)
        {
            try
            {
                //Time is expresed in ms
                var current = _mediaPlayer.Time / 1000;
                current += seconds;
                if (current < 0)
                {
                    current = 0;
                }

                ThreadPool.QueueUserWorkItem(_ => _mediaPlayer.Time = current * 1000);
            }
            catch (Exception ex)
            {
            }
        }

        public bool DiscoverChromecasts()
        {
            // choose the correct service discovery protocol depending on the host platform
            // Apple platforms use the Bonjour protocol
            var renderer = _libVLC.RendererList.FirstOrDefault();

            _rendererDiscoverer?.Dispose();

            // create a renderer discoverer
            _rendererDiscoverer = new RendererDiscoverer(_libVLC, renderer.Name);

            // register callback when a new renderer is found
            _rendererDiscoverer.ItemAdded += RendererDiscoverer_ItemAdded;

            // start discovery on the local network
            bool started = _rendererDiscoverer.Start();

            return started;
        }

        public async Task<long> GetDuration(string mrl)
        {
            bool isLocal = IsLocalFile(mrl);
            bool isUrl = IsUrlFile(mrl);
            if (!isLocal && !isUrl)
            {
                Debug.WriteLine($"Invalid mrl = {mrl}");
                return -1;
            }

            using var media = new Media(_libVLC, mrl, isLocal ? FromType.FromPath : FromType.FromLocation);
            try
            {
                var status = await media.Parse(MediaParseOptions.ParseNetwork | MediaParseOptions.ParseLocal);
                if (status == MediaParsedStatus.Done)
                    return media.Duration / 1000;
                return 0;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public bool IsLocalFile(string mrl)
        {
            return File.Exists(mrl);
        }

        public bool IsUrlFile(string mrl)
        {
            return Uri.TryCreate(mrl, UriKind.Absolute, out Uri uriResult) &&
                (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        private void Playing(object sender, EventArgs e)
        {
            Debug.WriteLine("Playing");
        }

        private void EndReached(object sender, EventArgs e)
        {
            OnEndReached?.Invoke();
        }

        private void PositionChanged(object sender, MediaPlayerPositionChangedEventArgs e)
        {
            var position = e.Position * 100;
            if (position > 100)
            {
                position = 100;
            }
            OnPositionChanged?.Invoke(position);
        }

        private void TimeChanged(object sender, MediaPlayerTimeChangedEventArgs e)
        {
            OnTimeChanged?.Invoke(e.Time / 1000);
        }

        private void Clean()
        {
            _libVLC.Log -= Log;
            _mediaPlayer?.Stop();
            _currentMedia?.Dispose();
            _rendererDiscoverer?.Stop();
            _mediaPlayer?.Dispose();
            _rendererDiscoverer?.Dispose();
            _libVLC?.Dispose();
        }

        private void RendererDiscoverer_ItemAdded(object sender, RendererDiscovererItemAddedEventArgs e)
        {
            Debug.WriteLine($"New item discovered: {e.RendererItem.Name} of type {e.RendererItem.Type}");
            if (e.RendererItem.CanRenderVideo)
                Debug.WriteLine("Can render video");
            if (e.RendererItem.CanRenderAudio)
                Debug.WriteLine("Can render audio");

            // add newly found renderer item to local collection
            _rendererItems.Add(e.RendererItem);

            OnCastableDeviceAdded?.Invoke(new CastableDevice
            {
                Name = e.RendererItem.Name,
                Type = e.RendererItem.Type
            });
        }

        public string GetFileName(string mrl)
        {
            if (IsLocalFile(mrl))
                return Path.GetFileName(mrl);
            return mrl;
        }

        public string GetExtension(string mrl)
        {
            if (IsLocalFile(mrl))
                return Path.GetExtension(mrl).ToUpper();
            if (IsUrlFile(mrl))
                return "WEB";
            return "N/A";
        }

        public string GetFileSizeString(string mrl)
        {
            if (!IsLocalFile(mrl))
                return "N/A";
            var fileInfo = new FileInfo(mrl);
            var sizeInBytes = fileInfo.Length;
            float sizeInMb = sizeInBytes / 1024F / 1024F;
            return Math.Round(sizeInMb, 2) + " MB";
        }

        public bool IsVideoFile(string mrl)
        {
            if (!IsLocalFile(mrl))
                return false;
            return IsVideoOrMusicFile(mrl, true);
        }

        public bool IsMusicFile(string mrl)
        {
            if (!IsLocalFile(mrl))
                return false;
            return IsVideoOrMusicFile(mrl, false);
        }

        private bool IsVideoOrMusicFile(string mrl, bool checkForVideo)
        {
            string ext = Path.GetExtension(mrl);
            if (checkForVideo)
                return AppConstants.AllowedVideoFormats.Contains(ext.ToLower(), StringComparer.OrdinalIgnoreCase);
            return AppConstants.AllowedMusicFormats.Contains(ext.ToLower(), StringComparer.OrdinalIgnoreCase);
        }
    }
}
