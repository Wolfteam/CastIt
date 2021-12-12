using CastIt.Cli.Common.Exceptions;
using CastIt.Shared.Server;
using System;

namespace CastIt.Cli.Common.Utils
{
    public static class ServerUtils
    {
        public static string StartServerIfNotStarted()
        {
            if (WebServerUtils.IsServerAlive())
            {
#if DEBUG
                Console.WriteLine("Server is alive, getting its port...");
#endif
                var port = WebServerUtils.GetServerPort();
                if (port.HasValue)
                    return WebServerUtils.GetWebServerIpAddress(port.Value);
                Console.WriteLine("Couldn't retrieve the server port");
                return null;
            }

            throw new ServerNotRunningException();
        }
    }
}
