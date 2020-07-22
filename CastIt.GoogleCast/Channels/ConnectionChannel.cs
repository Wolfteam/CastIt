using CastIt.GoogleCast.Interfaces;
using CastIt.GoogleCast.Interfaces.Channels;
using CastIt.GoogleCast.Interfaces.Messages;
using CastIt.GoogleCast.Messages.Connection;
using CastIt.GoogleCast.Models;
using System.Threading.Tasks;

namespace CastIt.GoogleCast.Channels
{
    internal class ConnectionChannel : Channel, IConnectionChannel
    {
        public ConnectionChannel(string destinationId) : base("tp.connection", destinationId)
        {
        }

        public Task ConnectAsync(ISender sender, string destinationId)
        {
            var msg = BuildCommonAppMsg(new ConnectMessage());
            msg.DestinationId = destinationId;
            return sender.SendAsync(msg);
        }

        public override Task<AppMessage> OnMessageReceivedAsync(ISender sender, IMessage message)
        {
            if (message is CloseMessage)
            {
                sender.Disconnect(true, true);
            }
            return base.OnMessageReceivedAsync(sender, message);
        }
    }
}
