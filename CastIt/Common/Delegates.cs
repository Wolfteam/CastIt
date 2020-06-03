using CastIt.GoogleCast.Interfaces;
using System.Collections.Generic;

namespace CastIt.Common
{
    public delegate void OnCastRendererSetHandler(string id);
    public delegate void OnCastableDeviceAddedHandler(IReceiver receiver);
    public delegate void OnCastableDeviceDeletedHandler(IReceiver receiver);
    public delegate void OnPositionChangedHandler(double newPosition);
    public delegate void OnEndReachedHandler();
    public delegate void OnTimeChangedHandler(double seconds);
    public delegate void OnQualitiesChanged(int selectedQuality, List<int> qualities);
}
