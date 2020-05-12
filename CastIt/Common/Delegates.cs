using CastIt.Models;

namespace CastIt.Common
{
    public delegate void OnCastRendererSetHandler(string name, string type);
    public delegate void OnCastableDeviceAddedHandler(CastableDevice newDevice);
    public delegate void OnCastableDeviceDeletedHandler(CastableDevice deletedDevice);
    public delegate void OnPositionChangedHandler(float newPosition);
    public delegate void OnEndReachedHandler();
    public delegate void OnTimeChangedHandler(long seconds);
}
