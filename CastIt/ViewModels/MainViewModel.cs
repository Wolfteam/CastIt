using CastIt.Interfaces;
using CastIt.Models.Messages;
using CastIt.ViewModels.Items;
using MvvmCross.Commands;
using MvvmCross.Logging;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CastIt.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        #region Members
        private readonly ICastService _castService;
        private readonly IPlayListsService _playListsService;
        private readonly IAppSettingsService _settingsService;
        private FileItemViewModel _currentlyPlayedFile;
        private bool _isPaused;
        private bool _isCurrentlyPlaying;
        private string _currentlyPlayingFilename;
        private bool _isExpanded = true;
        private int _selectedPlayListIndex;
        private MvxNotifyTask _durationTaskNotifier;
        private double _playedPercentage;
        private bool _showSettingsPopUp;

        private readonly MvxInteraction _closeApp = new MvxInteraction();
        private readonly MvxInteraction<(double, double)> _setWindowWidthAndHeight = new MvxInteraction<(double, double)>();
        #endregion

        #region Properties
        public bool IsExpanded
        {
            get => _isExpanded;
            set => SetProperty(ref _isExpanded, value);
        }

        public bool IsPaused
        {
            get => _isPaused;
            set => SetProperty(ref _isPaused, value);
        }

        public bool IsCurrentlyPlaying
        {
            get => _isCurrentlyPlaying;
            set => SetProperty(ref _isCurrentlyPlaying, value);
        }

        public string CurrentlyPlayingFilename
        {
            get => _currentlyPlayingFilename;
            set => SetProperty(ref _currentlyPlayingFilename, value);
        }

        public int SelectedPlayListIndex
        {
            get => _selectedPlayListIndex;
            set => SetProperty(ref _selectedPlayListIndex, value);
        }

        public double PlayedPercentage
        {
            get => _playedPercentage;
            set => SetProperty(ref _playedPercentage, value);
        }

        public bool ShowSettingsPopUp
        {
            get => _showSettingsPopUp;
            set => SetProperty(ref _showSettingsPopUp, value);
        }

        public MvxObservableCollection<PlayListItemViewModel> PlayLists { get; set; }
            = new MvxObservableCollection<PlayListItemViewModel>();

        public MvxNotifyTask DurationTaskNotifier
        {
            get => _durationTaskNotifier;
            private set => SetProperty(ref _durationTaskNotifier, value);
        }
        #endregion

        #region Commands
        public IMvxCommand TogglePlaylistVisibilityCommand { get; private set; }
        public IMvxCommand CloseAppCommand { get; private set; }
        public IMvxCommand PreviousCommand { get; private set; }
        public IMvxCommand NextCommand { get; private set; }
        public IMvxCommand TogglePlayBackCommand { get; private set; }
        public IMvxCommand StopPlayBackCommand { get; private set; }
        public IMvxCommand<int> SkipCommand { get; private set; }
        public IMvxCommand SwitchPlayListsCommand { get; private set; }
        public IMvxCommand AddNewPlayListCommand { get; private set; }
        public IMvxCommand<PlayListItemViewModel> DeletePlayListCommand { get; private set; }
        public IMvxCommand<PlayListItemViewModel> DeleteAllPlayListsExceptCommand { get; private set; }
        public IMvxCommand OpenSettingsCommand { get; private set; }
        #endregion

        #region Interactors
        public IMvxCommand WindowLoadedCommand  { get; private set; }
        public IMvxInteraction CloseApp
            => _closeApp;

        public IMvxInteraction<(double, double)> SetWindowWidthAndHeight
            => _setWindowWidthAndHeight;
        #endregion

        public MainViewModel(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            IMvxLogProvider logger,
            IMvxNavigationService navigationService,
            ICastService castService,
            IPlayListsService playListsService,
            IAppSettingsService settingsService)
            : base(textProvider, messenger, logger.GetLogFor<MainViewModel>(), navigationService)
        {
            _castService = castService;
            _playListsService = playListsService;
            _settingsService = settingsService;
        }

        #region Methods
        public override async Task Initialize()
        {
            IsExpanded = _settingsService.IsPlayListExpanded;
            _castService.Init();

            var playLists = await _playListsService.GetAllPlayLists();
            PlayLists.AddRange(playLists);

            DurationTaskNotifier = MvxNotifyTask.Create(SetFileDurations);

            _castService.OnPositionChanged += OnFilePositionChanged;
            _castService.OnEndReached += OnFileEndReached;
            await base.Initialize();
        }

        public override void SetCommands()
        {
            base.SetCommands();
            WindowLoadedCommand = new MvxCommand(() =>
            {
                var tuple = (_settingsService.WindowWidth, _settingsService.WindowHeight);
                _setWindowWidthAndHeight.Raise(tuple);
            });

            TogglePlaylistVisibilityCommand = new MvxCommand(() =>
            {
                IsExpanded = !IsExpanded;
            });

            CloseAppCommand = new MvxCommand(HandleCloseApp);

            PreviousCommand = new MvxCommand(() => GoTo(false));

            NextCommand = new MvxCommand(() => GoTo(true));

            TogglePlayBackCommand = new MvxCommand(() =>
            {
                _castService.TogglePlayback();
                IsPaused = !IsPaused;
            });

            StopPlayBackCommand = new MvxCommand(StopPlayBack);

            SkipCommand = new MvxCommand<int>((seconds) =>
            {
                _castService.AddSeconds(seconds);
            });

            SwitchPlayListsCommand = new MvxCommand(SwitchPlayLists);

            AddNewPlayListCommand = new MvxCommand(() =>
            {
                System.Diagnostics.Debug.WriteLine("Adding playlist");
            });

            DeletePlayListCommand = new MvxCommand<PlayListItemViewModel>((item) =>
            {
                if (PlayLists.Count > 1)
                    PlayLists.Remove(item);
            });

            DeleteAllPlayListsExceptCommand = new MvxCommand<PlayListItemViewModel>((except) =>
            {
                if (PlayLists.Count <= 1)
                    return;
                var exceptIndex = PlayLists.IndexOf(except);
                PlayLists.Move(exceptIndex, 0);
                PlayLists.RemoveRange(1, PlayLists.Count - 1);
            });

            OpenSettingsCommand = new MvxCommand(() =>
            {
                ShowSettingsPopUp = !ShowSettingsPopUp;
            });
        }

        public override void RegisterMessages()
        {
            base.RegisterMessages();
            SubscriptionTokens.AddRange(new List<MvxSubscriptionToken>
            {
                Messenger.Subscribe<PlayFileMsg>((msg) => PlayFile(msg.File))
            });
        }

        public void SaveWindowWidthAndHeight(double width, double height)
        {
            _settingsService.WindowWidth = width;
            _settingsService.WindowHeight = height;
            _settingsService.IsPlayListExpanded = IsExpanded;
            _settingsService.SaveSettings();
        }

        private async Task SetFileDurations()
        {
            foreach (var playlist in PlayLists)
            {
                foreach (var item in playlist.Items)
                {
                    await item.SetDuration();
                }
            }
        }

        private void HandleCloseApp()
        {
            _castService.OnPositionChanged -= OnFilePositionChanged;
            _castService.OnEndReached -= OnFileEndReached;
            _castService.StopPlayback();
            _castService.CleanThemAll();
            _currentlyPlayedFile?.CleanUp();
            _closeApp.Raise();
        }

        private void SwitchPlayLists()
        {
            int tentativeIndex = SelectedPlayListIndex + 1;
            if (PlayLists.ElementAtOrDefault(tentativeIndex) != null)
            {
                SelectedPlayListIndex = tentativeIndex;
            }
            else
            {
                SelectedPlayListIndex = 0;
            }
        }

        private void GoTo(bool nextTrack)
        {
            if (_currentlyPlayedFile == null)
                return;

            var playlist = PlayLists.First(p => p.Id == _currentlyPlayedFile.PlayListId);
            var fileIndex = playlist.Items.IndexOf(_currentlyPlayedFile);
            if (fileIndex < 0)
                return;

            if (nextTrack)
                fileIndex++;
            else
                fileIndex--;
            var file = playlist.Items.ElementAtOrDefault(fileIndex);

            if (file is null)
            {
                return;
            }

            StopPlayBack();
            file.PlayCommand.Execute();
        }

        private void PlayFile(FileItemViewModel file)
        {
            _currentlyPlayedFile?.CleanUp();
            _currentlyPlayedFile = file;
            _currentlyPlayedFile.ListenEvents();
            SetCurrentlyPlayingInfo(file.Filename, true);
            _castService.StartPlay(file.Path, true);
            if (file.PlayedPercentage != 0)
            {
                _castService.GoToPosition((float)file.PlayedPercentage);
            }
        }

        private void StopPlayBack()
        {
            _castService.StopPlayback();
            SetCurrentlyPlayingInfo(null, false);
            IsPaused = false;
        }

        private void SetCurrentlyPlayingInfo(string filename, bool isPlaying, float playedPercentage = 0)
        {
            OnFilePositionChanged(playedPercentage);
            CurrentlyPlayingFilename = filename;
            IsCurrentlyPlaying = isPlaying;
        }

        private void OnFilePositionChanged(float playedPercentage)
            => PlayedPercentage = playedPercentage;

        private void OnFileEndReached()
        {
            SetCurrentlyPlayingInfo(null, false);
            GoTo(true);
        } 
        #endregion
    }
}
