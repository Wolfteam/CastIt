using CastIt.GoogleCast.Interfaces;
using CastIt.GoogleCast.Interfaces.Channels;
using CastIt.GoogleCast.Messages;
using CastIt.GoogleCast.Messages.Receiver;
using CastIt.GoogleCast.Models;
using CastIt.GoogleCast.Models.Receiver;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CastIt.GoogleCast.Channels
{
    internal class ReceiverChannel : StatusChannel<ReceiverStatus, ReceiverStatusMessage>, IReceiverChannel
    {
        public ReceiverChannel(string destinationId) : base("receiver", destinationId)
        {
        }

        private bool IsConnected { get; set; }

        public async Task<ReceiverStatus> LaunchAsync(ISender sender, string applicationId)
        {
            var msg = new LaunchMessage { ApplicationId = applicationId };
            var response = await sender.SendAsync<ReceiverStatusMessage>(Namespace, msg, DestinationId);
            return response.Status;
        }

        public Task<ReceiverStatus> SetVolumeAsync(ISender sender, float level)
        {
            return SetVolumeAsync(sender, level, null);
        }

        public Task<ReceiverStatus> SetIsMutedAsync(ISender sender, bool isMuted)
        {
            return SetVolumeAsync(sender, null, isMuted);
        }

        private async Task<ReceiverStatus> SetVolumeAsync(ISender sender, float? level, bool? isMuted)
        {
            var msg = new SetVolumeMessage()
            {
                Volume = new Volume()
                {
                    Level = level,
                    IsMuted = isMuted,
                }
            };
            var response = await sender.SendAsync<ReceiverStatusMessage>(Namespace, msg, DestinationId);
            return response.Status;
        }

        private async Task<ReceiverStatus> CheckStatusAsync(ISender sender)
        {
            return Status ?? await GetStatusAsync(sender);
        }


        public async Task StopAsync(ISender sender, params Application[] applications)
        {
            IEnumerable<Application> apps = applications;
            if (apps?.Any() != true)
            {
                var status = await CheckStatusAsync(sender);
                apps = status.Applications;
                if (apps?.Any() != true)
                {
                    return;
                }
            }
            foreach (var application in apps)
            {
                var msg = new StopMessage() { SessionId = application.SessionId };
                await sender.SendAsync<ReceiverStatusMessage>(Namespace, msg, DestinationId);
            }
        }

        public async Task<ReceiverStatus> GetStatusAsync(ISender sender)
        {
            var response = await sender.SendAsync<ReceiverStatusMessage>(Namespace, new GetStatusMessage(), DestinationId);
            return response.Status;
        }

        public async Task<Application> GetApplication(ISender sender, IConnectionChannel connectionChannel, string ns)
        {
            var status = await CheckStatusAsync(sender);
            var application = status.Applications.First(a => a.Namespaces.Any(n => n.Name == ns));
            if (!IsConnected)
            {
                await connectionChannel.ConnectAsync(sender, application.SessionId);
                IsConnected = true;
            }
            return application;
        }

        //private void Disconnected(object sender, System.EventArgs e)
        //{
        //    IsConnected = false;
        //}
    }
}
