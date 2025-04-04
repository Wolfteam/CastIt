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
    }
}
