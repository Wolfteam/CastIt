using CastIt.GoogleCast.Interfaces.Channels;

namespace CastIt.GoogleCast.Interfaces.Channels
{
    internal interface IStatusChannel : IChannel
    {
        object Status { get; set; }
    }
}
