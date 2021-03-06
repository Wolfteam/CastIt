﻿using CastIt.Domain.Interfaces;
using CastIt.GoogleCast.Models.Events;
using CastIt.GoogleCast.Models.Media;
using CastIt.GoogleCast.Models.Receiver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CastIt.GoogleCast.Interfaces
{
    public interface IPlayer : IDisposable
    {
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

        public bool IsPlaying { get; }
        public bool IsPaused { get; }
        public string CurrentContentId { get; }
        public double CurrentMediaDuration { get; }
        public double ElapsedSeconds { get; }
        public double PlayedPercentage { get; }
        public double CurrentVolumeLevel { get; }
        public bool IsMuted { get; }

        void ListenForDevices();

        Task ConnectAsync();

        Task ConnectAsync(string host, int port);

        Task ConnectAsync(IReceiver receiver);

        Task<List<IReceiver>> GetDevicesAsync(TimeSpan scanTime);

        Task<ReceiverStatus> SetVolumeAsync(float level);

        Task<ReceiverStatus> SetIsMutedAsync(bool isMuted);

        Task DisconnectAsync();

        Task<MediaStatus> LoadAsync(
            MediaInformation media,
            bool autoPlay = true,
            double seekedSeconds = 0,
            params int[] activeTrackIds);

        Task<MediaStatus> GetMediaStatusAsync();

        Task<MediaStatus> PlayAsync();

        Task<MediaStatus> PauseAsync();

        Task<MediaStatus> StopPlaybackAsync();

        Task<MediaStatus> SeekAsync(double seconds);
    }
}
