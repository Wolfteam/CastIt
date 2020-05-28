namespace CastIt.GoogleCast.Interfaces.Messages
{
    public interface IMessageWithId : IMessage
    {
        int RequestId { get; }
    }
}
