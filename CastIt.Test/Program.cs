using CastIt.Application.Common.Utils;
using CastIt.Application.Server;
using CastIt.Domain.Models.Logging;
using CastIt.Shared.Extensions;
using CastIt.Test.Controllers;
using CastIt.Test.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CastIt.Test
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            //TODO: READ FROM THE SETTINGS
            if (args == null || args.Length == 0)
                args = new List<string>().ToArray();

            string ffmpegPath = null;
            string ffprobePath = null;
            int startingPort = -1;

            var argsList = args.ToList();
            int portIndex = argsList.IndexOf(AppWebServerConstants.PortArgument);
            int ffmpegBasePathIndex = argsList.IndexOf(AppWebServerConstants.FFmpegPathArgument);
            int ffprobeBasePathIndex = argsList.IndexOf(AppWebServerConstants.FFprobePathArgument);

            var possiblePort = argsList.ElementAtOrDefault(portIndex + 1);
            var ffmpegBasePath = argsList.ElementAtOrDefault(ffmpegBasePathIndex + 1);
            var ffprobeBasePath = argsList.ElementAtOrDefault(ffprobeBasePathIndex + 1);

            if (portIndex >= 0 && int.TryParse(possiblePort, out int port))
            {
                startingPort = port;
            }

            if (ffmpegBasePathIndex >= 0 && !string.IsNullOrWhiteSpace(ffmpegBasePath) && File.Exists(ffmpegBasePath))
            {
                ffmpegPath = ffmpegBasePath;
            }

            if (ffprobeBasePathIndex >= 0 && !string.IsNullOrWhiteSpace(ffprobeBasePath) && File.Exists(ffprobeBasePath))
            {
                ffprobePath = ffprobeBasePath;
            }

            var logs = new List<FileToLog>
            {
                new FileToLog(typeof(PlayerController), "controller_castit"),
                new FileToLog(typeof(CastItHub), "hub_castit"),
                new FileToLog(typeof(AppDataService), "service_appdata")
            };

            //TODO: LOG THEM ALL
            //logs.AddRange(DependencyInjection.GetServerLogs());
            logs.AddRange(Infrastructure.DependencyInjection.GetInfrastructureLogs());
            logs.AddRange(Application.DependencyInjection.GetApplicationLogs());
            logs.SetupLogging(AppFileUtils.GetServerLogsPath());

            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            CreateHostBuilder(args, ffmpegPath, ffprobePath, startingPort).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args, string ffmpegPath, string ffprobePath, int port) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    if (port > 0)
                    {
                        webBuilder.UseKestrel(options => options.ListenAnyIP(port));
                    }

                    webBuilder.UseStartup(factory => new Startup(factory.Configuration, ffmpegPath, ffprobePath));
                })
                //This has to happen AFTER ConfigureWebHostDefaults in order to get the server ip address
                //https://stackoverflow.com/questions/58457143/net-core-3-0-ihostedservice-access-web-server-url-scheme-host-port-etc
                .ConfigureServices(services => services.AddHostedService<CastItHostedService>());
    }
}
