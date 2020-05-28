namespace CastIt.GoogleCast.Messages.Base
{
    internal abstract class MediaSessionMessage : MessageWithId
    {
        public long? MediaSessionId { get; set; }
    }
}
