using CastIt.GoogleCast.Messages.Base;
using CastIt.GoogleCast.Models;

namespace CastIt.GoogleCast.Messages.Receiver
{
    internal class SetVolumeMessage : MessageWithId
    {
        public Volume Volume { get; set; }
    }
}
