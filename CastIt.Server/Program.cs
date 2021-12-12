using CastIt.Domain;
using CastIt.Domain.Models.Logging;
using CastIt.Domain.Utils;
using CastIt.FFmpeg;
using CastIt.GoogleCast.LocalFile;
using CastIt.GoogleCast.Youtube;
using CastIt.Server.Common.Extensions;
using CastIt.Server.Services;
using CastIt.Shared.Extensions;
using CastIt.Youtube;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CastIt.Server
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            if (args == null || args.Length == 0)
                args = new List<string>().ToArray();

            int startingPort = -1;

            var argsList = args.ToList();
            int portIndex = argsList.IndexOf(AppWebServerConstants.PortArgument);

            var possiblePort = argsList.ElementAtOrDefault(portIndex + 1);
            if (portIndex >= 0 && int.TryParse(possiblePort, out int port))
            {
                startingPort = port;
            }

            var logs = new List<FileToLog>();
            logs.AddServerLogs()
                .AddFFmpegLogs()
                .AddYoutubeParserLogs()
                .AddGoogleCastLocalFilesLogs()
                .AddGoogleCastYoutubeLogs();

            logs.SetupLogging(AppFileUtils.GetServerLogsPath());

#if !DEBUG
            if (startingPort <= 0)
            {
                startingPort = WebServerUtils.GetOpenPort();
            }
#endif
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            CreateHostBuilder(args, startingPort).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args, int port) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    if (port > 0)
                    {
                        webBuilder.UseKestrel(options => options.ListenAnyIP(port));
                    }

                    webBuilder.UseStartup(factory => new Startup(factory.Configuration));
                })
                //This has to happen AFTER ConfigureWebHostDefaults in order to get the server ip address
                //https://stackoverflow.com/questions/58457143/net-core-3-0-ihostedservice-access-web-server-url-scheme-host-port-etc
                .ConfigureServices(services => services.AddHostedService<CastItHostedService>())
                .UseWindowsService();
    }
}
