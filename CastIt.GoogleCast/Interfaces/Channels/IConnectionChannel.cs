using System.Threading.Tasks;

namespace CastIt.GoogleCast.Interfaces.Channels
{
    internal interface IConnectionChannel : IChannel
    {
        Task ConnectAsync(ISender sender, string destinationId);
    }
}