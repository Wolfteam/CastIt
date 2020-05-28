using System;

namespace CastIt.GoogleCast.Interfaces.Channels
{
    internal interface IStatusChannel<TStatus> : IStatusChannel
    {
        event EventHandler StatusChanged;

        new TStatus Status { get; }
    }
}
