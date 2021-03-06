﻿using CastIt.Application.Common.Utils;
using CastIt.Application.Interfaces;
using CastIt.Common;
using CastIt.Common.Utils;
using CastIt.Infrastructure.Interfaces;
using CastIt.Interfaces;
using CastIt.ViewModels.Items;
using Microsoft.Extensions.Logging;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace CastIt.ViewModels
{
    public class SplashViewModel : BaseViewModel
    {
        private readonly IMvxNavigationService _navigationService;
        private readonly ITelemetryService _telemetryService;
        private readonly IAppSettingsService _settingsService;
        private readonly IAppDataService _playListsService;
        private readonly Timer _timer;
        private readonly IFileService _fileService;

        private string _loadingText;

        private readonly MvxInteraction _beforeNavigatingToMainViewModel = new MvxInteraction();

        public string LoadingText
        {
            get => _loadingText;
            set => SetProperty(ref _loadingText, value);
        }

        public IMvxInteraction BeforeNavigatingToMainViewModel
            => _beforeNavigatingToMainViewModel;

        public SplashViewModel(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            ILogger<SplashViewModel> logger,
            IMvxNavigationService navigationService,
            ITelemetryService telemetryService,
            IAppSettingsService settingsService,
            IAppDataService playListsService,
            IFileService fileService) : base(textProvider, messenger, logger)
        {
            _navigationService = navigationService;
            _telemetryService = telemetryService;
            _settingsService = settingsService;
            _playListsService = playListsService;
            _fileService = fileService;

            _timer = new Timer(800)
            {
                AutoReset = false
            };
            _timer.Elapsed += TimerElapsed;
        }

        public override Task Initialize()
        {
            _telemetryService.Init();
            _settingsService.Init(AppFileUtils.GetBaseAppFolder(), AppConstants.AccentColorVividRed, AppConstants.MinWindowWidth, AppConstants.MinWindowHeight);
            TextProvider.SetLanguage(_settingsService.Language);

            LoadingText = $"{GetText("Loading")}...";

            Logger.LogInformation($"{nameof(Initialize)}: Applying app theme and accent color...");
            WindowsUtils.ChangeTheme(_settingsService.AppTheme, _settingsService.AccentColor);

            Logger.LogInformation($"{nameof(Initialize)}: Deleting old preview / log files...");
            try
            {
                _fileService.DeleteAppLogsAndPreviews();
                Logger.LogInformation($"{nameof(Initialize)}: Old files were deleted");
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"{nameof(Initialize)}: Error occurred while trying to delete previews");
                _telemetryService.TrackError(e);
            }
            return base.Initialize();
        }

        public override void ViewAppeared()
        {
            base.ViewAppeared();
            _timer.Start();
        }

        public (double, double) GetWindowWidthAndHeight()
        {
            return (_settingsService.WindowWidth, _settingsService.WindowHeight);
        }

        private async void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            _timer.Stop();
            _timer.Elapsed -= TimerElapsed;
            _timer.Dispose();

            Logger.LogInformation($"{nameof(Initialize)}: Getting all playlists...");
            var playLists = await _playListsService.GetAllPlayLists();
            foreach (var playlist in playLists)
            {
                var files = await _playListsService.GetAllFiles(playlist.Id);
                playlist.Items.AddRange(files.OrderBy(f => f.Position));
                playlist.SetPositionIfChanged();
            }

            Logger.LogInformation($"{nameof(Initialize)}: Navigating to main view model...");
            await _navigationService.Navigate<MainViewModel, List<PlayListItemViewModel>>(playLists).ConfigureAwait(false);
            _beforeNavigatingToMainViewModel.Raise();
        }
    }
}
