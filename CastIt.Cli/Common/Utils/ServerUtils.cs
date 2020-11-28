using System.IO;
using CastIt.Application.Server;
using McMaster.Extensions.CommandLineUtils;

namespace CastIt.Cli.Common.Utils
{
    public static class ServerUtils
    {
        public static string StartServerIfNotStarted(IConsole console, string ffmpegPath = null, string ffprobePath = null)
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

            var dir = Directory.GetCurrentDirectory();
            var path = Path.Combine(dir, "Server", WebServerUtils.ServerProcessName);
            console.WriteLine($"Trying to start process located in = {path}");
//#if DEBUG
//            path = "E:\\Proyectos\\CastIt\\CastIt.Server\\bin\\Debug\\net5.0\\CastIt.Server.exe";
//#endif

            console.WriteLine("Web server is not running, starting it...");
            var openPort = WebServerUtils.GetOpenPort();
            var args = new[]
            {
                AppWebServerConstants.PortArgument, $"{openPort}",
                AppWebServerConstants.FFmpegPathArgument, ffmpegPath,
                AppWebServerConstants.FFprobePathArgument, ffprobePath
            };
            var escapedArgs = ArgumentEscaper.EscapeAndConcatenate(args);
            bool started = WebServerUtils.StartServer(escapedArgs, path);
            if (started)
                return WebServerUtils.GetWebServerIpAddress(openPort);

            console.WriteLine("Couldn't start the web server");
            return null;
        }
    }
}
