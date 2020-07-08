using CastIt.GoogleCast.Interfaces;
using System.Collections.Generic;

namespace CastIt.Common
{
    public delegate void OnCastRendererSetHandler(string id);
    public delegate void OnCastableDeviceAddedHandler(IReceiver receiver);
    public delegate void OnCastableDeviceDeletedHandler(IReceiver receiver);
    public delegate void OnFileLoading();
    public delegate void OnFileLoaded(string title, string thumbPath, double duration, double volumeLevel, bool isMuted);
    public delegate void OnFileLoadedWs();
    public delegate void OnFileLoadingError(string error);
    public delegate void OnPositionChangedHandler(double newPosition);
    public delegate void OnEndReachedHandler();
    public delegate void OnTimeChangedHandler(double seconds);
    public delegate void OnQualitiesChanged(int selectedQuality, List<int> qualities);
    public delegate void OnPaused();
    public delegate void OnDisconnected();
    public delegate void OnVolumeChanged(double newLevel, bool isMuted);
    public delegate void OnAppClosing();
    public delegate void OnAppSettingsChanged();
    public delegate void OnPlayListsChanged();
    public delegate void OnFilesChanged();
    public delegate void OnServerMsg(string msg);
}
