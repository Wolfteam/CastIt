using CastIt.Domain.Interfaces;

namespace CastIt.Domain.Models.Device
{
    public class Receiver : IReceiver
    {
        public string Id { get; set; }

        public string FriendlyName { get; set; }

        public string Type { get; set; }

        public string Host { get; set; }

        public int Port { get; set; }

        public static Receiver Default(string host, int port)
        {
            return new Receiver
            {
                Host = host,
                Port = port,
                FriendlyName = "N/A",
                Type = "N/A"
            };
        }
    }
}
