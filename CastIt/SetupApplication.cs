using AutoMapper;
using CastIt.Application.FFMpeg;
using CastIt.Application.FilePaths;
using CastIt.Application.Interfaces;
using CastIt.Application.Telemetry;
using CastIt.Application.Youtube;
using CastIt.Common;
using CastIt.Common.Utils;
using CastIt.Interfaces;
using CastIt.Resources;
using CastIt.Server;
using CastIt.Server.Interfaces;
using CastIt.Services;
using CastIt.ViewModels;
using CastIt.ViewModels.Items;
using MvvmCross;
using MvvmCross.IoC;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;

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

            Mvx.IoCProvider.RegisterSingleton(CreateMapper);

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

            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<IAppWebServer, AppWebServer>();
            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<IFFmpegService, FFmpegService>();
            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<ICastService, CastService>();
            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<IFileWatcherService, FileWatcherService>();

            var messenger = Mvx.IoCProvider.Resolve<IMvxMessenger>();
            var appSettings = Mvx.IoCProvider.Resolve<IAppSettingsService>();
            var textProvider = new ResxTextProvider(Resource.ResourceManager, messenger, appSettings);
            Mvx.IoCProvider.RegisterSingleton<ITextProvider>(textProvider);

            //since im using automapper to resolve this one, i need to explicit register it
            Mvx.IoCProvider.RegisterType<PlayListItemViewModel>();
            Mvx.IoCProvider.RegisterType<FileItemViewModel>();
            Mvx.IoCProvider.RegisterType<DeviceItemViewModel>();
            Mvx.IoCProvider.ConstructAndRegisterSingleton(typeof(SettingsViewModel));

            Mvx.IoCProvider.Resolve<ITelemetryService>().Init();

            RegisterAppStart<SplashViewModel>();
        }

        private IMapper CreateMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                // Add all profiles in current assembly
                cfg.AddProfile<MappingProfile>();
                cfg.ConstructServicesUsing(Mvx.IoCProvider.Resolve);
            });
            //config.AssertConfigurationIsValid();
            return config.CreateMapper();
        }
    }
}
