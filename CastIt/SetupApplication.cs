using AutoMapper;
using CastIt.Common;
using CastIt.Domain.Utils;
using CastIt.Interfaces;
using CastIt.Resources;
using CastIt.Services;
using CastIt.Shared.FilePaths;
using CastIt.Shared.Telemetry;
using CastIt.ViewModels;
using CastIt.ViewModels.Items;
using CastIt.Youtube;
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

            SetupLogging();
            SetupMapper();

            //NPI WHY I NEED TO MANUALLY REGISTER THIS ONE
            Mvx.IoCProvider.RegisterSingleton<IMvxMessenger>(new MvxMessengerHub());

            string baseAppFolder = AppFileUtils.GetBaseAppFolder();
            var fileService = new FileService(baseAppFolder);

            Mvx.IoCProvider.RegisterSingleton(typeof(IFileService), () => fileService);
            Mvx.IoCProvider.RegisterSingleton(typeof(ICommonFileService), () => fileService);
            Mvx.IoCProvider.ConstructAndRegisterSingleton<ITelemetryService, TelemetryService>();
            Mvx.IoCProvider.ConstructAndRegisterSingleton<IDesktopAppSettingsService, DesktopAppSettingsService>();
            Mvx.IoCProvider.ConstructAndRegisterSingleton<IYoutubeUrlDecoder, YoutubeUrlDecoder>();

            var messenger = Mvx.IoCProvider.Resolve<IMvxMessenger>();
            var textProvider = new ResxTextProvider(Resource.ResourceManager, messenger);
            Mvx.IoCProvider.RegisterSingleton<ITextProvider>(textProvider);

            //since im using automapper to resolve this one, i need to explicit register it
            Mvx.IoCProvider.RegisterType<PlayListItemViewModel>();
            Mvx.IoCProvider.RegisterType<FileItemViewModel>();
            Mvx.IoCProvider.RegisterType<DeviceItemViewModel>();
            Mvx.IoCProvider.ConstructAndRegisterSingleton(typeof(SettingsViewModel));
            Mvx.IoCProvider.ConstructAndRegisterSingleton(typeof(DevicesViewModel));
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
            //todo: this
            //var basePath = AppFileUtils.GetDesktopLogsPath();
            //var logs = new List<ToLog>
            //{
            //    //Services
            //    new ToLog(typeof(CastItHubClientService), "service_castithub"),
            //    new ToLog(typeof(DesktopAppSettingsService), "service_settings"),
            //    //ViewModels
            //    new ToLog(typeof(MainViewModel), "vm_main"),
            //    new ToLog(typeof(DevicesViewModel), "vm_devices"),
            //    new ToLog(typeof(SettingsViewModel), "vm_settings"),
            //    new ToLog(typeof(PlayListItemViewModel), "vm_playlistitem"),
            //    new ToLog(typeof(FileItemViewModel), "vm_fileitem"),
            //    new ToLog(typeof(DeviceItemViewModel), "vm_deviceitem"),
            //    new ToLog(typeof(DownloadDialogViewModel), "vm_download_dialog"),
            //    new ToLog(typeof(SplashViewModel), "vm_splash"),
            //};
            //logs.SetupLogging(basePath);
        }
    }
}
