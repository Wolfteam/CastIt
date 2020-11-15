using CastIt.GoogleCast.Interfaces;
using System.Collections.Generic;
//TODO: MAYBE MOVE THIS OUT OF HERE
namespace CastIt.Server.Common
{
    public delegate void OnCastRendererSetHandler(string id);
    public delegate void OnCastableDeviceAddedHandler(IReceiver receiver);
    public delegate void OnCastableDeviceDeletedHandler(IReceiver receiver);
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
