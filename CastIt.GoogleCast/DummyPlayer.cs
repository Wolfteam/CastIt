using CastIt.GoogleCast.Interfaces;
using CastIt.GoogleCast.Models;
using CastIt.GoogleCast.Models.Events;
using CastIt.GoogleCast.Models.Media;
using CastIt.GoogleCast.Models.Receiver;
using CastIt.GoogleCast.Shared.Device;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CastIt.GoogleCast
{
    internal class DummyPlayer : IPlayer
    {
        #region Members
        private CancellationTokenSource _listenerToken;
        private List<IReceiver> _devices = new List<IReceiver>
        {
            new Receiver
            {
                FriendlyName = "Dummy",
                Host = "192.168.1.101",
                IsConnected = true,
                Id = "xxxxxx",
                Port = 8080,
                Type = "N/A"
            }
        };

        #endregion

        #region Events
        public event EventHandler Disconnected;
        public event EventHandler<DeviceAddedArgs> DeviceAdded;
        public event EventHandler FileLoading;
        public event EventHandler FileLoaded;
        public event EventHandler<double> TimeChanged;
        public event EventHandler<double> PositionChanged;
        public event EventHandler Paused;
        public event EventHandler EndReached;
        public event EventHandler<double> VolumeLevelChanged;
        public event EventHandler<bool> IsMutedChanged;
        public event EventHandler LoadFailed;
        #endregion


        #region Properties
        public bool IsPlaying { get; set; }
        public bool IsPaused { get; set; }
        public bool IsPlayingOrPaused
            => IsPlaying || IsPaused;
        public string CurrentContentId { get; set; }
        public double CurrentMediaDuration { get; set; }
        public double ElapsedSeconds { get; set; }
        public double PlayedPercentage
        {
            get
            {
                if (CurrentMediaDuration > 0)
                    return ElapsedSeconds * 100 / CurrentMediaDuration;
                return 0;
            }
        }
        public double CurrentVolumeLevel { get; set; }
        public bool IsMuted { get; set; }

        public PlayerStatus State => new PlayerStatus(this);
        private SemaphoreSlim ListenMediaChangesSemaphoreSlim { get; } = new SemaphoreSlim(1, 1);
        #endregion

        public void ListenForDevices()
        {
            DeviceAdded?.Invoke(this, new DeviceAddedArgs(_devices.First()));
        }

        public Task ConnectAsync()
        {
            return Task.Delay(TimeSpan.FromSeconds(2));
        }

        public Task ConnectAsync(string host, int port)
        {
            return Task.Delay(TimeSpan.FromSeconds(2));
        }

        public Task ConnectAsync(IReceiver receiver)
        {
            return Task.Delay(TimeSpan.FromSeconds(2));
        }

        public void Dispose()
        {
        }

        public async Task<MediaStatus> LoadAsync(MediaInformation media, bool autoPlay = true, double seekedSeconds = 0, params int[] activeTrackIds)
        {
            CleanLoadedFile();
            CancelAndSetListenerToken();

            FileLoading?.Invoke(this, EventArgs.Empty);
            await Task.Delay(TimeSpan.FromSeconds(2));

            CurrentContentId = media.ContentId;
            CurrentMediaDuration = media.Duration ?? 0;
            CurrentVolumeLevel = 1;
            IsMuted = false;
            ElapsedSeconds = 0;

            TriggerTimeEvents();
            IsPlaying = true;
            IsPaused = false;

            ListenForMediaChanges(_listenerToken.Token);

            FileLoaded?.Invoke(this, EventArgs.Empty);
            return new MediaStatus();
        }

        public Task<MediaStatus> GetMediaStatusAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<MediaStatus> PlayAsync()
        {
            IsPlaying = true;
            IsPaused = false;
            await Task.Delay(TimeSpan.FromSeconds(2));
            return new MediaStatus();
        }

        public async Task<MediaStatus> PauseAsync()
        {
            IsPlaying = false;
            await Task.Delay(TimeSpan.FromSeconds(2));
            Paused?.Invoke(this, EventArgs.Empty);
            return new MediaStatus();
        }

        public async Task<MediaStatus> StopPlaybackAsync()
        {
            CleanLoadedFile();
            CurrentContentId = null;
            IsPlaying = false;
            await Task.Delay(TimeSpan.FromSeconds(2));
            CancelAndSetListenerToken(false);
            return new MediaStatus();
        }

        public async Task<MediaStatus> SeekAsync(double seconds)
        {
            ElapsedSeconds += seconds;
            await Task.Delay(TimeSpan.FromSeconds(2));
            return new MediaStatus();
        }

        public async Task<List<IReceiver>> GetDevicesAsync(TimeSpan scanTime)
        {
            await Task.Delay(scanTime);
            return _devices;
        }

        public async Task<ReceiverStatus> SetVolumeAsync(float level)
        {
            CurrentVolumeLevel = level;
            await Task.Delay(TimeSpan.FromSeconds(2));
            VolumeLevelChanged?.Invoke(this, CurrentVolumeLevel);
            return new ReceiverStatus
            {
                Volume = new Volume
                {
                    IsMuted = IsMuted,
                    Level = (float)CurrentVolumeLevel,
                }
            };
        }

        public async Task<ReceiverStatus> SetIsMutedAsync(bool isMuted)
        {
            IsMuted = isMuted;
            await Task.Delay(TimeSpan.FromSeconds(2));
            IsMutedChanged?.Invoke(this, IsMuted);
            return new ReceiverStatus
            {
                Volume = new Volume
                {
                    IsMuted = IsMuted,
                    Level = (float)CurrentVolumeLevel,
                }
            };
        }

        public Task DisconnectAsync()
        {
            CancelAndSetListenerToken(false);
            CleanLoadedFile();
            OnDisconnect(this, EventArgs.Empty);
            return Task.Delay(TimeSpan.FromSeconds(2));
        }

        private void OnDisconnect(object sender, EventArgs e)
        {
            Disconnected?.Invoke(sender, e);
            IsPlaying = false;
        }

        private void TriggerTimeEvents()
        {
            TimeChanged?.Invoke(this, ElapsedSeconds);
            PositionChanged?.Invoke(this, PlayedPercentage);
        }

        private void CleanLoadedFile()
        {
            CurrentContentId = null;
            CurrentMediaDuration = 0;
            ElapsedSeconds = 0;
            IsPlaying = false;
            IsPaused = false;
        }

        private void CancelAndSetListenerToken(bool createNewToken = true)
        {
            if (_listenerToken?.IsCancellationRequested == false)
                _listenerToken.Cancel();

            if (createNewToken)
                _listenerToken = new CancellationTokenSource();
        }

        private async void ListenForMediaChanges(CancellationToken token)
        {
            try
            {
                await ListenMediaChangesSemaphoreSlim.WaitAsync(token);
                await Task.Run(async () =>
                {
                    while (!token.IsCancellationRequested)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(2), token);

                        ElapsedSeconds++;
                        if (IsPaused)
                        {
                            IsPaused = true;
                            IsPlaying = false;
                            Paused?.Invoke(this, EventArgs.Empty);
                            continue;
                        }
                        IsPlaying = true;
                        IsPaused = false;
                        TriggerTimeEvents();
                        //If CurrentMediaDuration  <= 0, that means that a live streaming is being played
                        if (CurrentMediaDuration > 0 && Math.Round(ElapsedSeconds, 4) >= Math.Round(CurrentMediaDuration, 4))
                        {
                            CleanLoadedFile();
                            CancelAndSetListenerToken(false);
                            EndReached?.Invoke(this, EventArgs.Empty);
                            break;
                        }
                    }
                }, token).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                if (e is TaskCanceledException or OperationCanceledException)
                    return;
            }
            finally
            {
                if (ListenMediaChangesSemaphoreSlim.CurrentCount == 0)
                    ListenMediaChangesSemaphoreSlim.Release();
            }
        }
    }
}
