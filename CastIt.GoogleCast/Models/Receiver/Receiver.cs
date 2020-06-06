using CastIt.GoogleCast.Interfaces;

namespace CastIt.GoogleCast.Models.Receiver
{
    internal class Receiver : IReceiver
    {
        public string Id { get; set; }

        public string FriendlyName { get; set; }

        public string Type { get; set; }

        public string Host { get; set; }

        public int Port { get; set; }
    }
}
