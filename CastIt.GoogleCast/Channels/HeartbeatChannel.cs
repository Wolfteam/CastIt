using CastIt.GoogleCast.Interfaces;
using CastIt.GoogleCast.Interfaces.Channels;
using CastIt.GoogleCast.Interfaces.Messages;
using CastIt.GoogleCast.Messages.HeartBeat;
using CastIt.GoogleCast.Models;
using System.Threading.Tasks;

namespace CastIt.GoogleCast.Channels
{
    internal class HeartbeatChannel : Channel, IHeartbeatChannel
    {
        public HeartbeatChannel(string destinationId) : base("tp.heartbeat", destinationId)
        {
        }

        public override Task<AppMessage> OnMessageReceivedAsync(ISender sender, IMessage message)
        {
            return message switch
            {
                PingMessage _ => Task.FromResult(BuildCommonAppMsg(new PongMessage())),
                _ => base.OnMessageReceivedAsync(sender, message)
            };
        }
    }
}
