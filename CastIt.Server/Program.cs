using CastIt.Application;
using CastIt.Application.Server;
using CastIt.Domain.Models.Logging;
using CastIt.Infrastructure;
using CastIt.Infrastructure.Interfaces;
using CastIt.Server.Interfaces;
using CastIt.Shared.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CastIt.Server
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            var logs = new List<FileToLog>
            {
                new FileToLog(typeof(MainService), "service_main")
            };

            logs.AddRange(DependencyInjection.GetServerLogs());
            logs.AddRange(Infrastructure.DependencyInjection.GetInfrastructureLogs());
            logs.AddRange(Application.DependencyInjection.GetApplicationLogs());

            logs.SetupLogging();
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureServices((h, s) => ConfigureServices(args, h, s));

        private static void ConfigureServices(string[] args, HostBuilderContext hostContext, IServiceCollection services)
        {
            if (args == null || args.Length == 0)
                throw new Exception("To start the web server you need to provide some args");

            services.AddLogging(b =>
            {
                b.AddDebug();
                b.AddConsole();
                b.AddSerilog();
                b.AddEventLog();
            });

            string ffmpegPath = null;
            string ffprobePath = null;
            int startingPort = AppWebServerConstants.DefaultPort;

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

            services.AddApplication(ffmpegPath, ffprobePath)
                .AddInfrastructure()
                .AddAppWebServer();

            services.AddHostedService(provider =>
            {
                var logger = provider.GetRequiredService<ILogger<MainService>>();
                var webServer = provider.GetRequiredService<IAppWebServer>();
                var castService = provider.GetRequiredService<ICastService>();
                var appSettingsService = provider.GetRequiredService<IAppSettingsService>();
                return new MainService(logger, webServer, castService, appSettingsService, startingPort);
            });
        }
    }
}
