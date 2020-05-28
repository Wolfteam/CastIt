namespace CastIt.GoogleCast.Interfaces.Messages
{
    internal interface IStatusMessage<TStatus> : IMessageWithId
    {
        TStatus Status { get; }
    }
}
