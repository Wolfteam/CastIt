using CastIt.GoogleCast.Shared.Device;
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
        private const string Protocol = "_googlecast._tcp.local.";

        private static IReceiver CreateReceiver(IZeroconfHost host)
        {
            string key = host.Services.Keys.FirstOrDefault(k => k == Protocol || k.EndsWith(Protocol));
            var service = host.Services[key];
            var properties = service.Properties.First();
            return new Receiver()
            {
                Id = properties.ContainsKey("id") ? properties["id"] : string.Empty,
                FriendlyName = properties.ContainsKey("fn") ? properties["fn"] : string.Empty,
                Type = properties.ContainsKey("md") ? properties["md"] : string.Empty,
                Host = host.IPAddress,
                Port = service.Port
            };
        }

        public static async Task<List<IReceiver>> FindReceiversAsync(TimeSpan scanTime)
        {
            var devices = await ZeroconfResolver
                .ResolveAsync(Protocol, scanTime, 1, 100)
                .ConfigureAwait(false);
            return devices.Select(CreateReceiver).ToList();
        }

        public static IObservable<IReceiver> FindReceiversContinuous()
        {
            return ZeroconfResolver.ResolveContinuous(Protocol).Select(CreateReceiver);
        }
    }
}
