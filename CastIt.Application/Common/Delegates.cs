using System.Collections.Generic;

namespace CastIt.Application.Common
{
    public delegate void OnCastRendererSetHandler(string id);
    public delegate void OnCastableDeviceAddedHandler(string id, string friendlyName, string type, string host, int port);
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
    public delegate void OnFileLoadFailed();
}