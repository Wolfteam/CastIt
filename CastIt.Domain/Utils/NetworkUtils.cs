using System;
using System.Net.NetworkInformation;

namespace CastIt.Domain.Utils
{
    public static class NetworkUtils
    {
        public static bool IsInternetAvailable()
        {
            const int timeout = 1000;
            const string host = "google.com";

            var ping = new Ping();

            try
            {
                var reply = ping.SendPingAsync(host, timeout).GetAwaiter().GetResult();
                return reply?.Status == IPStatus.Success;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}
