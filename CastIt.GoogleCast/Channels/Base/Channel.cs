using CastIt.GoogleCast.Interfaces;
using CastIt.GoogleCast.Interfaces.Channels;
using CastIt.GoogleCast.Interfaces.Messages;
using CastIt.GoogleCast.Models;
using System.Threading.Tasks;

namespace CastIt.GoogleCast.Channels
{
    internal abstract class Channel : IChannel
    {
        private const string BASE_NAMESPACE = "urn:x-cast:com.google.cast";
        protected readonly string DestinationId;

        public string Namespace { get; protected set; }

        protected Channel(string ns, string destinationId)
        {
            Namespace = $"{BASE_NAMESPACE}.{ns}";
            DestinationId = destinationId;
        }

        protected AppMessage BuildCommonAppMsg(IMessage message)
        {
            return new AppMessage
            {
                NameSpace = Namespace,
                DestinationId = DestinationId,
                Message = message
            };
        }

        public virtual Task<AppMessage> OnMessageReceivedAsync(ISender sender, IMessage message)
        {
            return Task.FromResult(BuildCommonAppMsg(null));
        }
    }
}
