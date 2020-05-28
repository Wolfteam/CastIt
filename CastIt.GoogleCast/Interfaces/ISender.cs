using CastIt.GoogleCast.Interfaces.Messages;
using CastIt.GoogleCast.Models;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace CastIt.GoogleCast.Interfaces
{
    internal interface ISender
    {
        ConcurrentDictionary<int, object> WaitingTasks { get; }
        bool IsConnected { get; }

        Task ConnectAsync();

        Task ConnectAsync(IReceiver receiver);

        void Disconnect();

        Task SendAsync(AppMessage msg);

        Task SendAsync(string ns, IMessage message, string destinationId);

        Task<TResponse> SendAsync<TResponse>(string ns, IMessageWithId message, string destinationId)
            where TResponse : IMessageWithId;
    }
}
