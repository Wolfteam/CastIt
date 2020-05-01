using CastIt.Common;
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
        private LibVLC _libVLC;
        private MediaPlayer _mediaPlayer;
        private RendererDiscoverer _rendererDiscoverer;
        private Media _currentMedia;
        private bool _renderWasSet;

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
            _libVLC = new LibVLC();
            _libVLC.Log += Log;

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

        public void StartPlay(string mrl, bool isLocal)
        {
            if (isLocal && !File.Exists(mrl))
            {
                Debug.WriteLine($"Path = {mrl} doesnt exists");
                return;
            }
            //TODO: SHOULD I CHECK IF _libVLC.RendererList IS EMPTY ?
            if (!_rendererItems.Any())
            {
                Debug.WriteLine($"No renders were found");
                return;
            }
            //TODO: MOVE THIS SET TO A METHOD
            //TODO: YOU WILL PROBABLY NEED TO CALL THIS METHOD FROM ANOTHER THREAD
            if (!_renderWasSet)
            {
                _mediaPlayer.SetRenderer(_rendererItems.First());
                _renderWasSet = true;
            }

            // create new media
            _currentMedia?.Dispose();
            _currentMedia = new Media(
                _libVLC,
                mrl,
                isLocal ? FromType.FromPath : FromType.FromLocation);

            // start the playback
            ThreadPool.QueueUserWorkItem(_ => _mediaPlayer.Play(_currentMedia));
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

        /// <summary>
        /// Goes to a specific position
        /// </summary>
        /// <param name="position">A value between 0 and 100</param>
        public void GoToPosition(float position)
        {
            try
            {
                position /= 100;
                System.Diagnostics.Debug.WriteLine($"Postion will be set to = {position}");
                if (position >= 0 && position <= 1)
                {
                    ThreadPool.QueueUserWorkItem(_ => _mediaPlayer.Position = position);
                }
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
                System.Diagnostics.Debug.WriteLine($"Adding = {seconds}. New value will be set to = {current}");

                if (current >= 0)
                {
                    ThreadPool.QueueUserWorkItem(_ => _mediaPlayer.Time = current * 1000);
                }
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

        public async Task<long> GetDuration(string mrl, bool isLocal)
        {
            using var media = new Media(_libVLC, mrl, isLocal ? FromType.FromPath : FromType.FromLocation);
            try
            {
                var status = await media.Parse(MediaParseOptions.ParseNetwork | MediaParseOptions.ParseLocal);
                if (status == MediaParsedStatus.Done)
                    return media.Duration / 1024;
                return 0;
            }
            catch (Exception)
            {
                return -1;
            }
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
    }
}
