using CastIt.Application;
using CastIt.Application.Server;
using CastIt.Domain.Models.Logging;
using CastIt.Infrastructure;
using CastIt.Infrastructure.Interfaces;
using CastIt.Server.Interfaces;
using CastIt.Server.Models;
using CastIt.Shared.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
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
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureAppConfiguration(ConfigureApp)
                .ConfigureServices((h, s) => ConfigureServices(args, h, s));

        private static void ConfigureApp(HostBuilderContext hostContext, IConfigurationBuilder builder)
        {
            //string environment = hostContext.HostingEnvironment.EnvironmentName;
            builder.AddJsonFile($"appsettings.json");
        }

        private static void ConfigureServices(string[] args, HostBuilderContext hostContext, IServiceCollection services)
        {
            services.AddLogging(b =>
            {
                b.AddDebug();
                b.AddConsole();
                b.AddSerilog();
                b.AddEventLog();
            });

            var config = hostContext.Configuration;
            var appSettings = config.GetSection(nameof(AppSettings)).Get<AppSettings>();

            int startingPort = AppWebServerConstants.DefaultPort;
            if (args != null && args.Length > 0)
            {
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
                    appSettings.FFmpegPath = ffmpegBasePath;
                }

                if (ffprobeBasePathIndex >= 0 && !string.IsNullOrWhiteSpace(ffprobeBasePath) && File.Exists(ffprobeBasePath))
                {
                    appSettings.FFprobePath = ffprobeBasePath;
                }
            }

            services.AddApplication(appSettings.FFmpegPath, appSettings.FFprobePath)
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
