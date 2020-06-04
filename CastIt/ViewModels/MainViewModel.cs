using CastIt.Common;
using CastIt.Common.Utils;
using CastIt.GoogleCast.Models.Media;
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
    //TODO: IF YOU PAUSE THE VIDEO, AND PLAY IT FROM YOUR PHONE, THE ICONS ARE NOT UPDATED
    //TODO: IF YOU PAUSE THE VIDEO, AND PLAY IT AGAIN, THE PLAYED TIME SYNC WILL BE LOST
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
        private bool _isBusy;

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

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public MvxObservableCollection<FileItemOptionsViewModel> CurrentFileVideos { get; }
            = new MvxObservableCollection<FileItemOptionsViewModel>();
        public MvxObservableCollection<FileItemOptionsViewModel> CurrentFileAudios { get; }
            = new MvxObservableCollection<FileItemOptionsViewModel>();
        public MvxObservableCollection<FileItemOptionsViewModel> CurrentFileSubTitles { get; }
            = new MvxObservableCollection<FileItemOptionsViewModel>();
        public MvxObservableCollection<FileItemOptionsViewModel> CurrentFileQualities { get; }
            = new MvxObservableCollection<FileItemOptionsViewModel>();

        public int CurrentFileVideoStreamIndex
            => CurrentFileVideos.FirstOrDefault(f => f.IsSelected)?.Id ?? 0;
        public int CurrentFileAudioStreamIndex
            => CurrentFileAudios.FirstOrDefault(f => f.IsSelected)?.Id ?? 0;
        public int CurrentFileSubTitleStreamIndex
            => CurrentFileSubTitles.FirstOrDefault(f => f.IsSelected)?.Id ?? -1;
        public int CurrentFileQuality
            => CurrentFileQualities.FirstOrDefault(f => f.IsSelected)?.Id ?? 360;
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
        public IMvxAsyncCommand<FileItemOptionsViewModel> FileOptionsChangedCommand { get; private set; }
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
            _castService.QualitiesChanged += QualitiesChanged;

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

            SkipCommand = new MvxAsyncCommand<int>(SkipSeconds);

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

            FileOptionsChangedCommand = new MvxAsyncCommand<FileItemOptionsViewModel>(FileOptionsChanged);
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
            IsBusy = true;
            await _castService.GoToSeconds(
                CurrentFileVideoStreamIndex,
                CurrentFileAudioStreamIndex,
                CurrentFileSubTitleStreamIndex,
                CurrentFileQuality,
                seconds);
            IsBusy = false;
        }

        private async Task SkipSeconds(int seconds)
        {
            IsBusy = true;
            await _castService.AddSeconds(
                CurrentFileVideoStreamIndex,
                CurrentFileAudioStreamIndex,
                CurrentFileSubTitleStreamIndex,
                CurrentFileQuality,
                seconds);
            IsBusy = false;
        }

        private Task SetFileDurations()
        {
            Logger.Info($"{nameof(SetFileDurations)}: Setting file duration to all the files");
            var tasks = PlayLists.Select(pl => Task.Run(async () =>
            {
                foreach (var file in pl.Items)
                {
                    await file.SetFileInfo(_setDurationTokenSource.Token);
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
            playlist.CleanUp();
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
            foreach (var playlist in items)
            {
                playlist.CleanUp();
            }

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

        private async Task<bool> PlayFile(FileItemViewModel file, bool force, bool fileOptionsChanged = false)
        {
            if (!file.Exists)
            {
                await ShowSnackbarMsg(GetText("FileDoesntExist"));
                return false;
            }

            if (file == _currentlyPlayedFile && !force && !fileOptionsChanged)
            {
                await ShowSnackbarMsg(GetText("FileIsAlreadyBeingPlayed"));
                return false;
            }

            if (string.IsNullOrEmpty(file.Duration))
            {
                Logger.Info(
                    $"{nameof(PlayFile)}: Cant play file = {file.Filename} yet, " +
                    $"because im still setting the duration for some files.");
                await ShowSnackbarMsg(GetText("FileIsNotReadyYet"));
                return false;
            }

            if (_castService.AvailableDevices.Count == 0)
            {
                await ShowSnackbarMsg(GetText("NoDevicesWereFound"));
                return false;
            }

            if (file.IsUrlFile && !NetworkUtils.IsInternetAvailable())
            {
                await ShowSnackbarMsg(GetText("NoInternetConnection"));
                return false;
            }

            IsBusy = true;

            _currentlyPlayedFile?.CleanUp();
            _currentlyPlayedFile = file;
            _currentlyPlayedFile.ListenEvents();

            SetCurrentlyPlayingInfo(file.Filename, true);
            if (!fileOptionsChanged)
                SetAvailableAudiosAndSubTitles();

            Logger.Info($"{nameof(PlayFile)}: Trying to play file = {file.Filename}");

            var playList = PlayLists.First(pl => pl.Id == file.PlayListId);
            playList.SelectedItem = file;

            try
            {
                //TODO: CHECK IF WE SHOULD REMOVE THIS MEDIASTATUS
                MediaStatus mediaStatus;
                if (file.CanStartPlayingFromCurrentPercentage &&
                    !file.IsUrlFile &&
                    !force &&
                    !_settingsService.StartFilesFromTheStart)
                {
                    Logger.Info($"{nameof(PlayFile)}: File will be resumed from = {file.PlayedPercentage} %");
                    mediaStatus = await _castService.GoToPosition(
                        file.Path,
                        CurrentFileVideoStreamIndex,
                        CurrentFileAudioStreamIndex,
                        CurrentFileSubTitleStreamIndex,
                        CurrentFileQuality,
                        file.PlayedPercentage,
                        file.TotalSeconds);
                }
                else
                {
                    Logger.Info($"{nameof(PlayFile)}: Playing file from the start");
                    mediaStatus = await _castService.StartPlay(
                        file.Path,
                        CurrentFileVideoStreamIndex,
                        CurrentFileAudioStreamIndex,
                        CurrentFileSubTitleStreamIndex,
                        CurrentFileQuality);
                }

                if (file.IsUrlFile)
                {
                    file.SetDuration(mediaStatus?.Media?.Duration ?? 0);
                    await RaisePropertyChanged(() => CurrentFileDuration);
                }

                CurrentFileThumbnail = _castService.GetFirstThumbnail();
                _castService.GenerateThumbmnails(file.Path);

                _onSkipOrPrevious = false;

                Logger.Info($"{nameof(PlayFile)}: Playing...");

                return true;
            }
            catch (Exception e)
            {
                Logger.Error(e, $"{nameof(PlayFile)}: Unknown error occurred");
                playList.SelectedItem = null;
                await StopPlayBack();
                await ShowSnackbarMsg(GetText("CouldntPlayFile"));
                return false;
            }
            finally
            {
                IsBusy = false;
            }
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
            ElapsedTimeString = string.Empty;
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
            if (_currentlyPlayedFile.IsUrlFile && _currentlyPlayedFile.TotalSeconds <= 0)
                ElapsedTimeString = $"{elapsed}";
            else
                ElapsedTimeString = $"{elapsed} / {total}";
        }

        private void OnFilePositionChanged(double playedPercentage)
            => PlayedPercentage = playedPercentage;

        private void OnFileEndReached()
        {
            Logger.Info($"{nameof(OnFileEndReached)}: End reached for file = {_currentlyPlayedFile?.Path}");

            SetCurrentlyPlayingInfo(null, false);

            if (_settingsService.PlayNextFileAutomatically)
            {
                GoTo(true);
            }
            else
            {
                _currentlyPlayedFile?.CleanUp();
                _currentlyPlayedFile = null;
                IsPaused = false;
            }
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

        private void SetAvailableAudiosAndSubTitles()
        {
            Logger.Info($"{nameof(SetAvailableAudiosAndSubTitles)}: Cleaning current file videos, audios and subs streams");
            CurrentFileVideos.Clear();
            CurrentFileAudios.Clear();
            CurrentFileSubTitles.Clear();
            CurrentFileQualities.Clear();
            if (_currentlyPlayedFile?.FileInfo == null)
            {
                Logger.Warn($"{nameof(SetAvailableAudiosAndSubTitles)}: Current file = {_currentlyPlayedFile?.Path} doesnt have a fileinfo");
                return;
            }

            Logger.Info($"{nameof(SetAvailableAudiosAndSubTitles)}: Setting available file videos, audios and subs streams");
            bool isSelected = true;
            bool isEnabled = isEnabled = _currentlyPlayedFile.FileInfo.Videos.Count > 1;
            foreach (var video in _currentlyPlayedFile.FileInfo.Videos)
            {
                CurrentFileVideos.Add(new FileItemOptionsViewModel
                {
                    Id = video.Index,
                    IsSelected = isSelected,
                    IsEnabled = isEnabled,
                    IsVideo = true,
                    Text = video.VideoText
                });
                isSelected = false;
            }

            isSelected = true;
            isEnabled = _currentlyPlayedFile.FileInfo.Audios.Count > 1;
            foreach (var audio in _currentlyPlayedFile.FileInfo.Audios)
            {
                CurrentFileAudios.Add(new FileItemOptionsViewModel
                {
                    Id = audio.Index,
                    IsSelected = isSelected,
                    IsEnabled = isEnabled,
                    IsAudio = true,
                    Text = audio.AudioText
                });
                isSelected = false;
            }

            isEnabled = _currentlyPlayedFile.FileInfo.SubTitles.Count > 1;
            if (_currentlyPlayedFile.FileInfo.SubTitles.Count > 0)
            {
                CurrentFileSubTitles.Add(new FileItemOptionsViewModel
                {
                    Id = -1,
                    IsSubTitle = true,
                    IsSelected = true,
                    IsEnabled = isEnabled,
                    Text = GetText("None")
                });
            }

            isEnabled = _currentlyPlayedFile.FileInfo.SubTitles.Count > 0;
            foreach (var subtitle in _currentlyPlayedFile.FileInfo.SubTitles)
            {
                CurrentFileSubTitles.Add(new FileItemOptionsViewModel
                {
                    Id = subtitle.Index,
                    IsSelected = false,
                    IsEnabled = isEnabled,
                    IsSubTitle = true,
                    Text = subtitle.SubTitleText
                });
            }
        }

        private Task FileOptionsChanged(FileItemOptionsViewModel selectedItem)
        {
            if (selectedItem == null)
            {
                Logger.Warn($"{nameof(FileOptionsChanged)}: Selected option is null");
                return Task.CompletedTask;
            }

            var options = selectedItem.IsVideo
                ? CurrentFileVideos
                : selectedItem.IsAudio
                    ? CurrentFileAudios
                    : selectedItem.IsSubTitle
                        ? CurrentFileSubTitles
                        : selectedItem.IsQuality
                            ? CurrentFileQualities
                            : throw new ArgumentOutOfRangeException(
                                "File option changed, but the one that changes is not audio, " +
                                "nor video, nor subs, nor quality");

            if (selectedItem.IsSelected)
            {
                return Task.CompletedTask;
            }

            foreach (var item in options)
            {
                if (item.IsSelected)
                    item.IsSelected = false;
            }

            selectedItem.IsSelected = true;
            Logger.Info($"{nameof(FileOptionsChanged)}: StreamId = {selectedItem.Id}  was selected. Text = {selectedItem.Text}");

            return PlayFile(_currentlyPlayedFile, false, true);
        }

        private void QualitiesChanged(int selectedQuality, List<int> qualities)
        {
            var vms = qualities.OrderBy(q => q).Select(q => new FileItemOptionsViewModel
            {
                Id = q,
                IsSelected = selectedQuality == q,
                IsEnabled = qualities.Count > 1,
                IsQuality = true,
                Text = $"{q}"
            }).ToList();
            CurrentFileQualities.ReplaceWith(vms);
        }
        #endregion
    }
}
