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
        private readonly IMvxLog _logger;
        private readonly HashSet<RendererItem> _rendererItems = new HashSet<RendererItem>();
        private readonly string _ffmpegPath;
        private LibVLC _libVLC;
        private MediaPlayer _mediaPlayer;
        private RendererDiscoverer _rendererDiscoverer;
        private Media _currentMedia;
        private bool _renderWasSet;
        private string _currentFilePath;

        private bool _checkGenerateThumbnailProcess;
        private readonly Process _generateThumbnailProcess;
        private bool _checkGenerateAllThumbnailsProcess;
        private readonly Process _generateAllThumbnailsProcess;

        public List<CastableDevice> AvailableDevices => _rendererItems.Select(r => new CastableDevice
        {
            Name = r.Name,
            Type = r.Type
        }).ToList();
        public OnCastRendererSetHandler OnCastRendererSet { get; set; }
        public OnCastableDeviceAddedHandler OnCastableDeviceAdded { get; set; }
        public OnCastableDeviceDeletedHandler OnCastableDeviceDeleted { get; set; }
        public OnPositionChangedHandler OnPositionChanged { get; set; }
        public OnTimeChangedHandler OnTimeChanged { get; set; }
        public OnEndReachedHandler OnEndReached { get; set; }

        public CastService(IMvxLogProvider logProvider)
        {
            _logger = logProvider.GetLogFor<CastService>();
            _ffmpegPath = FileUtils.GetFFMpegPath();

            var startInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                CreateNoWindow = true,
                FileName = "cmd.exe"
            };

            _generateThumbnailProcess = new Process
            {
                StartInfo = startInfo
            };

            _generateAllThumbnailsProcess = new Process
            {
                StartInfo = startInfo
            };
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
            _mediaPlayer.EncounteredError += EncounteredError;
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
            KillThumbnailProcess();
            bool isLocal = IsLocalFile(mrl);
            bool isUrlFile = IsUrlFile(mrl);
            if (!isLocal && !isUrlFile)
            {
                _logger.Warn($"{nameof(StartPlay)}: Invalid = {mrl}. Its not a local file and its not a url file");
                return;
            }

            if (_rendererItems.Count == 0)
            {
                _logger.Warn($"{nameof(StartPlay)}: No renders were found, file = {mrl} will be played locally");
            }

            if (!_renderWasSet && _rendererItems.Count > 0)
            {
                SetCastRenderer(_rendererItems.First());
            }

            // create new media
            _currentFilePath = mrl;
            _currentMedia?.Dispose();
            _currentMedia = new Media(
                _libVLC,
                mrl,
                isLocal ? FromType.FromPath : FromType.FromLocation);

            await _currentMedia.Parse(MediaParseOptions.ParseNetwork | MediaParseOptions.ParseLocal);

            await Task.Run(() =>
            {
                if (IsLocalFile(mrl))
                    _mediaPlayer.Play(_currentMedia);
                else if (_currentMedia.SubItems.Any())
                    _mediaPlayer.Play(_currentMedia.SubItems.First());
                else
                    _mediaPlayer.Play(_currentMedia);
            });
        }

        public string GetFirstThumbnail()
            => GetThumbnail(3);

        public string GetFirstThumbnail(string filePath)
            => GetThumbnail(filePath, 3);

        public string GetThumbnail(int second)
            => GetThumbnail(_currentFilePath, second);

        public string GetThumbnail(string filePath, int second)
        {
            if (!IsLocalFile(filePath))
            {
                _logger.Warn($"{nameof(GetThumbnail)}: Cant get thumbnail for file = {filePath}. Its not a local file");
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
            //- ss position(input video is decoded and dumped until the timestamp reaches position, 
            //     our thumbnail's position in seconds)
            //- t duration(the output file stops writing after duration seconds)
            //-y(overwrites the output file if it already exists)
            //-s widthxheight(size of the frame in pixels)
            //https://stackoverflow.com/questions/48425905/cmd-exe-via-system-diagnostics-process-cannot-parse-an-argument-with-spaces
            string cmd;

            if (AppConstants.AllowedMusicFormats.Contains(ext.ToLower(), StringComparer.OrdinalIgnoreCase))
            {
                cmd = $@"/C "" ""{_ffmpegPath}"" -y -i ""{filePath}"" -an -filter:v scale=200:150 ""{thumbnailPath}"" "" && exit";
            }
            else
            {
                cmd = $@"/C -ss {second} -an "" ""{_ffmpegPath}"" -y -i ""{filePath}"" -vframes 1 -s 200x150 ""{thumbnailPath}"" "" && exit";
            }
            try
            {
                _logger.Info($"{nameof(GetThumbnail)}: Generating first thumbnail for file = {filePath}. Cmd = {cmd}");
                _checkGenerateThumbnailProcess = true;
                _generateThumbnailProcess.StartInfo.Arguments = cmd;
                _generateThumbnailProcess.Start();
                _generateThumbnailProcess.WaitForExit();
                _logger.Info($"{nameof(GetThumbnail)}: First thumbnail was succesfully generated for file = {filePath}");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"{nameof(GenerateThumbmnails)}: Unknown error occurred");
            }
            finally
            {
                _checkGenerateThumbnailProcess = false;
            }
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

            if (AppConstants.AllowedMusicFormats.Contains(ext.ToLower(), StringComparer.OrdinalIgnoreCase))
            {
                _logger.Info(
                    $"{nameof(GenerateThumbmnails)}: File = {filePath} is a music one, " +
                    $"so we wont generate thumbnails for it");
                return;
            }
            var thumbnailPath = FileUtils.GetPreviewThumbnailFilePath(filename);
            var cmd = $@"/C "" ""{_ffmpegPath}"" -y -i ""{filePath}"" -an -vf fps=1/5 -s 200x150 ""{thumbnailPath}"" "" && exit";
            try
            {
                _logger.Info($"{nameof(GenerateThumbmnails)}: Generating all thumbnails for file = {filePath}. Cmd = {cmd}");
                _checkGenerateAllThumbnailsProcess = true;
                _generateThumbnailProcess.StartInfo.Arguments = cmd;
                _generateAllThumbnailsProcess.Start();
                _generateAllThumbnailsProcess.WaitForExit();
                _logger.Info($"{nameof(GenerateThumbmnails)}: All thumbnails were succesfully generated for file = {filePath}");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"{nameof(GenerateThumbmnails)}: Unknown error occurred");
            }
            finally
            {
                _checkGenerateAllThumbnailsProcess = false;
            }
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

        public Task StopPlayback()
        {
            KillThumbnailProcess();
            return Task.Run(() => _mediaPlayer.Stop());
        }

        public void CleanThemAll()
        {
            Clean();
        }

        public Task GoToPosition(float position)
        {
            try
            {
                position /= 100;
                if (position >= 0 && position <= 1)
                {
                    return Task.Run(() => _mediaPlayer.Position = position);
                }
                else
                {
                    _logger.Warn($"{nameof(GoToPosition)} Cant go to position = {position}");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"{nameof(GoToPosition)}: Unknown error while goin to position = {position}");
            }

            return Task.CompletedTask;
        }

        public Task GoToSeconds(long seconds)
        {
            try
            {
                return Task.Run(() => _mediaPlayer.Time = seconds * 1000);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"{nameof(GoToSeconds)}: Unknown error while going to seconds = {seconds * 1000}");
            }
            return Task.CompletedTask;
        }

        public Task AddSeconds(long seconds)
        {
            try
            {
                //Time is expresed in ms
                var current = _mediaPlayer.Time / 1000;
                current += seconds;
                if (current < 0)
                {
                    _logger.Warn($"{nameof(AddSeconds)}: The seconds to add are = {current}. They will be set to 0");
                    current = 0;
                }
                return Task.Run(() => _mediaPlayer.Time = current * 1000);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"{nameof(AddSeconds)}: Unknown error while adding seconds = {seconds}");
            }
            return Task.CompletedTask;
        }

        public bool DiscoverChromecasts()
        {
            // choose the correct service discovery protocol depending on the host platform
            // Apple platforms use the Bonjour protocol
            var renderer = _libVLC.RendererList.FirstOrDefault();
            _logger.Info($"{nameof(DiscoverChromecasts)}: The renderer to be used is = {renderer.Name}");

            _rendererDiscoverer?.Dispose();

            // create a renderer discoverer
            _rendererDiscoverer = new RendererDiscoverer(_libVLC, renderer.Name);

            // register callback when a new renderer is found
            _rendererDiscoverer.ItemAdded += RendererDiscovererItemAdded;
            _rendererDiscoverer.ItemDeleted += RendererDiscovererItemDeleted;

            // start discovery on the local network
            return _rendererDiscoverer.Start();
        }

        public async Task<long> GetDuration(string mrl, CancellationToken cancellationToken = default)
        {
            bool isLocal = IsLocalFile(mrl);
            bool isUrl = IsUrlFile(mrl);
            if (!isLocal && !isUrl)
            {
                _logger.Warn($"{nameof(GetDuration)}: Couldnt retrieve duration for file = {mrl}. It is not a local / url file");
                return -1;
            }

            using var media = new Media(_libVLC, mrl, isLocal ? FromType.FromPath : FromType.FromLocation);
            try
            {
                var status = await media.Parse(
                    MediaParseOptions.ParseNetwork | MediaParseOptions.ParseLocal,
                    cancellationToken: cancellationToken);
                if (status == MediaParsedStatus.Done)
                    return media.Duration / 1000;
                return 0;
            }
            catch (Exception ex)
            {
                if (!(ex is TaskCanceledException))
                    _logger.Error(ex, $"{nameof(GetDuration)}: Unknown error while trying to parse file = {mrl}");
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

        private void EncounteredError(object sender, EventArgs e)
        {
            _logger.Error($"{nameof(EncounteredError)}: Unknown error {e}");
        }

        private void Clean()
        {
            try
            {
                _libVLC.Log -= Log;
                _mediaPlayer.EncounteredError -= EncounteredError;
                _mediaPlayer.PositionChanged -= PositionChanged;
                _mediaPlayer.TimeChanged -= TimeChanged;
                _mediaPlayer.EndReached -= EndReached;
                _mediaPlayer?.Stop();
                _currentMedia?.Dispose();
                _rendererDiscoverer.ItemAdded -= RendererDiscovererItemAdded;
                _rendererDiscoverer.ItemDeleted -= RendererDiscovererItemDeleted;
                _rendererDiscoverer?.Stop();
                _mediaPlayer?.Dispose();
                _rendererDiscoverer?.Dispose();
                _libVLC?.Dispose();

                _libVLC = null;
                _mediaPlayer = null;
                _currentMedia = null;
                _rendererDiscoverer = null;

                KillThumbnailProcess();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"{nameof(Clean)}: An unknown error ocurred");
            }
        }

        private void RendererDiscovererItemAdded(object sender, RendererDiscovererItemAddedEventArgs e)
        {
            _logger.Info(
                $"{nameof(RendererDiscovererItemAdded)}: New item discovered: " +
                $"{e.RendererItem.Name} of type {e.RendererItem.Type}");
            _rendererItems.Add(e.RendererItem);

            OnCastableDeviceAdded?.Invoke(new CastableDevice
            {
                Name = e.RendererItem.Name,
                Type = e.RendererItem.Type
            });
        }

        private void RendererDiscovererItemDeleted(object sender, RendererDiscovererItemDeletedEventArgs e)
        {
            _logger.Info(
                $"{nameof(RendererDiscovererItemAdded)}: Item removed: " +
                $"{e.RendererItem.Name} of type {e.RendererItem.Type}");
            _rendererItems.Remove(e.RendererItem);
            OnCastableDeviceDeleted?.Invoke(new CastableDevice
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

        private void KillThumbnailProcess()
        {
            try
            {
                if (_checkGenerateAllThumbnailsProcess &&
                    !_generateAllThumbnailsProcess.HasExited)
                {
                    _logger.Info($"{nameof(KillThumbnailProcess)}: Killing the generate all thumbnails process");
                    _generateAllThumbnailsProcess.Kill(true);
                    _generateAllThumbnailsProcess.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"{nameof(KillThumbnailProcess)}: Could not stop the generate all thumbnails process");
            }

            try
            {
                if (_checkGenerateThumbnailProcess &&
                    !_generateThumbnailProcess.HasExited)
                {
                    _logger.Info($"{nameof(KillThumbnailProcess)}: Killing the generate thumbnail process");
                    _generateThumbnailProcess.Kill(true);
                    _generateThumbnailProcess.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"{nameof(KillThumbnailProcess)}: Could not stop the generate thumbnail process");
            }
        }

        private void SetCastRenderer(RendererItem renderer)
        {
            _mediaPlayer.SetRenderer(renderer);
            _renderWasSet = true;
            OnCastRendererSet?.Invoke(renderer.Name, renderer.Type);
        }

        public void SetCastRenderer(string name, string type)
        {
            if (string.IsNullOrEmpty(name) && string.IsNullOrEmpty(type))
            {
                SetNullCastRenderer();
                return;
            }
            var renderer = _rendererItems.FirstOrDefault(d => d.Name == name && d.Type == type);
            if (renderer is null)
            {
                SetNullCastRenderer();
                return;
            }

            SetCastRenderer(renderer);
        }

        private void SetNullCastRenderer()
        {
            _mediaPlayer.SetRenderer(null);
            _renderWasSet = false;
        }
    }
}
