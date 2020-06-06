using CastIt.GoogleCast.Interfaces;
using CastIt.GoogleCast.Interfaces.Messages;

namespace CastIt.GoogleCast.Messages.Base
{
    internal abstract class StatusMessage<TStatus> : MessageWithId, IStatusMessage<TStatus>
    {
        public TStatus Status { get; set; }
    }
}
