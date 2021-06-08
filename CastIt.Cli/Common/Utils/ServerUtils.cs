using CastIt.Application.Server;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.IO;

namespace CastIt.Cli.Common.Utils
{
    public static class ServerUtils
    {
        public static string StartServerIfNotStarted(string ffmpegPath = null, string ffprobePath = null)
        {
            if (WebServerUtils.IsServerAlive())
            {
                Console.WriteLine("Server is alive, getting its port...");
                var port = WebServerUtils.GetServerPort();
                if (port.HasValue)
                    return WebServerUtils.GetWebServerIpAddress(port.Value);
                Console.WriteLine("Couldn't retrieve the server port");
                return null;
            }
            Console.WriteLine("Web server is not running, starting it...");

            var dir = Directory.GetCurrentDirectory();
            var path = Path.Combine(dir, "Server", WebServerUtils.ServerProcessName);
#if DEBUG
            path = "D:\\Proyectos\\CastIt\\CastIt.Test\\bin\\Debug\\net5.0\\CastIt.Test.exe";
#endif
            Console.WriteLine("Getting an open port...");
            var openPort = WebServerUtils.GetOpenPort();
            var args = new List<string>
            {
                AppWebServerConstants.PortArgument, $"{openPort}"
            };

            if (!string.IsNullOrWhiteSpace(ffmpegPath))
            {
                args.Add(AppWebServerConstants.FFmpegPathArgument);
                args.Add(ffmpegPath);
            }

            if (!string.IsNullOrWhiteSpace(ffprobePath))
            {
                args.Add(AppWebServerConstants.FFprobePathArgument);
                args.Add(ffprobePath);
            }

            var escapedArgs = ArgumentEscaper.EscapeAndConcatenate(args);
            Console.WriteLine($"Trying to start process located in = {path} with args = {escapedArgs}");
            bool started = WebServerUtils.StartServer(escapedArgs, path);
            if (started)
                return WebServerUtils.GetWebServerIpAddress(openPort);

            Console.WriteLine("Couldn't start the web server");
            return null;
        }
    }
}
