using CastIt.Models;

namespace CastIt.Common
{
    public delegate void OnCastableDeviceAddedHandler(CastableDevice newDevice);
    public delegate void OnPositionChangedHandler(float newPosition);
    public delegate void OnEndReachedHandler();
    public delegate void OnTimeChangedHandler(long seconds);
}
