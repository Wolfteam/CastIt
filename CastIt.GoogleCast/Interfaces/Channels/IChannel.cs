using CastIt.GoogleCast.Interfaces.Messages;
using CastIt.GoogleCast.Models;
using System.Threading.Tasks;

namespace CastIt.GoogleCast.Interfaces.Channels
{
    internal interface IChannel
    {
        string Namespace { get; }

        Task<AppMessage> OnMessageReceivedAsync(ISender sender, IMessage message);
    }
}