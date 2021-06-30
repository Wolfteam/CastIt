using CastIt.Domain.Dtos.Responses;
using CastIt.Domain.Enums;
using CastIt.Domain.Interfaces;
using System.Collections.Generic;

namespace CastIt.Application.Common
{
    public delegate void OnCastRendererSetHandler(IReceiver device);
    public delegate void OnCastableDeviceAddedHandler(IReceiver device);
    public delegate void OnCastableDeviceDeletedHandler(string id);
    public delegate void OnFileLoadingHandler();
    public delegate void OnFileLoadedHandler(string title, string thumbPath, double duration, double volumeLevel, bool isMuted);
    public delegate void OnPositionChangedHandler(double newPosition);
    public delegate void OnEndReachedHandler();
    public delegate void OnTimeChangedHandler(double seconds);
    public delegate void OnQualitiesChangedHandler(int selectedQuality, List<int> qualities);
    public delegate void OnPausedHandler();
    public delegate void OnDisconnectedHandler();
    public delegate void OnVolumeChangedHandler(double newLevel, bool isMuted);
    //public delegate void OnFileLoadFailed();
    public delegate void OnServerMessageHandler(AppMessageType msg);

    public delegate void OnPlayListBusyHandler(long playListId, bool busy);

    public delegate void OnFileLoadingOrLoadedHandler(FileItemResponseDto file);

    public delegate void OnStoppedPlayback();
}