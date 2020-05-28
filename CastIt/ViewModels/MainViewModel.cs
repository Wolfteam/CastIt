using CastIt.Common;
using CastIt.Common.Utils;
using CastIt.Interfaces;
using CastIt.Models.Messages;
using CastIt.ViewModels.Dialogs;
using CastIt.ViewModels.Items;
using MvvmCross;
using MvvmCross.Commands;
using MvvmCross.Logging;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CastIt.ViewModels
{
    //TODO: SWITCH TO FFPROBE TO RETRIEVE FILE METADATA ?
    //TODO: IF YOU PAUSE THE VIDEO, AND PLAY IT FROM YOUR PHONE, THE ICONS ARE NOT UPDATED
    //TODO: IF YOU PAUSE THE VIDEO, AND PLAY IT AGAIN, THE PLAYED TIME SYNC WILL BE LOST
    //TODO: QUEUING THE MEDIA EVENTS DOES NOT GUARANTEE THE ORDER OF EXECUTION (E.G: if you enqueue a stop and a play, the final result could be play and stop)
    public class MainViewModel : BaseViewModel
    {
        #region Members
        private readonly ICastService _castService;
        private readonly IPlayListsService _playListsService;
        private readonly IAppSettingsService _settingsService;
        private readonly IMvxNavigationService _navigationService;

        private FileItemViewModel _currentlyPlayedFile;
        private bool _isPaused;
        private bool _isCurrentlyPlaying;
        private string _currentlyPlayingFilename;
        private bool _isExpanded = true;
        private int _selectedPlayListIndex;
        private MvxNotifyTask _durationTaskNotifier;
        private double _playedPercentage;
        private bool _showSettingsPopUp;
        private bool _showDevicesPopUp;
        private string _elapsedTimeString;
        private string _currentFileThumbnail;
        private double _currentPlayedSeconds;
        private string _previewThumbnailImg;
        private bool _onSkipOrPrevious;
        private readonly CancellationTokenSource _setDurationTokenSource = new CancellationTokenSource();
        private bool _showSnackbar;
        private string _snackbarMsg;
        private string _snackBarActionMsg;

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
            set
            {
                if (value == _playedPercentage)
                    return;
                SetProperty(ref _playedPercentage, value);
            }
        }

        public double CurrentFileDuration
            => _currentlyPlayedFile?.TotalSeconds ?? 1;

        public double CurrentPlayedSeconds
        {
            get => _currentPlayedSeconds;
            set
            {
                if (value == _currentPlayedSeconds)
                    return;
                else if (value > CurrentFileDuration)
                    SetProperty(ref _currentPlayedSeconds, CurrentFileDuration);
                else
                    SetProperty(ref _currentPlayedSeconds, value);
            }
        }

        public bool ShowSettingsPopUp
        {
            get => _showSettingsPopUp;
            set => SetProperty(ref _showSettingsPopUp, value);
        }

        public bool ShowDevicesPopUp
        {
            get => _showDevicesPopUp;
            set => SetProperty(ref _showDevicesPopUp, value);
        }

        public string ElapsedTimeString
        {
            get => _elapsedTimeString;
            set => this.RaiseAndSetIfChanged(ref _elapsedTimeString, value);
        }

        public string CurrentFileThumbnail
        {
            get => _currentFileThumbnail;
            set => SetProperty(ref _currentFileThumbnail, value);
        }

        public string PreviewThumbnailImg
        {
            get => _previewThumbnailImg;
            set => this.RaiseAndSetIfChanged(ref _previewThumbnailImg, value);
        }

        public MvxObservableCollection<PlayListItemViewModel> PlayLists { get; set; }
            = new MvxObservableCollection<PlayListItemViewModel>();

        public MvxNotifyTask DurationTaskNotifier
        {
            get => _durationTaskNotifier;
            private set => SetProperty(ref _durationTaskNotifier, value);
        }

        public bool ShowSnackbar
        {
            get => _showSnackbar;
            set => SetProperty(ref _showSnackbar, value);
        }

        public string SnackbarMsg
        {
            get => _snackbarMsg;
            set => SetProperty(ref _snackbarMsg, value);
        }

        public string SnackBarActionMsg
        {
            get => _snackBarActionMsg;
            set => SetProperty(ref _snackBarActionMsg, value);
        }
        #endregion

        #region Commands
        public IMvxCommand TogglePlaylistVisibilityCommand { get; private set; }
        public IMvxAsyncCommand CloseAppCommand { get; private set; }
        public IMvxCommand PreviousCommand { get; private set; }
        public IMvxCommand NextCommand { get; private set; }
        public IMvxCommand TogglePlayBackCommand { get; private set; }
        public IMvxAsyncCommand StopPlayBackCommand { get; private set; }
        public IMvxAsyncCommand<int> SkipCommand { get; private set; }
        public IMvxCommand SwitchPlayListsCommand { get; private set; }
        public IMvxAsyncCommand AddNewPlayListCommand { get; private set; }
        public IMvxAsyncCommand<PlayListItemViewModel> DeletePlayListCommand { get; private set; }
        public IMvxAsyncCommand<PlayListItemViewModel> DeleteAllPlayListsExceptCommand { get; private set; }
        public IMvxCommand OpenSettingsCommand { get; private set; }
        public IMvxCommand OpenDevicesCommand { get; private set; }
        public IMvxCommand SnackbarActionCommand { get; private set; }
        public IMvxAsyncCommand<long> GoToSecondsCommand { get; private set; }
        public IMvxAsyncCommand ShowDownloadDialogCommand { get; private set; }
        #endregion

        #region Interactors
        public IMvxInteraction CloseApp
            => _closeApp;
        public IMvxInteraction<(double, double)> SetWindowWidthAndHeight
            => _setWindowWidthAndHeight;
        #endregion

        public MainViewModel(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            IMvxLogProvider logger,
            ICastService castService,
            IPlayListsService playListsService,
            IAppSettingsService settingsService,
            IMvxNavigationService navigationService)
            : base(textProvider, messenger, logger.GetLogFor<MainViewModel>())
        {
            _castService = castService;
            _playListsService = playListsService;
            _settingsService = settingsService;
            _navigationService = navigationService;
        }

        #region Methods
        public override async Task Initialize()
        {
            IsExpanded = _settingsService.IsPlayListExpanded;
            Logger.Info($"{nameof(Initialize)}: Initializing cast service...");
            _castService.Init();

            Logger.Info($"{nameof(Initialize)}: Getting all playlists...");
            var playLists = await _playListsService.GetAllPlayLists();
            PlayLists.AddRange(playLists.OrderBy(pl => pl.Position));
            foreach (var playlist in playLists)
            {
                var files = await _playListsService.GetAllFiles(playlist.Id);
                playlist.Items.AddRange(files);
            }

            Logger.Info($"{nameof(Initialize)}: Setting cast events..");
            _castService.OnTimeChanged += OnFileDurationChanged;
            _castService.OnPositionChanged += OnFilePositionChanged;
            _castService.OnEndReached += OnFileEndReached;

            Logger.Info($"{nameof(Initialize)}: Applying app theme and accent color...");
            WindowsUtils.ChangeTheme(_settingsService.AppTheme, _settingsService.AccentColor);

            Logger.Info($"{nameof(Initialize)}: Deleting old preview files...");
            try
            {
                FileUtils.DeleteFilesInDirectory(FileUtils.GetPreviewsPath());
            }
            catch (Exception e)
            {
                Logger.Error(e, $"{nameof(Initialize)}: Error occurred while trying to delete previews");
            }

            Logger.Info($"{nameof(Initialize)}: Completed");
            await base.Initialize();
        }

        public override void SetCommands()
        {
            base.SetCommands();

            TogglePlaylistVisibilityCommand = new MvxCommand(() => IsExpanded = !IsExpanded);

            CloseAppCommand = new MvxAsyncCommand(HandleCloseApp);

            PreviousCommand = new MvxCommand(() => GoTo(false));

            NextCommand = new MvxCommand(() => GoTo(true));

            TogglePlayBackCommand = new MvxCommand(() =>
            {
                _castService.TogglePlayback();
                IsPaused = !IsPaused;
            });

            StopPlayBackCommand = new MvxAsyncCommand(StopPlayBack);

            SkipCommand = new MvxAsyncCommand<int>(
                async (seconds) => await _castService.AddSeconds(seconds));

            SwitchPlayListsCommand = new MvxCommand(SwitchPlayLists);

            AddNewPlayListCommand = new MvxAsyncCommand(AddNewPlayList);

            DeletePlayListCommand = new MvxAsyncCommand<PlayListItemViewModel>(DeletePlayList);

            DeleteAllPlayListsExceptCommand = new MvxAsyncCommand<PlayListItemViewModel>(DeleteAllPlayLists);

            OpenSettingsCommand = new MvxCommand(() => ShowSettingsPopUp = true);

            OpenDevicesCommand = new MvxCommand(() => ShowDevicesPopUp = true);

            SnackbarActionCommand = new MvxCommand(() => ShowSnackbar = false);

            GoToSecondsCommand = new MvxAsyncCommand<long>(GoToSeconds);

            ShowDownloadDialogCommand = new MvxAsyncCommand(async () =>
            {
                bool filesWereDownloaded = await _navigationService.Navigate<DownloadDialogViewModel, bool>();
                if (!filesWereDownloaded)
                    CloseAppCommand.Execute();
                else
                    await ShowSnackbarMsg(GetText("AppIsRdyToUse"));
            });
        }

        public override void RegisterMessages()
        {
            base.RegisterMessages();
            SubscriptionTokens.AddRange(new List<MvxSubscriptionToken>
            {
                Messenger.Subscribe<PlayFileMessage>(async(msg) => await PlayFile(msg.File, msg.Force))
            });
        }

        public override void ViewAppeared()
        {
            base.ViewAppeared();
            var tuple = (_settingsService.WindowWidth, _settingsService.WindowHeight);
            _setWindowWidthAndHeight.Raise(tuple);

            Logger.Info($"{nameof(ViewAppeared)}: Creating the file duration task..");
            DurationTaskNotifier = MvxNotifyTask.Create(SetFileDurations());
            string path = FileUtils.GetFFMpegPath();
            if (File.Exists(path))
                return;

            Logger.Info($"{nameof(ViewAppeared)}: Ffmpeg is not in user folder, showing download dialog...");
            ShowDownloadDialogCommand.Execute();
        }

        public void SaveChangesBeforeClosing(
            double width,
            double height,
            Dictionary<long, int> positions)
        {
            _settingsService.WindowWidth = width;
            _settingsService.WindowHeight = height;
            _settingsService.IsPlayListExpanded = IsExpanded;
            _settingsService.SaveSettings();

            var files = PlayLists.SelectMany(pl => pl.Items)
                .Where(f => f.WasPlayed || f.PositionChanged)
                .ToList();
            _playListsService.SaveChangesBeforeClosingApp(positions, files);
        }

        public long TrySetThumbnail(double sliderWidth, double mouseX)
        {
            if (_currentlyPlayedFile == null)
            {
                PreviewThumbnailImg = null;
                return -1;
            }

            long tentativeSecond = GetMainProgressBarSeconds(sliderWidth, mouseX);

            if (FileUtils.IsMusicFile(_currentlyPlayedFile.Path))
            {
                PreviewThumbnailImg = CurrentFileThumbnail;
                return tentativeSecond;
            }
            PreviewThumbnailImg = FileUtils.GetClosestThumbnail(_currentlyPlayedFile.Path, tentativeSecond);
            return tentativeSecond;
        }

        public long GetMainProgressBarSeconds(double sliderWidth, double mouseX)
            => Convert.ToInt64(mouseX * _currentlyPlayedFile.TotalSeconds / sliderWidth);

        private async Task GoToSeconds(long seconds)
        {
            await _castService.GoToSeconds(seconds);
        }

        private Task SetFileDurations()
        {
            Logger.Info($"{nameof(SetFileDurations)}: Setting file duration to all the files");
            var tasks = PlayLists.Select(pl => Task.Run(async () =>
            {
                foreach (var file in pl.Items)
                {
                    await file.SetDuration(_setDurationTokenSource.Token);
                }
            }, _setDurationTokenSource.Token)).ToList();

            return Task.WhenAll(tasks);
        }

        private async Task AddNewPlayList()
        {
            var vm = Mvx.IoCProvider.Resolve<PlayListItemViewModel>();
            vm.Name = $"New PlayList {PlayLists.Count}";
            vm.Position = PlayLists.Max(pl => pl.Position) + 1;

            var playList = await _playListsService.AddNewPlayList(vm.Name, vm.Position);
            vm.Id = playList.Id;

            PlayLists.Add(vm);
            SelectedPlayListIndex = PlayLists.Count - 1;
        }

        private async Task DeletePlayList(PlayListItemViewModel playlist)
        {
            if (playlist == null)
                return;

            await _playListsService.DeletePlayList(playlist.Id);
            PlayLists.Remove(playlist);
        }

        private async Task DeleteAllPlayLists(PlayListItemViewModel except)
        {
            if (PlayLists.Count <= 1)
                return;
            var exceptIndex = PlayLists.IndexOf(except);

            var items = new List<PlayListItemViewModel>();
            for (int i = 0; i < PlayLists.Count; i++)
            {
                if (i == exceptIndex)
                    continue;
                items.Add(PlayLists[i]);
            }

            await _playListsService.DeletePlayLists(items.Select(pl => pl.Id).ToList());
            PlayLists.RemoveItems(items);
        }

        private async Task HandleCloseApp()
        {
            _setDurationTokenSource.Cancel();
            foreach (var playList in PlayLists)
            {
                playList.CleanUp();
            }
            //await _playListsService.SavePlayLists(PlayLists.ToList());
            await StopPlayBack();
            _castService.CleanThemAll();
            _currentlyPlayedFile?.CleanUp();
            _castService.OnTimeChanged -= OnFileDurationChanged;
            _castService.OnPositionChanged -= OnFilePositionChanged;
            _castService.OnEndReached -= OnFileEndReached;
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
            if (_currentlyPlayedFile == null || _onSkipOrPrevious)
                return;

            var playlist = PlayLists.First(p => p.Id == _currentlyPlayedFile.PlayListId);
            var fileIndex = playlist.Items.IndexOf(_currentlyPlayedFile);
            if (fileIndex < 0)
                return;

            _onSkipOrPrevious = true;
            if (nextTrack)
                fileIndex++;
            else
                fileIndex--;
            var file = playlist.Items.ElementAtOrDefault(fileIndex);

            if (file is null)
            {
                Logger.Warn($"{nameof(GoTo)}: File at index = {fileIndex} in playListId {playlist.Id} was not found");
                return;
            }

            file.PlayCommand.Execute();
        }

        private async Task PlayFile(FileItemViewModel file, bool force)
        {
            if (!file.Exists)
            {
                await ShowSnackbarMsg(GetText("FileDoesntExist"));
                return;
            }

            if (file == _currentlyPlayedFile && !force)
            {
                await ShowSnackbarMsg(GetText("FileIsAlreadyBeingPlayed"));
                return;
            }

            if (string.IsNullOrEmpty(file.Duration))
            {
                Logger.Info(
                    $"{nameof(PlayFile)}: Cant play file = {file.Filename} yet, " +
                    $"because im still setting the duration for some files.");
                await ShowSnackbarMsg(GetText("FileIsNotReadyYet"));
                return;
            }

            if (_castService.AvailableDevices.Count == 0)
            {
                await ShowSnackbarMsg(GetText("NoDevicesWereFound"));
                return;
            }

            _currentlyPlayedFile?.CleanUp();
            _currentlyPlayedFile = file;
            _currentlyPlayedFile.ListenEvents();
            SetCurrentlyPlayingInfo(file.Filename, true);

            Logger.Info($"{nameof(PlayFile)}: Trying to play file = {file.Filename}");

            var playList = PlayLists.First(pl => pl.Id == file.PlayListId);
            playList.SelectedItem = file;

            if (file.CanStartPlayingFromCurrentPercentage && !force && !_settingsService.StartFilesFromTheStart)
            {
                Logger.Info($"{nameof(PlayFile)}: File will be resumed from = {file.PlayedPercentage} %");
                await _castService.GoToPosition(file.Path, file.PlayedPercentage, file.TotalSeconds);
            }
            else
            {
                Logger.Info($"{nameof(PlayFile)}: Playing file from the start");
                await _castService.StartPlay(file.Path);
            }

            CurrentFileThumbnail = _castService.GetFirstThumbnail();
            await Task.Run(() => _castService.GenerateThumbmnails(file.Path));

            _onSkipOrPrevious = false;

            Logger.Info($"{nameof(PlayFile)}: Playing...");
        }

        private async Task StopPlayBack()
        {
            await _castService.StopPlayback();
            _currentlyPlayedFile?.CleanUp();
            _currentlyPlayedFile = null;
            SetCurrentlyPlayingInfo(null, false);
            IsPaused = false;
        }

        private void SetCurrentlyPlayingInfo(
            string filename,
            bool isPlaying,
            double playedPercentage = 0,
            double playedSeconds = 0)
        {
            OnFilePositionChanged(playedPercentage);
            CurrentFileThumbnail = null;
            CurrentlyPlayingFilename = filename;
            IsCurrentlyPlaying = isPlaying;
            CurrentPlayedSeconds = playedSeconds;
            RaisePropertyChanged(() => CurrentFileDuration);
        }

        private void OnFileDurationChanged(double seconds)
        {
            if (_currentlyPlayedFile is null)
                return;

            CurrentPlayedSeconds = seconds;

            var elapsed = TimeSpan.FromSeconds(seconds)
                .ToString(AppConstants.FullElapsedTimeFormat);
            var total = TimeSpan.FromSeconds(_currentlyPlayedFile.TotalSeconds)
                .ToString(AppConstants.FullElapsedTimeFormat);
            if (_currentlyPlayedFile.IsUrlFile)
                ElapsedTimeString = $"{elapsed}";
            else
                ElapsedTimeString = $"{elapsed} / {total}";
        }

        private void OnFilePositionChanged(double playedPercentage)
            => PlayedPercentage = playedPercentage;

        private void OnFileEndReached()
        {
            ElapsedTimeString = string.Empty;
            SetCurrentlyPlayingInfo(null, false);
            GoTo(true);
        }

        private async Task ShowSnackbarMsg(string msg, string actionContent = null)
        {
            if (ShowSnackbar)
                return;

            if (string.IsNullOrEmpty(actionContent))
            {
                actionContent = GetText("Dismiss");
            }

            ShowSnackbar = true;
            SnackbarMsg = msg;
            SnackBarActionMsg = actionContent;
            //The snacbarmsgqueue has a problem that sometimes
            //it doesnt show the msg, thats why we use this implementation
            await Task.Delay(2000);

            SnackbarActionCommand.Execute();
        }
        #endregion
    }
}
