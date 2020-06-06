using System;
using System.Net.NetworkInformation;

namespace CastIt.Common.Utils
{
    public static class NetworkUtils
    {
        public static bool IsInternetAvailable()
        {
            const int timeout = 1000;
            const string host = "google.com";

            var ping = new Ping();
            var buffer = new byte[32];
            var pingOptions = new PingOptions();

            try
            {
                var reply = ping.Send(host, timeout, buffer, pingOptions);
                return reply?.Status == IPStatus.Success;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
