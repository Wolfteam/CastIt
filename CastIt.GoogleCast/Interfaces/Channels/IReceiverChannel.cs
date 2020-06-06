using CastIt.GoogleCast.Models.Receiver;
using System.Threading.Tasks;

namespace CastIt.GoogleCast.Interfaces.Channels
{
    internal interface IReceiverChannel : IStatusChannel<ReceiverStatus>
    {
        bool IsConnected { get; set; }

        Task<ReceiverStatus> LaunchAsync(ISender sender, string applicationId);

        Task<ReceiverStatus> GetStatusAsync(ISender sender);

        Task<Application> GetApplication(ISender sender, IConnectionChannel connectionChannel, string ns);

        Task<ReceiverStatus> SetVolumeAsync(ISender sender, float level);

        Task<ReceiverStatus> SetIsMutedAsync(ISender sender, bool isMuted);

        Task StopAsync(ISender sender, params Application[] applications);
    }
}