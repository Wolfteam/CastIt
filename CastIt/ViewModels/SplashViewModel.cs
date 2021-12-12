using CastIt.Common.Utils;
using CastIt.Interfaces;
using CastIt.Shared.FilePaths;
using CastIt.Shared.Telemetry;
using Microsoft.Extensions.Logging;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using System;
using System.Threading.Tasks;
using System.Timers;

namespace CastIt.ViewModels
{
    public class SplashViewModel : BaseViewModel
    {
        private readonly IMvxNavigationService _navigationService;
        private readonly ITelemetryService _telemetryService;
        private readonly IDesktopAppSettingsService _settingsService;
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
            IDesktopAppSettingsService settingsService,
            IFileService fileService) : base(textProvider, messenger, logger)
        {
            _navigationService = navigationService;
            _telemetryService = telemetryService;
            _settingsService = settingsService;
            _fileService = fileService;

            _timer = new Timer(800)
            {
                AutoReset = false
            };
            _timer.Elapsed += TimerElapsed;
        }

        public override async Task Initialize()
        {
            _telemetryService.Init();
            await _settingsService.Init();

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
            await base.Initialize();
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
            Logger.LogInformation($"{nameof(Initialize)}: Navigating to main view model...");
            await _navigationService.Navigate<MainViewModel>().ConfigureAwait(false);
            _beforeNavigatingToMainViewModel.Raise();
        }
    }
}
