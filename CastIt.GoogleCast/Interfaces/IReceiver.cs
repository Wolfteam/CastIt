namespace CastIt.GoogleCast.Interfaces
{
    public interface IReceiver
    {
        string Id { get; }

        string FriendlyName { get; }

        string Host { get; }

        int Port { get; }
    }
}
