using CastIt.Application.Interfaces;
using CastIt.Common.Utils;
using CastIt.Interfaces;
using CastIt.ViewModels.Items;
using MvvmCross.Logging;
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
            IMvxLogProvider logProvider,
            IMvxNavigationService navigationService,
            ITelemetryService telemetryService,
            IAppSettingsService settingsService,
            IAppDataService playListsService,
            IFileService fileService) : base(textProvider, messenger, logProvider.GetLogFor<SplashViewModel>())
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
            LoadingText = $"{GetText("Loading")}...";
            Logger.Info($"{nameof(Initialize)}: Applying app theme and accent color...");
            WindowsUtils.ChangeTheme(_settingsService.AppTheme, _settingsService.AccentColor);

            Logger.Info($"{nameof(Initialize)}: Deleting old preview / log files...");
            try
            {
                _fileService.DeleteFilesInDirectory(_fileService.GetPreviewsPath(), DateTime.Now.AddDays(-1));
                _fileService.DeleteFilesInDirectory(FileUtils.GetLogsPath(), DateTime.Now.AddDays(-3));
                Logger.Info($"{nameof(Initialize)}: Old files were deleted");
            }
            catch (Exception e)
            {
                Logger.Error(e, $"{nameof(Initialize)}: Error occurred while trying to delete previews");
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

            Logger.Info($"{nameof(Initialize)}: Getting all playlists...");
            var playLists = await _playListsService.GetAllPlayLists();
            foreach (var playlist in playLists)
            {
                var files = await _playListsService.GetAllFiles(playlist.Id);
                playlist.Items.AddRange(files.OrderBy(f => f.Position));
                playlist.SetPositionIfChanged();
            }

            Logger.Info($"{nameof(Initialize)}: Navigating to main view model...");
            await _navigationService.Navigate<MainViewModel, List<PlayListItemViewModel>>(playLists).ConfigureAwait(false);
            _beforeNavigatingToMainViewModel.Raise();
        }
    }
}
