using CastIt.GoogleCast.Messages.Base;

namespace CastIt.GoogleCast.Messages.Media
{
    internal class InvalidRequestMessage : MessageWithId
    {
        public string Reason { get; set; }
    }
}
