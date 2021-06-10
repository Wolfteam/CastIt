using CastIt.Application.Common;
using CastIt.Domain.Interfaces;
using CastIt.Domain.Models.FFmpeg.Info;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CastIt.Server.Interfaces
{
    public interface ICastService
    {
        List<IReceiver> AvailableDevices { get; }
        OnCastRendererSetHandler OnCastRendererSet { get; set; }
        OnCastableDeviceAddedHandler OnCastableDeviceAdded { get; set; }
        OnCastableDeviceDeletedHandler OnCastableDeviceDeleted { get; set; }
        OnFileLoadedHandler OnFileLoaded { get; set; }
        OnEndReachedHandler OnEndReached { get; set; }
        OnPositionChangedHandler OnPositionChanged { get; set; }
        OnTimeChangedHandler OnTimeChanged { get; set; }
        OnQualitiesChangedHandler QualitiesChanged { get; set; }
        OnPausedHandler OnPaused { get; set; }
        OnDisconnectedHandler OnDisconnected { get; set; }
        OnVolumeChangedHandler OnVolumeChanged { get; set; }
        OnServerMessageHandler OnServerMessage { get; set; }
        Func<string> GetSubTitles { get; set; }
        bool IsPlayingOrPaused { get; }

        public event OnFileLoadingHandler FileLoading;

        Task Init();
        Task CleanThemAll();
        void StopRunningProcess();
        Task AddSeconds(
            int videoStreamIndex,
            int audioStreamIndex,
            int subtitleStreamIndex,
            int quality,
            double seconds,
            FFProbeFileInfo fileInfo);
        Task AddSeconds(double seconds);
        Task GoToSeconds(
            int videoStreamIndex,
            int audioStreamIndex,
            int subtitleStreamIndex,
            int quality,
            double seconds,
            FFProbeFileInfo fileInfo);
        Task GoToSeconds(double seconds);
        Task GoToPosition(
            string filePath,
            int videoStreamIndex,
            int audioStreamIndex,
            int subtitleStreamIndex,
            int quality,
            double position,
            double totalSeconds,
            FFProbeFileInfo fileInfo);
        Task GoToPosition(double position, double totalSeconds);
        Task GoToPosition(double position);
        Task<double> SetVolume(double level);
        Task<bool> SetIsMuted(bool isMuted);
        Task StartPlay(
            string mrl,
            int videoStreamIndex,
            int audioStreamIndex,
            int subtitleStreamIndex,
            int quality,
            FFProbeFileInfo fileInfo,
            double seconds = 0);

        Task<string> GetFirstThumbnail();
        Task<string> GetFirstThumbnail(string filePath);
        Task<string> GetThumbnail();
        Task<string> GetThumbnail(string filePath);
        Task StopPlayback();
        Task TogglePlayback();
        void GenerateThumbnails();
        void GenerateThumbnails(string filePath);
        Task SetCastRenderer(string id);
        Task SetCastRenderer(string host, int port);

        void SendFileLoading();

        void SendEndReached();

        void SendPositionChanged(double position);

        void SendTimeChanged(double seconds);

        void SendPaused();

        void SendDisconnected();

        void SendVolumeLevelChanged(double newValue);

        void SendIsMutedChanged(bool isMuted);

        void SendRendererDiscovererItemAdded(IReceiver item);

        void SendErrorLoadingFile();

        void SendNoDevicesFound();

        void SendNoInternetConnection();

        void SendPlayListNotFound();

        void SendFileNotFound();

        void SendInvalidRequest();
    }
}