using AutoMapper;
using CastIt.Application.Common.Extensions;
using CastIt.Application.Common.Utils;
using CastIt.Application.Interfaces;
using CastIt.Domain.Exceptions;
using CastIt.Infrastructure.Interfaces;
using CastIt.Interfaces;
using CastIt.Models.Messages;
using CastIt.Server.Interfaces;
using CastIt.ViewModels.Dialogs;
using CastIt.ViewModels.Items;
using Microsoft.Extensions.Logging;
using MvvmCross;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CastIt.ViewModels
{
    public partial class MainViewModel : BaseViewModel<List<PlayListItemViewModel>>, IViewForMediaWebSocket
    {
        #region Members
        private const int NoStreamSelectedId = -1;
        private const int DefaultSelectedStreamId = 0;
        private const int DefaultQualitySelected = 360;

        private readonly ICastService _castService;
        private readonly IAppDataService _playListsService;
        private readonly IAppSettingsService _settingsService;
        private readonly IMvxNavigationService _navigationService;
        private readonly ITelemetryService _telemetryService;
        private readonly IAppWebServer _appWebServer;
        private readonly IMapper _mapper;
        private readonly IFileWatcherService _fileWatcherService;
        private readonly IFileService _fileService;

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
        private bool _showSnackbar;
        private string _snackbarMsg;
        private string _snackBarActionMsg;
        private bool _isBusy;
        private double _volumeLevel;
        private bool _isMuted;

        private readonly CancellationTokenSource _setDurationTokenSource = new CancellationTokenSource();
        private readonly CancellationTokenSource _webServerCancellationToken = new CancellationTokenSource();

        private readonly MvxInteraction _closeApp = new MvxInteraction();
        private readonly MvxInteraction _openSubTitleFileDialog = new MvxInteraction();
        private readonly MvxInteraction<PlayListItemViewModel> _beforeDeletingPlayList = new MvxInteraction<PlayListItemViewModel>();
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
            set => this.RaiseAndSetIfChanged(ref _isPaused, value);
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
            set => this.RaiseAndSetIfChanged(ref _playedPercentage, value);
        }

        public double CurrentFileDuration
            => _currentlyPlayedFile?.TotalSeconds ?? 1; //Has to be one, in order for the slider to show correctly

        public double CurrentPlayedSeconds
        {
            get => _currentPlayedSeconds;
            set
            {
                if (value == _currentPlayedSeconds)
                    return;
                if (value > CurrentFileDuration)
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
            set => this.RaiseAndSetIfChanged(ref _currentFileThumbnail, value);
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

        public double VolumeLevel
        {
            get => _volumeLevel;
            set => this.RaiseAndSetIfChanged(ref _volumeLevel, value);
        }

        public bool IsMuted
        {
            get => _isMuted;
            set => this.RaiseAndSetIfChanged(ref _isMuted, value);
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
            => CurrentFileVideos.FirstOrDefault(f => f.IsSelected)?.Id ?? DefaultSelectedStreamId;
        public int CurrentFileAudioStreamIndex
            => CurrentFileAudios.FirstOrDefault(f => f.IsSelected)?.Id ?? DefaultSelectedStreamId;
        public int CurrentFileSubTitleStreamIndex
            => CurrentFileSubTitles.FirstOrDefault(f => f.IsSelected)?.Id ?? NoStreamSelectedId;
        public int CurrentFileQuality
            => CurrentFileQualities.FirstOrDefault(f => f.IsSelected)?.Id ?? DefaultQualitySelected;
        #endregion

        #region Commands
        public IMvxCommand TogglePlaylistVisibilityCommand { get; private set; }
        public IMvxAsyncCommand CloseAppCommand { get; private set; }
        public IMvxCommand PreviousCommand { get; private set; }
        public IMvxCommand NextCommand { get; private set; }
        public IMvxAsyncCommand TogglePlayBackCommand { get; private set; }
        public IMvxAsyncCommand StopPlayBackCommand { get; private set; }
        public IMvxAsyncCommand<int> SkipCommand { get; private set; }
        public IMvxCommand SwitchPlayListsCommand { get; private set; }
        public IMvxAsyncCommand AddNewPlayListCommand { get; private set; }
        public MvxCommand<PlayListItemViewModel> DeletePlayListCommand { get; private set; }
        public IMvxAsyncCommand<PlayListItemViewModel> DeleteAllPlayListsExceptCommand { get; private set; }
        public IMvxCommand OpenSettingsCommand { get; private set; }
        public IMvxCommand OpenDevicesCommand { get; private set; }
        public IMvxCommand SnackbarActionCommand { get; private set; }
        public IMvxAsyncCommand<long> GoToSecondsCommand { get; private set; }
        public IMvxAsyncCommand ShowDownloadDialogCommand { get; private set; }
        public IMvxAsyncCommand<FileItemOptionsViewModel> FileOptionsChangedCommand { get; private set; }
        public IMvxCommand OpenSubTitleFileDialogCommand { get; private set; }
        public IMvxAsyncCommand<string> SetSubTitlesCommand { get; private set; }
        public IMvxAsyncCommand SetVolumeCommand { get; private set; }
        public IMvxAsyncCommand ToggleMuteCommand { get; private set; }
        #endregion

        #region Interactors
        public IMvxInteraction CloseApp
            => _closeApp;
        public IMvxInteraction OpenSubTitleFileDialog
            => _openSubTitleFileDialog;
        public IMvxInteraction<PlayListItemViewModel> BeforeDeletingPlayList
            => _beforeDeletingPlayList;
        #endregion

        public MainViewModel(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            ILogger<MainViewModel> logger,
            ICastService castService,
            IAppDataService playListsService,
            IAppSettingsService settingsService,
            IMvxNavigationService navigationService,
            ITelemetryService telemetryService,
            IAppWebServer appWebServer,
            IMapper mapper,
            IFileWatcherService fileWatcher,
            IFileService fileService)
            : base(textProvider, messenger, logger)
        {
            _castService = castService;
            _playListsService = playListsService;
            _settingsService = settingsService;
            _navigationService = navigationService;
            _telemetryService = telemetryService;
            _appWebServer = appWebServer;
            _mapper = mapper;
            _fileWatcherService = fileWatcher;
            _fileService = fileService;
        }

        #region Methods
        public override void Prepare(List<PlayListItemViewModel> parameter)
        {
            PlayLists.AddRange(parameter.OrderBy(pl => pl.Position));
        }

        public override async Task Initialize()
        {
            IsExpanded = _settingsService.IsPlayListExpanded;
            Logger.LogInformation($"{nameof(Initialize)}: Initializing cast service...");
            _castService.Init();

            //This needs to happen after the playlist/files are initialized, otherwise, you will be sending a lot of ws msgs
            Logger.LogInformation($"{nameof(Initialize)}: Initializing web server...");
            _appWebServer.Init(_fileService.GetPreviewsPath(), _fileService.GetSubTitleFolder(), this, _castService, _webServerCancellationToken.Token);

            InitializeCastHandlers();
            InitializeOrUpdateFileWatcher(false);

            Logger.LogInformation($"{nameof(Initialize)}: Completed");
            await base.Initialize();
        }

        public override void SetCommands()
        {
            base.SetCommands();

            TogglePlaylistVisibilityCommand = new MvxCommand(() => IsExpanded = !IsExpanded);

            CloseAppCommand = new MvxAsyncCommand(HandleCloseApp);

            PreviousCommand = new MvxCommand(() => GoTo(false));

            NextCommand = new MvxCommand(() => GoTo(true));

            TogglePlayBackCommand = new MvxAsyncCommand(TogglePlayBack);

            StopPlayBackCommand = new MvxAsyncCommand(StopPlayBack);

            SkipCommand = new MvxAsyncCommand<int>(SkipSeconds);

            SwitchPlayListsCommand = new MvxCommand(() => SwitchPlayLists());

            AddNewPlayListCommand = new MvxAsyncCommand(AddNewPlayList);

            DeletePlayListCommand = new MvxCommand<PlayListItemViewModel>(pl => _beforeDeletingPlayList.Raise(pl));

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

            OpenSubTitleFileDialogCommand = new MvxCommand(() => _openSubTitleFileDialog.Raise());

            SetSubTitlesCommand = new MvxAsyncCommand<string>(OnSubTitleFileSelected);

            SetVolumeCommand = new MvxAsyncCommand(async () => VolumeLevel = await _castService.SetVolume(VolumeLevel));

            ToggleMuteCommand = new MvxAsyncCommand(async () => IsMuted = await _castService.SetIsMuted(!IsMuted));
        }

        public override void RegisterMessages()
        {
            base.RegisterMessages();
            SubscriptionTokens.AddRange(new List<MvxSubscriptionToken>
            {
                Messenger.Subscribe<PlayFileMessage>(async(msg) => await PlayFile(msg.File, msg.Force)),
                Messenger.Subscribe<ManualDisconnectMessage>(_ => OnStoppedPlayBack()),
                Messenger.Subscribe<LoopFileMessage>(msg => DisableLoopForAllFiles(msg.File.Id)),
                Messenger.Subscribe<SnackbarMessage>(async msg => await ShowSnackbarMsg(msg.Message)),
                Messenger.Subscribe<IsBusyMessage>(msg => IsBusy = msg.IsBusy)
            });
        }

        public override void ViewAppeared()
        {
            base.ViewAppeared();
            Logger.LogInformation($"{nameof(ViewAppeared)}: Creating the file duration task..");
            DurationTaskNotifier = MvxNotifyTask.Create(SetFileDurations());
            string path = _fileService.GetFFmpegPath();
            if (_fileService.Exists(path))
                return;

            Logger.LogInformation($"{nameof(ViewAppeared)}: FFmpeg is not in user folder, showing download dialog...");
            ShowDownloadDialogCommand.Execute();
        }

        public void SaveChangesBeforeClosing(
            double width,
            double height,
            Dictionary<PlayListItemViewModel, int> positions)
        {
            _settingsService.WindowWidth = width;
            _settingsService.WindowHeight = height;
            _settingsService.IsPlayListExpanded = IsExpanded;
            _settingsService.SaveSettings();

            var files = PlayLists.SelectMany(pl => pl.Items)
                .Where(f => f.WasPlayed || f.PositionChanged)
                .ToList();
            _playListsService.SaveChangesBeforeClosingApp(positions, files);
            _playListsService.Close();
        }

        public long TrySetThumbnail(double sliderWidth, double mouseX)
        {
            if (_currentlyPlayedFile == null)
            {
                PreviewThumbnailImg = null;
                return -1;
            }

            long tentativeSecond = GetMainProgressBarSeconds(sliderWidth, mouseX);

            if (_fileService.IsMusicFile(_currentlyPlayedFile.Path) || _fileService.IsUrlFile(_currentlyPlayedFile.Path))
            {
                PreviewThumbnailImg = CurrentFileThumbnail;
                return tentativeSecond;
            }
            PreviewThumbnailImg = _fileService.GetClosestThumbnail(_currentlyPlayedFile.Path, tentativeSecond);
            return tentativeSecond;
        }

        public long GetMainProgressBarSeconds(double sliderWidth, double mouseX)
            => Convert.ToInt64(mouseX * _currentlyPlayedFile.TotalSeconds / sliderWidth);

        private async Task GoToSeconds(long seconds)
        {
            Logger.LogInformation($"{nameof(GoToSeconds)}: Trying to go to seconds = {seconds}");

            if (_currentlyPlayedFile == null)
            {
                Logger.LogWarning($"{nameof(GoToSeconds)}: Can't go to seconds = {seconds} because the current played file is null");
                return;
            }

            IsBusy = true;
            await _castService.GoToSeconds(
                CurrentFileVideoStreamIndex,
                CurrentFileAudioStreamIndex,
                CurrentFileSubTitleStreamIndex,
                CurrentFileQuality,
                seconds,
                _currentlyPlayedFile.FileInfo);
            IsBusy = false;
        }

        private async Task SkipSeconds(int seconds)
        {
            Logger.LogInformation($"{nameof(SkipSeconds)}: Trying to skip {seconds} seconds");
            if (_currentlyPlayedFile == null)
            {
                Logger.LogWarning($"{nameof(SkipSeconds)}: Can't go skip seconds = {seconds} because the current played file is null");
                return;
            }

            IsBusy = true;
            await _castService.AddSeconds(
                CurrentFileVideoStreamIndex,
                CurrentFileAudioStreamIndex,
                CurrentFileSubTitleStreamIndex,
                CurrentFileQuality,
                seconds,
                _currentlyPlayedFile.FileInfo);
            IsBusy = false;
        }

        private async Task SetFileDurations()
        {
            Logger.LogInformation($"{nameof(SetFileDurations)}: Setting file duration to all the files");
            var tasks = PlayLists.Select(pl => pl.SetFilesInfo(_setDurationTokenSource.Token)).ToList();

            await Task.WhenAll(tasks).ConfigureAwait(false);
            Logger.LogInformation($"{nameof(SetFileDurations)}: File duration was set to all the files");
        }

        private async Task AddNewPlayList()
        {
            var vm = Mvx.IoCProvider.Resolve<PlayListItemViewModel>();
            vm.Name = $"New PlayList {PlayLists.Count}";
            vm.Position = PlayLists.Any()
                ? PlayLists.Max(pl => pl.Position) + 1
                : 1;

            var playList = await _playListsService.AddNewPlayList(vm.Name, vm.Position);
            vm.Id = playList.Id;

            PlayLists.Add(vm);
            SelectedPlayListIndex = PlayLists.Count - 1;

            _appWebServer.OnPlayListAdded?.Invoke(vm.Id);
        }

        public async Task DeletePlayList(int logicalIndex, PlayListItemViewModel playlist)
        {
            if (playlist == null)
                return;

            long index = PlayLists.IndexOf(playlist);
            long id = playlist.Id;
            //Remember that if you move the tabs, the SelectedPlayListIndex is not updated
            if (index == SelectedPlayListIndex)
                SwitchPlayLists(false, logicalIndex);
            await _playListsService.DeletePlayList(id);
            playlist.CleanUp();
            PlayLists.Remove(playlist);
            _appWebServer.OnPlayListDeleted?.Invoke(id);
            InitializeOrUpdateFileWatcher(true);
        }

        private async Task DeleteAllPlayLists(PlayListItemViewModel except)
        {
            if (PlayLists.Count <= 1)
                return;
            var exceptIndex = PlayLists.IndexOf(except);

            var items = PlayLists.Where((t, i) => i != exceptIndex).ToList();

            var ids = items.Select(pl => pl.Id).ToList();
            await _playListsService.DeletePlayLists(ids);
            foreach (var playlist in items)
            {
                playlist.CleanUp();
            }

            PlayLists.RemoveItems(items);
            foreach (var id in ids)
            {
                _appWebServer.OnPlayListDeleted?.Invoke(id);
            }

            InitializeOrUpdateFileWatcher(true);
        }

        private async Task HandleCloseApp()
        {
            Logger.LogInformation($"{nameof(HandleCloseApp)} App is about to be closed, cleaning them all!");
            _fileWatcherService.StopListening();
            _appWebServer.OnAppClosing?.Invoke();
            _setDurationTokenSource.Cancel();
            foreach (var playList in PlayLists)
            {
                playList.CleanUp();
            }
            await StopPlayBack();
            _castService.CleanThemAll();
            _webServerCancellationToken.Cancel();
            _currentlyPlayedFile?.CleanUp();
            RemoveCastHandlers();
            _closeApp.Raise();
        }

        private void SwitchPlayLists(bool forward = true, int? playlistIndex = null)
        {
            int increment = forward ? 1 : -1;
            int tentativeIndex = SelectedPlayListIndex + increment;
            if (playlistIndex.HasValue)
            {
                tentativeIndex = playlistIndex.Value + increment;
            }
            SelectedPlayListIndex = PlayLists.ElementAtOrDefault(tentativeIndex) != null ? tentativeIndex : 0;
        }

        private void GoTo(bool nextTrack, bool isAnAutomaticCall = false)
        {
            if (_currentlyPlayedFile == null || _onSkipOrPrevious)
                return;

            var playlist = PlayLists.FirstOrDefault(p => p.Id == _currentlyPlayedFile.PlayListId);
            if (playlist is null)
            {
                Logger.LogInformation($"{nameof(GoTo)}: PlaylistId = {_currentlyPlayedFile.PlayListId} does not exist. It may have been deleted. Playback will stop now");
                StopPlayBackCommand.Execute();
                return;
            }

            _onSkipOrPrevious = true;
            Logger.LogInformation($"{nameof(GoTo)}: Getting the next / previous file to play.... Going to next file = {nextTrack}");
            int increment = nextTrack ? 1 : -1;
            var fileIndex = playlist.Items.IndexOf(_currentlyPlayedFile);
            int newIndex = fileIndex + increment;
            bool random = playlist.Shuffle && playlist.Items.Count > 1;
            if (random)
                Logger.LogInformation($"{nameof(GoTo)}: Random is active for playListId = {playlist.Id}, picking a random file ...");

            if (!isAnAutomaticCall && !random && playlist.Items.Count > 1 && playlist.Items.ElementAtOrDefault(newIndex) == null)
            {
                Logger.LogInformation($"{nameof(GoTo)}: The new index = {newIndex} does not exist in the playlist, falling back to the first or last item in the list");
                var nextPreviousFile = nextTrack ? playlist.Items.First() : playlist.Items.Last();
                nextPreviousFile.PlayCommand.Execute();
                return;
            }

            if (fileIndex < 0)
            {
                Logger.LogInformation(
                    $"{nameof(GoTo)}: File = {_currentlyPlayedFile.Path} is no longer present in the playlist, " +
                    "it may have been deleted, getting the closest one...");
                int nextPosition = _currentlyPlayedFile.Position + increment;
                int closestPosition = playlist.Items
                    .Select(f => f.Position)
                    .GetClosest(nextPosition);

                var closestFile = playlist.Items.FirstOrDefault(f => f.Position == closestPosition);

                Logger.LogInformation($"{nameof(GoTo)}: Closest file is = {closestFile?.Path}, trying to play it");
                if (closestFile != _currentlyPlayedFile)
                    closestFile?.PlayCommand?.Execute();
                return;
            }

            var file = random
                ? playlist.Items.PickRandom(fileIndex)
                : playlist.Items.ElementAtOrDefault(newIndex);

            if (file is null)
            {
                Logger.LogInformation(
                    $"{nameof(GoTo)}: File at index = {fileIndex} in playListId {playlist.Id} was not found. " +
                    "Probably an end of playlist");

                if (!playlist.Loop)
                {
                    Logger.LogInformation($"{nameof(GoTo)}: Since no file was found and playlist is not marked to loop, the playback of this playlist will end here");
                    StopPlayBackCommand.Execute();
                    return;
                }
                Logger.LogInformation($"{nameof(GoTo)}: Looping playlistId = {playlist.Id}");
                playlist.Items.FirstOrDefault()?.PlayCommand?.Execute();
                return;
            }

            Logger.LogInformation(
                $"{nameof(GoTo)}: The next file to play is = {file.Path} and it's index is = {newIndex} " +
                $"compared to the old one = {fileIndex}....");
            file.PlayCommand.Execute();
        }

        private async Task TogglePlayBack()
        {
            if (IsCurrentlyPlaying)
            {
                await _castService.TogglePlayback();
                IsPaused = !IsPaused;
            }
        }

        private async Task<bool> PlayFile(FileItemViewModel file, bool force, bool fileOptionsChanged = false)
        {
            if (file == null)
            {
                Logger.LogWarning($"{nameof(PlayFile)}: Cant play file, it is null !!!");
                return false;
            }

            DisableLoopForAllFiles(file.Id);
            if (!file.Exists)
            {
                Logger.LogInformation($"{nameof(PlayFile)}: Cant play file = {file.Path}. It doesnt exist");
                await ShowSnackbarMsg(GetText("FileDoesntExist"));
                return false;
            }

            if (file == _currentlyPlayedFile && !force && !fileOptionsChanged && !file.Loop)
            {
                await ShowSnackbarMsg(GetText("FileIsAlreadyBeingPlayed"));
                return false;
            }

            if (string.IsNullOrEmpty(file.Duration))
            {
                Logger.LogInformation(
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
            IsPaused = false;

            var playList = PlayLists.FirstOrDefault(pl => pl.Id == file.PlayListId);
            if (playList == null)
            {
                await ShowSnackbarMsg(GetText("PlayListDoesntExist"));
                return false;
            }
            playList.SelectedItem = file;

            _currentlyPlayedFile?.CleanUp();
            _currentlyPlayedFile = file;
            _appWebServer.OnFileLoading?.Invoke();

            Logger.LogInformation($"{nameof(PlayFile)}: Updating file info for file = {file.Filename}");
            await playList.SetFileInfo(file.Id, _setDurationTokenSource.Token);

            Logger.LogInformation($"{nameof(PlayFile)}: Trying to play file = {file.Filename}");

            SetCurrentlyPlayingInfo(file.Filename, true);
            if (!fileOptionsChanged)
                await SetAvailableAudiosAndSubTitles();

            try
            {
                if (file.CanStartPlayingFromCurrentPercentage &&
                    !file.IsUrlFile &&
                    !force &&
                    !_settingsService.StartFilesFromTheStart)
                {
                    Logger.LogInformation(
                        $"{nameof(PlayFile)}: File will be resumed from = {file.PlayedPercentage} %");
                    await _castService.GoToPosition(
                        file.Path,
                        CurrentFileVideoStreamIndex,
                        CurrentFileAudioStreamIndex,
                        CurrentFileSubTitleStreamIndex,
                        CurrentFileQuality,
                        file.PlayedPercentage,
                        file.TotalSeconds,
                        file.FileInfo);
                }
                else
                {
                    Logger.LogInformation($"{nameof(PlayFile)}: Playing file from the start");
                    await _castService.StartPlay(
                        file.Path,
                        CurrentFileVideoStreamIndex,
                        CurrentFileAudioStreamIndex,
                        CurrentFileSubTitleStreamIndex,
                        CurrentFileQuality,
                        file.FileInfo);
                }

                _currentlyPlayedFile.ListenEvents();
                _castService.GenerateThumbnails(file.Path);

                Logger.LogInformation($"{nameof(PlayFile)}: File is being played...");

                return true;
            }
            catch (Exception e)
            {
                await HandlePlayException(playList, e);
                return false;
            }
            finally
            {
                IsBusy = false;
                _onSkipOrPrevious = false;
            }
        }

        private async Task HandlePlayException(PlayListItemViewModel playList, Exception e)
        {
            var msg = GetText("CouldntPlayFile");
            switch (e)
            {
                case NotSupportedException _:
                    msg = GetText("FileNotSupported");
                    break;
                case NoDevicesException _:
                    msg = GetText("NoDevicesWereFound");
                    break;
                case ConnectingException _:
                    msg = GetText("ConnectionInProgress");
                    break;
                default:
                    Logger.LogError(e, $"{nameof(HandlePlayException)}: Unknown error occurred");
                    _telemetryService.TrackError(e);
                    break;
            }
            playList.SelectedItem = null;
            _appWebServer.OnFileLoadingError?.Invoke(msg);
            await StopPlayBack();
            await ShowSnackbarMsg(msg);
        }

        private async Task StopPlayBack()
        {
            if (IsCurrentlyPlaying)
            {
                await _castService.StopPlayback();
            }
            OnStoppedPlayBack();
        }

        private void OnStoppedPlayBack()
        {
            _currentlyPlayedFile?.CleanUp();
            _currentlyPlayedFile = null;
            _appWebServer.OnEndReached?.Invoke();
            _castService.StopRunningProcess();
            SetCurrentlyPlayingInfo(null, false);
            IsPaused = false;
            DisableLoopForAllFiles();
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

        private async Task ShowSnackbarMsg(string msg, string actionContent = null)
        {
            if (ShowSnackbar)
                return;

            if (string.IsNullOrEmpty(actionContent))
            {
                actionContent = GetText("Dismiss");
            }
            _appWebServer.OnServerMsg?.Invoke(msg);
            ShowSnackbar = true;
            SnackbarMsg = msg;
            SnackBarActionMsg = actionContent;
            //The snacbarmsgqueue has a problem that sometimes
            //it doesnt show the msg, thats why we use this implementation
            await Task.Delay(2000);

            SnackbarActionCommand.Execute();
        }

        private async Task SetAvailableAudiosAndSubTitles()
        {
            Logger.LogInformation($"{nameof(SetAvailableAudiosAndSubTitles)}: Cleaning current file videos, audios and subs streams");
            CurrentFileVideos.Clear();
            CurrentFileAudios.Clear();
            CurrentFileSubTitles.Clear();
            CurrentFileQualities.Clear();

            if (_currentlyPlayedFile == null)
            {
                Logger.LogWarning($"{nameof(SetAvailableAudiosAndSubTitles)}: Current file is null");
                return;
            }

            if (_currentlyPlayedFile.FileInfo == null)
            {
                await _currentlyPlayedFile.SetFileInfo(_setDurationTokenSource.Token);
                if (_currentlyPlayedFile.FileInfo == null)
                {
                    Logger.LogWarning($"{nameof(SetAvailableAudiosAndSubTitles)}: Current file = {_currentlyPlayedFile.Path} doesnt have a fileinfo");
                    return;
                }
            }

            Logger.LogInformation($"{nameof(SetAvailableAudiosAndSubTitles)}: Setting available file videos, audios and subs streams");

            //Videos
            bool isSelected = true;
            bool isEnabled = _currentlyPlayedFile.FileInfo.Videos.Count > 1;
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

            //Audios
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

            //Subtitles
            if (!_fileService.IsVideoFile(_currentlyPlayedFile.Path))
                return;

            var (localSubsPath, filename) = TryGetSubTitlesLocalPath();
            bool localSubExists = !string.IsNullOrEmpty(localSubsPath);
            isEnabled = _currentlyPlayedFile.FileInfo.SubTitles.Count > 1 || localSubExists;
            CurrentFileSubTitles.Add(new FileItemOptionsViewModel
            {
                Id = NoStreamSelectedId,
                IsSubTitle = true,
                IsSelected = !localSubExists && _currentlyPlayedFile.FileInfo.SubTitles.Count == 0 || !_settingsService.LoadFirstSubtitleFoundAutomatically,
                IsEnabled = isEnabled,
                Text = GetText("None")
            });

            if (localSubExists)
            {
                CurrentFileSubTitles.Add(new FileItemOptionsViewModel
                {
                    Id = NoStreamSelectedId - 1,
                    IsSubTitle = true,
                    IsSelected = true,
                    IsEnabled = true,
                    Text = filename,
                    Path = localSubsPath
                });
            }

            isEnabled = _currentlyPlayedFile.FileInfo.SubTitles.Count > 0;
            isSelected = !localSubExists && _settingsService.LoadFirstSubtitleFoundAutomatically;
            foreach (var subtitle in _currentlyPlayedFile.FileInfo.SubTitles)
            {
                CurrentFileSubTitles.Add(new FileItemOptionsViewModel
                {
                    Id = subtitle.Index,
                    IsEnabled = isEnabled,
                    IsSubTitle = true,
                    IsSelected = isSelected,
                    Text = subtitle.SubTitleText
                });

                isSelected = false;
            }
        }

        private (string, string) TryGetSubTitlesLocalPath()
        {
            Logger.LogInformation($"{nameof(TryGetSubTitlesLocalPath)}: Checking if subtitle exist in the same dir as file = {_currentlyPlayedFile.Path}");
            var (possibleSubTitlePath, filename) = _fileService.TryGetSubTitlesLocalPath(_currentlyPlayedFile.Path);
            if (!string.IsNullOrWhiteSpace(possibleSubTitlePath))
            {
                Logger.LogInformation($"{nameof(TryGetSubTitlesLocalPath)}: Found subtitles in path = {possibleSubTitlePath}");
                return (possibleSubTitlePath, filename);
            }

            Logger.LogInformation($"{nameof(TryGetSubTitlesLocalPath)}: No subtitles were found for file = {_currentlyPlayedFile.Path}");
            return (possibleSubTitlePath, filename);
        }

        private Task FileOptionsChanged(FileItemOptionsViewModel selectedItem)
        {
            if (selectedItem == null)
            {
                Logger.LogWarning($"{nameof(FileOptionsChanged)}: Selected option is null");
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

            Logger.LogInformation($"{nameof(FileOptionsChanged)}: StreamId = {selectedItem.Id}  was selected. Text = {selectedItem.Text}");
            selectedItem.IsSelected = true;

            return PlayFile(_currentlyPlayedFile, false, true);
        }

        private Task OnSubTitleFileSelected(string filePath)
        {
            var (isSub, filename) = _fileService.IsSubtitle(filePath);
            if (!isSub || CurrentFileSubTitles.Any(f => f.Text == filename))
            {
                Logger.LogInformation($"{nameof(OnSubTitleFileSelected)}: Subtitle = {filePath} is not valid or is already in the current sub files");
                return Task.CompletedTask;
            }

            foreach (var item in CurrentFileSubTitles)
            {
                if (item.Id == NoStreamSelectedId)
                    item.IsEnabled = true;
                item.IsSelected = false;
            }

            CurrentFileSubTitles.Add(new FileItemOptionsViewModel
            {
                Id = CurrentFileSubTitles.Min(f => f.Id) - 1,
                IsSubTitle = true,
                IsSelected = true,
                Text = filename,
                Path = filePath,
                IsEnabled = true
            });

            return PlayFile(_currentlyPlayedFile, false, true);
        }

        private void DisableLoopForAllFiles(long exceptFileId = -1)
        {
            var files = PlayLists.SelectMany(pl => pl.Items).Where(f => f.Loop && f.Id != exceptFileId).ToList();

            foreach (var file in files)
                file.Loop = false;
        }
        #endregion
    }
}
