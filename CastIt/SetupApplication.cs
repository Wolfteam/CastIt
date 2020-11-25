using AutoMapper;
using CastIt.Application.FFMpeg;
using CastIt.Application.FilePaths;
using CastIt.Application.Interfaces;
using CastIt.Application.Telemetry;
using CastIt.Application.Youtube;
using CastIt.Common;
using CastIt.Common.Utils;
using CastIt.Domain.Models.Logging;
using CastIt.GoogleCast;
using CastIt.GoogleCast.Interfaces;
using CastIt.Infrastructure.Interfaces;
using CastIt.Infrastructure.Services;
using CastIt.Interfaces;
using CastIt.Resources;
using CastIt.Server;
using CastIt.Server.Interfaces;
using CastIt.Services;
using CastIt.Shared.Extensions;
using CastIt.ViewModels;
using CastIt.ViewModels.Dialogs;
using CastIt.ViewModels.Items;
using Microsoft.Extensions.Logging;
using MvvmCross;
using MvvmCross.IoC;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using Serilog;
using System.Collections.Generic;

namespace CastIt
{
    public class SetupApplication : MvxApplication
    {
        public override void Initialize()
        {
            base.Initialize();

            CreatableTypes()
                .EndingWith("Service")
                .AsInterfaces()
                .RegisterAsLazySingleton();

            SetupLogging();
            SetupMapper();

            //NPI WHY I NEED TO MANUALLY REGISTER THIS ONE
            Mvx.IoCProvider.RegisterSingleton<IMvxMessenger>(new MvxMessengerHub());

            string baseAppFolder = FileUtils.GetBaseAppFolder();
            var fileService = new FileService(baseAppFolder);

            Mvx.IoCProvider.RegisterSingleton(typeof(IFileService), () => fileService);
            Mvx.IoCProvider.RegisterSingleton(typeof(ICommonFileService), () => fileService);
            Mvx.IoCProvider.ConstructAndRegisterSingleton<ITelemetryService, TelemetryService>();
            Mvx.IoCProvider.ConstructAndRegisterSingleton<IAppSettingsService, AppSettingsService>();
            Mvx.IoCProvider.ConstructAndRegisterSingleton<IAppDataService, AppDataService>();
            Mvx.IoCProvider.ConstructAndRegisterSingleton<IYoutubeUrlDecoder, YoutubeUrlDecoder>();

            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<IPlayer>(() =>
            {
                var logger = Mvx.IoCProvider.Resolve<ILogger<Player>>();
                return new Player(logger, logToConsole: false);
            });

            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<IFFmpegService, FFmpegService>();
            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<ICastService, CastService>();
            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<IFileWatcherService, FileWatcherService>();

            Mvx.IoCProvider.ConstructAndRegisterSingleton<IAppWebServer, AppWebServer>();
            Mvx.IoCProvider.RegisterSingleton(typeof(IBaseWebServer), () => Mvx.IoCProvider.Resolve<IAppWebServer>());

            var messenger = Mvx.IoCProvider.Resolve<IMvxMessenger>();
            var textProvider = new ResxTextProvider(Resource.ResourceManager, messenger);
            Mvx.IoCProvider.RegisterSingleton<ITextProvider>(textProvider);

            //since im using automapper to resolve this one, i need to explicit register it
            Mvx.IoCProvider.RegisterType<PlayListItemViewModel>();
            Mvx.IoCProvider.RegisterType<FileItemViewModel>();
            Mvx.IoCProvider.RegisterType<DeviceItemViewModel>();
            Mvx.IoCProvider.ConstructAndRegisterSingleton(typeof(SettingsViewModel));

            RegisterAppStart<SplashViewModel>();
        }

        private static void SetupMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                // Add all profiles in current assembly
                cfg.AddProfile<MappingProfile>();
                cfg.ConstructServicesUsing(Mvx.IoCProvider.Resolve);
            });
            //config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();
            Mvx.IoCProvider.RegisterSingleton(mapper);
        }

        private static void SetupLogging()
        {
            var basePath = FileUtils.GetLogsPath();
            var logs = new List<FileToLog>
            {
                //ViewModels
                new FileToLog(typeof(MainViewModel), "vm_main"),
                new FileToLog(typeof(DevicesViewModel), "vm_devices"),
                new FileToLog(typeof(SettingsViewModel), "vm_settings"),
                new FileToLog(typeof(PlayListItemViewModel), "vm_playlistitem"),
                new FileToLog(typeof(FileItemViewModel), "vm_fileitem"),
                new FileToLog(typeof(DeviceItemViewModel), "vm_deviceitem"),
                new FileToLog(typeof(DownloadDialogViewModel), "vm_download_dialog"),
                new FileToLog(typeof(SplashViewModel), "vm_splash"),
                //Others
                new FileToLog(typeof(Player), "cast_player"),
            };

            logs.AddRange(Application.DependencyInjection.GetApplicationLogs());
            logs.AddRange(Infrastructure.DependencyInjection.GetInfrastructureLogs());
            logs.AddRange(Server.DependencyInjection.GetServerLogs());

            logs.SetupLogging(basePath);
            var loggerFactory = new LoggerFactory()
                .AddSerilog();
            //to resolve an ilogger<T> you need a iloggerfactory, thats why whe register both
            Mvx.IoCProvider.RegisterSingleton(loggerFactory);
            Mvx.IoCProvider.RegisterType(typeof(ILogger<>), typeof(Logger<>));
        }
    }
}
