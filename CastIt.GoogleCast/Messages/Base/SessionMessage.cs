namespace CastIt.GoogleCast.Messages.Base
{
    public abstract class SessionMessage : MessageWithId
    {
        public string SessionId { get; set; }
    }
}
