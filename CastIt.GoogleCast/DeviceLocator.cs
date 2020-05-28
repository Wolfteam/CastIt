using CastIt.GoogleCast.Interfaces;
using CastIt.GoogleCast.Models.Receiver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Zeroconf;

namespace CastIt.GoogleCast
{
    internal static class DeviceLocator
    {
        private const string PROTOCOL = "_googlecast._tcp.local.";

        private static IReceiver CreateReceiver(IZeroconfHost host)
        {
            var service = host.Services[PROTOCOL];
            var properties = service.Properties.First();
            return new Receiver()
            {
                Id = properties["id"],
                FriendlyName = properties["fn"],
                Host = host.IPAddress,
                Port = service.Port
            };
        }

        public static async Task<List<IReceiver>> FindReceiversAsync()
        {
            var devices = await ZeroconfResolver.ResolveAsync(PROTOCOL, retries: 1, retryDelayMilliseconds: 100);
            return devices.Select(CreateReceiver).ToList();
        }

        public static IObservable<IReceiver> FindReceiversContinuous()
        {
            return ZeroconfResolver.ResolveContinuous(PROTOCOL).Select(CreateReceiver);
        }
    }
}
