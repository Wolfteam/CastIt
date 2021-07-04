using AutoMapper;
using CastIt.Application.Interfaces;
using CastIt.Application.Server;
using CastIt.Domain.Dtos.Responses;
using CastIt.Domain.Enums;
using CastIt.Domain.Extensions;
using CastIt.Interfaces;
using CastIt.Models.Messages;
using CastIt.ViewModels.Dialogs;
using CastIt.ViewModels.Items;
using Microsoft.Extensions.Logging;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CastIt.ViewModels
{
    public partial class MainViewModel : BaseViewModel
    {
        #region Members

        private readonly IDesktopAppSettingsService _settingsService;
        private readonly IMvxNavigationService _navigationService;
        private readonly ITelemetryService _telemetryService;
        private readonly IMapper _mapper;
        private readonly IFileService _fileService;
        private readonly ICastItHubClientService _castItHub;

        private bool _isPaused;
        private bool _isCurrentlyPlaying;
        private string _currentlyPlayingFilename;

        private bool _isExpanded = true;

        //-1 to make sure that no tab is selected
        private int _selectedPlayListIndex = -1;
        private double _playedPercentage;
        private double _currentFileDuration;
        private bool _showSettingsPopUp;
        private bool _showDevicesPopUp;
        private string _elapsedTimeString;
        private string _currentFileThumbnail;
        private double _currentPlayedSeconds;
        private string _previewThumbnailImg;
        private bool _showSnackbar;
        private string _snackbarMsg;
        private string _snackBarActionMsg;
        private bool _isBusy;
        private double _volumeLevel;
        private bool _isMuted;
        private bool _serverIsRunning;

        private bool _updatingPlayerStatus;

        private MvxNotifyTask _initializeCastServerTask;

        private readonly MvxInteraction _closeApp = new MvxInteraction();
        private readonly MvxInteraction _openSubTitleFileDialog = new MvxInteraction();

        private readonly MvxInteraction<PlayListItemViewModel> _beforeDeletingPlayList =
            new MvxInteraction<PlayListItemViewModel>();

        private List<FileThumbnailRangeResponseDto> _thumbnailRanges = new List<FileThumbnailRangeResponseDto>();

        #endregion

        #region Properties

        public FileItemViewModel CurrentPlayedFile { get; private set; }

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
            set => this.RaiseAndSetIfChanged(ref _isCurrentlyPlaying, value);
        }

        public string CurrentlyPlayingFilename
        {
            get => _currentlyPlayingFilename;
            set => this.RaiseAndSetIfChanged(ref _currentlyPlayingFilename, value);
        }

        public int SelectedPlayListIndex
        {
            get => _selectedPlayListIndex;
            set
            {
                bool changed = _selectedPlayListIndex != value;
                this.RaiseAndSetIfChanged(ref _selectedPlayListIndex, value);
                if (!changed || _settingsService.UseGridViewForPlayLists)
                    return;
                //Only load the items if a change happened and we are not using the gridview
                for (int i = 0; i < PlayLists.Count; i++)
                {
                    var playList = PlayLists[i];
                    playList.ClosePlayList();
                    if (i == value)
                    {
                        playList.LoadFileItems();
                    }
                }
            }
        }

        //TODO: IS THIS BEING USED ?
        public double PlayedPercentage
        {
            get => _playedPercentage;
            set => this.RaiseAndSetIfChanged(ref _playedPercentage, value);
        }

        public double CurrentFileDuration
        {
            get => _currentFileDuration;
            set => this.RaiseAndSetIfChanged(ref _currentFileDuration, value);
        }

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

        public MvxNotifyTask InitializeCastServerTask
        {
            get => _initializeCastServerTask;
            private set => SetProperty(ref _initializeCastServerTask, value);
        }

        public bool ServerIsRunning
        {
            get => _serverIsRunning;
            set => SetProperty(ref _serverIsRunning, value);
        }

        #endregion

        #region Commands

        public IMvxCommand TogglePlaylistVisibilityCommand { get; private set; }
        public IMvxAsyncCommand CloseAppCommand { get; private set; }
        public IMvxAsyncCommand PreviousCommand { get; private set; }
        public IMvxAsyncCommand NextCommand { get; private set; }
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
        public IMvxAsyncCommand GoBackCommand { get; private set; }
        public IMvxAsyncCommand ShowChangeServerUrlDialogCommand { get; private set; }

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
            IDesktopAppSettingsService settingsService,
            IMvxNavigationService navigationService,
            ITelemetryService telemetryService,
            IMapper mapper,
            IFileService fileService,
            ICastItHubClientService castItHub)
            : base(textProvider, messenger, logger)
        {
            _settingsService = settingsService;
            _navigationService = navigationService;
            _telemetryService = telemetryService;
            _mapper = mapper;
            _fileService = fileService;
            _castItHub = castItHub;
        }

        #region Methods

        public override Task Initialize()
        {
            CleanProgressBar();
            IsBusy = true;
            IsExpanded = _settingsService.IsPlayListExpanded;

            InitializeCastServerTask = MvxNotifyTask.Create(InitializeCastServer);

            return base.Initialize();
        }

        public override void SetCommands()
        {
            base.SetCommands();

            TogglePlaylistVisibilityCommand = new MvxCommand(() => IsExpanded = !IsExpanded);

            CloseAppCommand = new MvxAsyncCommand(HandleCloseApp);

            PreviousCommand = new MvxAsyncCommand(GoToPrevious);

            NextCommand = new MvxAsyncCommand(GoToNext);

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

            ShowDownloadDialogCommand = new MvxAsyncCommand(ShowDownloadFfmpegDialog);

            FileOptionsChangedCommand = new MvxAsyncCommand<FileItemOptionsViewModel>(FileOptionsChanged);

            OpenSubTitleFileDialogCommand = new MvxCommand(() => _openSubTitleFileDialog.Raise());

            SetSubTitlesCommand = new MvxAsyncCommand<string>(SetFileSubtitlesFromPath);

            SetVolumeCommand = new MvxAsyncCommand(async () => await ChangeVolumeLevel(VolumeLevel, IsMuted));

            ToggleMuteCommand = new MvxAsyncCommand(async () => await ChangeVolumeLevel(VolumeLevel, !IsMuted));

            GoBackCommand = new MvxAsyncCommand(GoToPlayLists);

            ShowChangeServerUrlDialogCommand = new MvxAsyncCommand(ShowChangeServerUrlDialog);
        }

        public override void RegisterMessages()
        {
            base.RegisterMessages();
            SubscriptionTokens.AddRange(new List<MvxSubscriptionToken>
            {
                Messenger.Subscribe<PlayFileMessage>(async (msg) => await PlayFile(msg.File, msg.Force)),
                Messenger.Subscribe<ManualDisconnectMessage>(_ => OnStoppedPlayBack()),
                Messenger.Subscribe<SnackbarMessage>(async msg => await ShowSnackbarMsg(msg.Message)),
                Messenger.Subscribe<IsBusyMessage>(msg => IsBusy = msg.IsBusy),
                Messenger.Subscribe<UseGridViewMessage>(async (_) => await GoToPlayLists()),
                Messenger.Subscribe<ShowDownloadFFmpegDialogMessage>(async _ => await ShowDownloadDialogCommand.ExecuteAsync()),
            });
        }

        public override async void ViewAppeared()
        {
            base.ViewAppeared();
            await GoToPlayLists();
        }

        private async Task InitializeCastServer()
        {
            _castItHub.OnClientConnected += CastItHubOnOnClientConnected;

            _castItHub.OnClientDisconnected += CastItHubOnOnClientDisconnected;

            _castItHub.OnServerMessage += ShowSnackbarMessage;

            _castItHub.OnPlayerStatusChanged += OnPlayerStatusChanged;

            _castItHub.OnPlayListsLoaded += OnPlayListsLoaded;

            _castItHub.OnPlayListAdded += OnPlayListAdded;

            _castItHub.OnPlayListChanged += OnPlayListChanged;

            _castItHub.OnPlayListsChanged += OnPlayListsChanged;

            _castItHub.OnPlayListDeleted += OnPlayListDeleted;

            _castItHub.OnPlayListBusy += OnPlayListBusy;

            _castItHub.OnFileAdded += OnFileAdded;

            _castItHub.OnFileChanged += OnFileChanged;

            _castItHub.OnFilesChanged += OnFilesChanged;

            _castItHub.OnFileDeleted += OnFileDeleted;

            _castItHub.OnFileLoading += OnFileLoading;

            _castItHub.OnFileLoaded += OnFileLoaded;

            _castItHub.OnFileEndReached += OnFileEndReached;

            Logger.LogInformation($"{nameof(InitializeCastServer)}: Initializing cast service...");
            bool initialized = await _castItHub.Init().ConfigureAwait(false);
            if (!initialized)
            {
                Logger.LogInformation($"{nameof(Initialize)}: Couldn't connect to the hub");
                IsBusy = false;
                return;
            }

            ServerIsRunning = true;
            IsBusy = false;
            Logger.LogInformation($"{nameof(InitializeCastServer)}: Completed");
        }

        public async void MovePlayList(PlayListItemViewModel vm, PlayListItemViewModel target)
        {
            var currentIndex = PlayLists.IndexOf(vm);
            var newIndex = PlayLists.IndexOf(target);
            await MovePlayList(vm.Id, currentIndex, newIndex);
        }

        public async Task MovePlayList(int newIndex, PlayListItemViewModel vm)
        {
            var currentIndex = PlayLists.IndexOf(vm);
            await MovePlayList(vm.Id, currentIndex, newIndex);
        }

        public async Task MovePlayList(long id, int currentIndex, int newIndex)
        {
            bool move = currentIndex >= 0 && newIndex >= 0 && currentIndex != newIndex;
            if (move)
            {
                await _castItHub.UpdatePlayListPosition(id, newIndex);
            }
        }

        public async Task SaveChangesBeforeClosing(double width, double height)
        {
            _settingsService.WindowWidth = width;
            _settingsService.WindowHeight = height;
            _settingsService.IsPlayListExpanded = IsExpanded;
            await _settingsService.SaveSettings(_settingsService.Settings);
        }

        //TODO: WHEN A CHROMECAST DISCONNECT HAPPENS, THE UI IS NOT GETTING NOTIFIED
        public (int, int) GetPreviewThumbnailCoordinates(long tentativeSecond)
        {
            int centerX = 0;
            int centerY = 0;

            //some sanity checks
            if (!_thumbnailRanges.Any() || CurrentPlayedFile == null || !CurrentPlayedFile.Type.IsLocalVideo())
            {
                return (centerX, centerY);
            }

            var range = _thumbnailRanges.Find(r => r.ThumbnailRange.ContainsValue(tentativeSecond));
            var position = range.GetPositionBySecond(tentativeSecond);

            centerX = position.X * (int)AppWebServerConstants.ThumbnailImageWidth;
            centerY = position.Y * (int)AppWebServerConstants.ThumbnailImageHeight;

            return (centerX, centerY);
        }

        public void SetPreviewThumbnailImage(long tentativeSecond)
        {
            if (CurrentPlayedFile == null)
            {
                PreviewThumbnailImg = null;
                return;
            }

            if (!CurrentPlayedFile.Type.IsLocalVideo())
            {
                PreviewThumbnailImg = CurrentFileThumbnail;
                return;
            }

            if (!_thumbnailRanges.Any())
            {
                return;
            }

            //TODO: MOVE THE URL TO A COMMON PLACE
            //TODO: MAYBE YOU SHOULD DOWNLOAD THE YT IMAGE BEFORE PLAYING
            //PreviewThumbnailImg = $"{_currentPlayedFile.Path}|{tentativeSecond}";
            var range = _thumbnailRanges.Find(r => r.ThumbnailRange.ContainsValue(tentativeSecond));
            PreviewThumbnailImg = range.PreviewThumbnailUrl;
        }

        public long GetMainProgressBarSecondsForThumbnails(double sliderWidth, double mouseX)
        {
            if (CurrentPlayedFile == null)
            {
                return -1;
            }

            return GetMainProgressBarSeconds(sliderWidth, mouseX);
        }

        public long GetMainProgressBarSeconds(double sliderWidth, double mouseX)
            => Convert.ToInt64(mouseX * CurrentFileDuration / sliderWidth);

        private Task GoToSeconds(long seconds)
        {
            Logger.LogInformation($"{nameof(GoToSeconds)}: Trying to go to seconds = {seconds}");
            return _castItHub.GoToSeconds(seconds);
        }

        private Task SkipSeconds(int seconds)
        {
            Logger.LogInformation($"{nameof(SkipSeconds)}: Trying to skip {seconds} seconds");
            return _castItHub.SkipSeconds(seconds);
        }

        private Task ChangeVolumeLevel(double newValue, bool isMuted)
        {
            Logger.LogInformation(
                $"{nameof(ChangeVolumeLevel)}: Trying to set volume level to = {newValue} and is muted = {isMuted}");
            return _castItHub.SetVolume(newValue, isMuted);
        }

        private async Task AddNewPlayList()
        {
            Logger.LogInformation($"{nameof(AddNewPlayList)}: Creating a new playlist");
            await _castItHub.AddNewPlayList();
        }

        public async Task DeletePlayList(int logicalIndex, PlayListItemViewModel playlist)
        {
            if (playlist == null)
                return;
            long index = PlayLists.IndexOf(playlist);
            long id = playlist.Id;
            Logger.LogInformation($"{nameof(DeletePlayList)}: Deleting playListId = {id}");
            //Remember that if you move the tabs, the SelectedPlayListIndex is not updated
            if (index == SelectedPlayListIndex)
                SwitchPlayLists(false, logicalIndex);

            await _castItHub.DeletePlayList(id);
        }

        private async Task DeleteAllPlayLists(PlayListItemViewModel except)
        {
            await _castItHub.DeleteAllPlayLists(except?.Id ?? -1);
        }

        private async Task HandleCloseApp()
        {
            Logger.LogInformation($"{nameof(HandleCloseApp)} App is about to be closed, cleaning them all!");
            _castItHub.OnClientConnected -= CastItHubOnOnClientConnected;
            _castItHub.OnClientDisconnected -= CastItHubOnOnClientDisconnected;
            _castItHub.OnServerMessage -= ShowSnackbarMessage;
            _castItHub.OnPlayerStatusChanged -= OnPlayerStatusChanged;
            _castItHub.OnPlayListsLoaded -= OnPlayListsLoaded;
            _castItHub.OnPlayListAdded -= OnPlayListAdded;
            _castItHub.OnPlayListChanged -= OnPlayListChanged;
            _castItHub.OnPlayListsChanged -= OnPlayListsChanged;
            _castItHub.OnPlayListDeleted -= OnPlayListDeleted;
            _castItHub.OnPlayListBusy -= OnPlayListBusy;
            _castItHub.OnFileAdded -= OnFileAdded;
            _castItHub.OnFileChanged -= OnFileChanged;
            _castItHub.OnFilesChanged -= OnFilesChanged;
            _castItHub.OnFileDeleted -= OnFileDeleted;
            _castItHub.OnFileLoading -= OnFileLoading;
            _castItHub.OnFileLoaded -= OnFileLoaded;
            _castItHub.OnFileEndReached -= OnFileEndReached;

            await _castItHub.DisposeAsync();
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
            Logger.LogInformation(
                $"{nameof(SwitchPlayLists)}: Changing selected playlistIndex to = {SelectedPlayListIndex}");
        }

        private Task GoToNext()
        {
            Logger.LogInformation($"{nameof(GoToNext)}: Going to the next file in the playlist");
            return _castItHub.GoTo(true, false);
        }

        private Task GoToPrevious()
        {
            Logger.LogInformation($"{nameof(GoToNext)}: Going to the previous file in the playlist");
            return _castItHub.GoTo(false, true);
        }

        private Task TogglePlayBack()
        {
            Logger.LogInformation($"{nameof(TogglePlayBack)}: Toggling playback");
            return _castItHub.TogglePlayBack();
        }

        private async Task PlayFile(FileItemViewModel file, bool force, bool fileOptionsChanged = false)
        {
            if (file == null)
            {
                await ShowSnackbarMsg(AppMessageType.FileNotFound);
                return;
            }

            Logger.LogInformation(
                $"{nameof(PlayFile)}: Trying to play file = {file.Filename}. Force = {force} - FileOptionsChanged = {fileOptionsChanged}");
            CurrentPlayedFile = file;
            await _castItHub.Play(file.PlayListId, file.Id, force, fileOptionsChanged);
        }

        private async Task StopPlayBack()
        {
            Logger.LogInformation($"{nameof(StopPlayBack)}: Stopping playback");
            await _castItHub.StopPlayBack();
        }

        private void SetCurrentlyPlayingInfo(
            string filename,
            bool isPlaying,
            double playedPercentage = 0,
            double playedSeconds = 0)
        {
            if (!isPlaying)
            {
                _thumbnailRanges.Clear();
                CurrentFileThumbnail = null;
                ElapsedTimeString = string.Empty;
                CurrentFileVideos.Clear();
                CurrentFileAudios.Clear();
                CurrentFileQualities.Clear();
                CurrentFileSubTitles.Clear();
                CleanProgressBar();
            }

            CurrentlyPlayingFilename = filename;
            IsCurrentlyPlaying = isPlaying;
            PlayedPercentage = playedPercentage;
            CurrentPlayedSeconds = playedSeconds;
        }

        private async void ShowSnackbarMessage(AppMessageType type)
        {
            await ShowSnackbarMsg(type);
        }

        private async Task ShowSnackbarMsg(AppMessageType type)
        {
            Logger.LogInformation($"{nameof(ShowSnackbarMsg)}: Get server msg = {type}");
            var message = type switch
            {
                AppMessageType.InvalidRequest => GetText("InvalidRequest"),
                AppMessageType.NotFound => GetText("NotFound"),
                AppMessageType.PlayListNotFound => GetText("PlayListDoesntExist"),
                AppMessageType.UnknownErrorLoadingFile => GetText("UnknownErrorLoadingFile"),
                AppMessageType.FileNotFound => GetText("FileDoesntExist"),
                AppMessageType.FileIsAlreadyBeingPlayed => GetText("FileIsAlreadyBeingPlayed"),
                AppMessageType.FileNotSupported => GetText("FileNotSupported"),
                AppMessageType.FilesAreNotValid => GetText("FilesAreNotValid"),
                AppMessageType.NoFilesToBeAdded => GetText("NoFilesToBeAdded"),
                AppMessageType.UrlNotSupported => GetText("UrlNotSupported"),
                AppMessageType.UrlCouldntBeParsed => GetText("UrlCouldntBeParsed"),
                AppMessageType.OneOrMoreFilesAreNotReadyYet => GetText("OneOrMoreFilesAreNotReadyYet"),
                AppMessageType.NoDevicesFound => GetText("NoDevicesWereFound"),
                AppMessageType.NoInternetConnection => GetText("NoInternetConnection"),
                AppMessageType.ConnectionToDeviceIsStillInProgress => GetText("ConnectionInProgress"),
                AppMessageType.ServerIsClosing => GetText("ConnectionToServerLost"),
                _ => GetText("SomethingWentWrong"),
            };
            await ShowSnackbarMsg(message);
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

        private async Task FileOptionsChanged(FileItemOptionsViewModel selectedItem)
        {
            if (selectedItem == null)
            {
                Logger.LogWarning($"{nameof(FileOptionsChanged)}: Selected option is null");
                return;
            }

            Logger.LogInformation($"{nameof(FileOptionsChanged)}: Setting the fileOptions to id = {selectedItem.Id}");
            await _castItHub.SetFileOptions(
                selectedItem.Id, selectedItem.IsAudio,
                selectedItem.IsSubTitle, selectedItem.IsQuality);
        }

        private async Task SetFileSubtitlesFromPath(string filePath)
        {
            Logger.LogInformation($"{nameof(SetFileSubtitlesFromPath)}: Setting subtitles from path = {filePath}");
            await _castItHub.SetFileSubtitlesFromPath(filePath);
        }

        private async Task GoToPlayLists()
        {
            if (_settingsService.UseGridViewForPlayLists)
            {
                await _navigationService.Navigate<PlayListsGridViewModel>();
            }
            else
            {
                await _navigationService.Navigate<PlayListsViewModel>();
            }
        }

        public async Task GoToPlayList(PlayListItemViewModel vm)
        {
            SelectedPlayListIndex = PlayLists.IndexOf(vm);
            vm.LoadFileItems();
            await _navigationService.Navigate<PlayListGridItemViewModel, PlayListItemViewModel>(vm);
        }

        private void CleanProgressBar()
        {
            CurrentFileDuration = 1;
            CurrentPlayedSeconds = 0;
        }

        private void UpdateFileOptionsIfNeeded(MvxObservableCollection<FileItemOptionsViewModel> current, List<FileItemOptionsResponseDto> updated)
        {
            if (!current.Any())
            {
                var vms = updated.Select(o => _mapper.Map<FileItemOptionsViewModel>(o));
                current.ReplaceWith(vms);
                return;
            }

            foreach (var file in updated)
            {
                var existing = current.FirstOrDefault(o => o.Id == file.Id);
                if (existing == null)
                {
                    current.Add(_mapper.Map<FileItemOptionsViewModel>(file));
                }
                else
                {
                    existing.Text = file.Text;
                    existing.IsSelected = file.IsSelected;
                    existing.IsEnabled = file.IsEnabled;
                }
            }
        }

        private async Task ShowChangeServerUrlDialog()
        {
            ServerIsRunning = await _navigationService.Navigate<ChangeServerUrlDialogViewModel, bool>();
        }

        private async Task ShowDownloadFfmpegDialog()
        {
            bool filesWereDownloaded = await _navigationService.Navigate<DownloadDialogViewModel, bool>();
            if (!filesWereDownloaded)
            {
                CloseAppCommand.Execute();
            }
            else
            {
                await ShowSnackbarMsg(GetText("AppIsRdyToUse"));
            }
        }
        #endregion
    }
}
