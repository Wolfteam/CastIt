using CastIt.Application.Server;
using CastIt.Server.Common;
using McMaster.Extensions.CommandLineUtils;

namespace CastIt.GoogleCast.Cli.Common.Utils
{
    public static class ServerUtils
    {
        public static string StartServerIfNotStarted(IConsole console)
        {
            if (WebServerUtils.IsServerAlive())
            {
                console.WriteLine("Server is alive, getting its port...");
                var port = WebServerUtils.GetServerPort();
                if (port.HasValue)
                    return WebServerUtils.GetWebServerIpAddress(port.Value);
                console.WriteLine("Couldn't retrieve the server port");
                return null;
            }

            console.WriteLine("Trying to start web server...");
            var openPort = WebServerUtils.GetOpenPort();
            var args = new[] { AppWebServerConstants.PortArgument, $"{openPort}" };
            var escapedArgs = ArgumentEscaper.EscapeAndConcatenate(args);
            console.WriteLine("Web server is not running, starting it...");
            bool started = WebServerUtils.StartServer(escapedArgs);
            if (started)
                return WebServerUtils.GetWebServerIpAddress(openPort);

            console.WriteLine("Couldn't start the web server");
            return null;
        }
    }
}
