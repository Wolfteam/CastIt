﻿using AutoMapper;
using CastIt.Common;
using CastIt.Interfaces;
using CastIt.Models;
using CastIt.Resources;
using CastIt.Services;
using CastIt.ViewModels;
using CastIt.ViewModels.Items;
using MvvmCross;
using MvvmCross.IoC;
using MvvmCross.Logging;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;

namespace CastIt
{
    public class Application : MvxApplication
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

            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<ITextProvider>(() =>
            {
                var messenger = Mvx.IoCProvider.Resolve<IMvxMessenger>();
                var appSettings = Mvx.IoCProvider.Resolve<IAppSettingsService>();
                return new ResxTextProvider(Resource.ResourceManager, messenger, appSettings);
            });

            Mvx.IoCProvider.RegisterSingleton(new AppDbContext());

            Mvx.IoCProvider.ConstructAndRegisterSingleton<IAppSettingsService, AppSettingsService>();
            Mvx.IoCProvider.ConstructAndRegisterSingleton<ICastService, CastService>();
            Mvx.IoCProvider.ConstructAndRegisterSingleton<IPlayListsService, PlayListsService>();

            AppDbContext.Init(Mvx.IoCProvider.Resolve<IAppSettingsService>(), Mvx.IoCProvider.Resolve<IMvxLogProvider>().GetLogFor<Setup>());

            //since im using automapper to resolve this one, i need to explicit register it
            Mvx.IoCProvider.RegisterType<PlayListItemViewModel>();
            Mvx.IoCProvider.RegisterType<FileItemViewModel>();

            RegisterAppStart<MainViewModel>();
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
