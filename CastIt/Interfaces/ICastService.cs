﻿using CastIt.Common;
using CastIt.GoogleCast.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CastIt.Interfaces
{
    public interface ICastService
    {
        IList<IReceiver> AvailableDevices { get; }
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
        Func<string> GetSubTitles { get; set; }

        void Init();
        void CleanThemAll();
        void StopRunningProcess();
        Task AddSeconds(
            int videoStreamIndex,
            int audioStreamIndex,
            int subtitleStreamIndex,
            int quality,
            double seconds);
        Task GoToSeconds(
            int videoStreamIndex,
            int audioStreamIndex,
            int subtitleStreamIndex,
            int quality,
            double seconds);
        Task GoToPosition(
            string filePath,
            int videoStreamIndex,
            int audioStreamIndex,
            int subtitleStreamIndex,
            int quality,
            double position,
            double totalSeconds);
        Task<double> SetVolume(double level);
        Task<bool> SetIsMuted(bool isMuted);
        Task StartPlay(
            string mrl,
            int videoStreamIndex,
            int audioStreamIndex,
            int subtitleStreamIndex,
            int quality,
            double seconds = 0);

        string GetFirstThumbnail();
        string GetFirstThumbnail(string filePath);
        string GetThumbnail(int second);
        string GetThumbnail(string filePath, int second);
        Task StopPlayback();
        Task TogglePlayback();
        void GenerateThumbmnails();
        void GenerateThumbmnails(string filePath);
        Task SetCastRenderer(string id);
    }
}