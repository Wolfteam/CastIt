﻿using CastIt.Domain.Enums;
using CastIt.GoogleCast.Shared.Device;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CastIt.Server.Interfaces
{
    public interface ICastService
    {
        List<IReceiver> AvailableDevices { get; }
        bool IsPlayingOrPaused { get; }

        Task Init();
        Task StopAsync();
        Task SavePlayListAndFileChanges();
        Task AddSeconds(double seconds);
        Task GoToSeconds(double seconds);
        Task GoToPosition(double position);
        Task<double> SetVolume(double level);
        Task<bool> SetIsMuted(bool isMuted);
        Task TogglePlayback();
        Task SetCastRenderer(string id);
        Task SetCastRenderer(string host, int port);

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

        void SendServerMsg(AppMessageType type);

        Task RefreshCastDevices(TimeSpan? ts);
    }
}