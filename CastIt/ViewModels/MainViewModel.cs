using AutoMapper;
using CastIt.Common;
using CastIt.Common.Enums;
using CastIt.Common.Extensions;
using CastIt.Common.Utils;
using CastIt.Interfaces;
using CastIt.Interfaces.ViewModels;
using CastIt.Models.Messages;
using CastIt.Server.Dtos.Responses;
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
    public class MainViewModel : BaseViewModel, IMainViewModel
    {
        #region Members
        private const int NoStreamSelectedId = -1;
        private const int DefaultSelectedStreamId = 0;
        private const int DefaultQualitySelected = 360;

        private readonly ICastService _castService;
        private readonly IPlayListsService _playListsService;
        private readonly IAppSettingsService _settingsService;
        private readonly IMvxNavigationService _navigationService;
        private readonly ITelemetryService _telemetryService;
        private readonly IAppWebServer _appWebServer;
        private readonly IMapper _mapper;

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
        private readonly MvxInteraction<(double, double)> _setWindowWidthAndHeight = new MvxInteraction<(double, double)>();
        private readonly MvxInteraction _openSubTitleFileDialog = new MvxInteraction();
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
        public IMvxAsyncCommand<PlayListItemViewModel> DeletePlayListCommand { get; private set; }
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
        public IMvxInteraction<(double, double)> SetWindowWidthAndHeight
            => _setWindowWidthAndHeight;
        public IMvxInteraction OpenSubTitleFileDialog
            => _openSubTitleFileDialog;
        #endregion

        public MainViewModel(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            IMvxLogProvider logger,
            ICastService castService,
            IPlayListsService playListsService,
            IAppSettingsService settingsService,
            IMvxNavigationService navigationService,
            ITelemetryService telemetryService,
            IAppWebServer appWebServer,
            IMapper mapper)
            : base(textProvider, messenger, logger.GetLogFor<MainViewModel>())
        {
            _castService = castService;
            _playListsService = playListsService;
            _settingsService = settingsService;
            _navigationService = navigationService;
            _telemetryService = telemetryService;
            _appWebServer = appWebServer;
            _mapper = mapper;
        }

        #region Methods
        public override async Task Initialize()
        {
            IsExpanded = _settingsService.IsPlayListExpanded;
            Logger.Info($"{nameof(Initialize)}: Initializing cast service...");
            _castService.Init();
            _appWebServer.Init(this, _webServerCancellationToken.Token);

            Logger.Info($"{nameof(Initialize)}: Getting all playlists...");
            var playLists = await _playListsService.GetAllPlayLists();
            PlayLists.AddRange(playLists.OrderBy(pl => pl.Position));
            foreach (var playlist in playLists)
            {
                var files = await _playListsService.GetAllFiles(playlist.Id);
                playlist.Items.AddRange(files.OrderBy(f => f.Position));
            }

            foreach (var pl in PlayLists)
            {
                pl.SetPositionIfChanged();
            }

            Logger.Info($"{nameof(Initialize)}: Setting cast events..");
            _castService.OnFileLoaded += OnFileLoaded;
            _castService.OnTimeChanged += OnFileDurationChanged;
            _castService.OnPositionChanged += OnFilePositionChanged;
            _castService.OnEndReached += OnFileEndReached;
            _castService.QualitiesChanged += OnQualitiesChanged;
            _castService.OnPaused += OnPaused;
            _castService.OnDisconnected += OnDisconnected;
            _castService.GetSubTitles = () => CurrentFileSubTitles.FirstOrDefault(f => f.IsSelected)?.Path;
            _castService.OnVolumeChanged += OnVolumeChanged;

            Logger.Info($"{nameof(Initialize)}: Applying app theme and accent color...");
            WindowsUtils.ChangeTheme(_settingsService.AppTheme, _settingsService.AccentColor);

            Logger.Info($"{nameof(Initialize)}: Deleting old preview / log files...");
            try
            {
                FileUtils.DeleteFilesInDirectory(FileUtils.GetPreviewsPath(), DateTime.Now.AddDays(-1));
                FileUtils.DeleteFilesInDirectory(FileUtils.GetLogsPath(), DateTime.Now.AddDays(-3));
            }
            catch (Exception e)
            {
                Logger.Error(e, $"{nameof(Initialize)}: Error occurred while trying to delete previews");
                _telemetryService.TrackError(e);
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

            TogglePlayBackCommand = new MvxAsyncCommand(TogglePlayBack);

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
        }

        public long TrySetThumbnail(double sliderWidth, double mouseX)
        {
            if (_currentlyPlayedFile == null)
            {
                PreviewThumbnailImg = null;
                return -1;
            }

            long tentativeSecond = GetMainProgressBarSeconds(sliderWidth, mouseX);

            if (FileUtils.IsMusicFile(_currentlyPlayedFile.Path) || FileUtils.IsUrlFile(_currentlyPlayedFile.Path))
            {
                PreviewThumbnailImg = CurrentFileThumbnail;
                return tentativeSecond;
            }
            PreviewThumbnailImg = FileUtils.GetClosestThumbnail(_currentlyPlayedFile.Path, tentativeSecond);
            return tentativeSecond;
        }

        public long GetMainProgressBarSeconds(double sliderWidth, double mouseX)
            => Convert.ToInt64(mouseX * _currentlyPlayedFile.TotalSeconds / sliderWidth);

        #region Web Socket methods
        public List<GetAllPlayListResponseDto> GetAllPlayLists()
        {
            return PlayLists.Select(pl => new GetAllPlayListResponseDto
            {
                Id = pl.Id,
                Loop = pl.Loop,
                Name = pl.Name,
                NumberOfFiles = pl.Items.Count,
                Position = pl.Position,
                Shuffle = pl.Shuffle
            }).ToList();
        }

        public PlayListItemResponseDto GetPlayList(long playlistId)
        {
            var playlist = PlayLists.FirstOrDefault(pl => pl.Id == playlistId);
            if (playlist == null)
                return null;

            return new PlayListItemResponseDto
            {
                Id = playlist.Id,
                Loop = playlist.Loop,
                Name = playlist.Name,
                NumberOfFiles = playlist.Items.Count,
                Position = playlist.Position,
                Shuffle = playlist.Shuffle,
                Files = playlist.Items.Select(f => new FileItemResponseDto
                {
                    Extension = f.Extension,
                    Filename = f.Filename,
                    Exists = f.Exists,
                    Id = f.Id,
                    IsLocalFile = f.IsLocalFile,
                    IsUrlFile = f.IsUrlFile,
                    Path = f.Path,
                    PlayedPercentage = f.PlayedPercentage,
                    PlayListId = f.PlayListId,
                    Position = f.Position,
                    Size = f.Size,
                    IsBeingPlayed = f.IsBeingPlayed,
                    Loop = f.Loop,
                    TotalSeconds = f.TotalSeconds
                }).ToList()
            };
        }

        public FileLoadedResponseDto GetCurrentFileLoaded()
        {
            if (_currentlyPlayedFile == null)
                return null;

            var response = new FileLoadedResponseDto
            {
                Id = _currentlyPlayedFile.Id,
                Duration = _currentlyPlayedFile.TotalSeconds,
                Filename = _currentlyPlayedFile.Filename,
                LoopFile = _currentlyPlayedFile.Loop,
                CurrentSeconds = CurrentPlayedSeconds,
                IsPaused = IsPaused,
                IsMuted = IsMuted,
                VolumeLevel = VolumeLevel,
                ThumbnailUrl = CurrentFileThumbnail
            };

            var playlist = PlayLists.FirstOrDefault(pl => pl.Id == _currentlyPlayedFile.PlayListId);
            response.PlayListId = playlist?.Id ?? 0;
            response.PlayListName = playlist?.Name ?? "N/A";
            response.LoopPlayList = playlist?.Loop ?? false;
            response.ShufflePlayList = playlist?.Shuffle ?? false;
            return response;
        }

        public Task PlayFile(long id, long playlistId)
        {
            var pl = PlayLists.FirstOrDefault(pl => pl.Id == playlistId);
            if (pl == null)
            {
                Logger.Warn($"{nameof(PlayFile)}: Couldnt play fileId = {id} because playlistId = {playlistId} doesnt exists");
                _appWebServer.OnServerMsg?.Invoke(GetText("PlayListDoesntExist"));
                return Task.CompletedTask;
            }

            var file = pl.Items.FirstOrDefault(f => f.Id == id);
            if (file == null)
            {
                Logger.Warn($"{nameof(PlayFile)}: Couldnt play fileId = {id} because it doesnt exists");
                _appWebServer.OnServerMsg?.Invoke(GetText("FileDoesntExist"));
                return Task.CompletedTask;
            }

            return PlayFile(file, true);
        }

        public void SetPlayListOptions(long id, bool loop, bool shuffle)
        {
            var pl = PlayLists.FirstOrDefault(pl => pl.Id == id);
            if (pl == null)
            {
                Logger.Warn($"{nameof(SetPlayListOptions)}: PlaylistId = {id} doesnt exists");
                _appWebServer.OnServerMsg?.Invoke(GetText("PlayListDoesntExist"));
                return;
            }

            pl.Loop = loop;
            pl.Shuffle = shuffle;
        }

        public Task DeletePlayList(long id)
        {
            var pl = PlayLists.FirstOrDefault(pl => pl.Id == id);
            if (pl == null)
            {
                Logger.Warn($"{nameof(DeletePlayList)}: Cant delete playlistId = {id} because it doesnt exists");
                return ShowSnackbarMsg(GetText("PlayListDoesntExist"));
            }
            return DeletePlayList(pl);
        }

        public Task DeleteFile(long id, long playListId)
        {
            var pl = PlayLists.FirstOrDefault(pl => pl.Id == playListId);
            if (pl == null)
            {
                Logger.Warn($"{nameof(DeleteFile)}: Couldnt delete fileId = {id} because playlistId = {playListId} doesnt exists");
                return ShowSnackbarMsg(GetText("PlayListDoesntExist"));
            }
            return pl.RemoveFile(id);
        }

        public Task SetFileLoop(long id, long playlistId, bool loop)
        {
            var pl = PlayLists.FirstOrDefault(pl => pl.Id == playlistId);
            if (pl == null)
            {
                Logger.Warn($"{nameof(SetFileLoop)}: Couldnt update fileId = {id} because playlistId = {playlistId} doesnt exists");
                return ShowSnackbarMsg(GetText("PlayListDoesntExist"));
            }


            var file = pl.Items.FirstOrDefault(f => f.Id == id);
            if (file == null)
            {
                Logger.Warn($"{nameof(SetFileLoop)}: Couldnt update fileId = {id} because it doesnt exists");
                return ShowSnackbarMsg(GetText("FileDoesntExist"));
            }

            file.Loop = loop;
            return Task.CompletedTask;
        }

        public List<FileItemOptionsResponseDto> GetFileOptions(long id)
        {
            var fileOptions = new List<FileItemOptionsResponseDto>();
            if (_currentlyPlayedFile == null || _currentlyPlayedFile.Id != id)
                return fileOptions;

            fileOptions.AddRange(_mapper.Map<List<FileItemOptionsResponseDto>>(CurrentFileAudios));
            fileOptions.AddRange(_mapper.Map<List<FileItemOptionsResponseDto>>(CurrentFileQualities));
            fileOptions.AddRange(_mapper.Map<List<FileItemOptionsResponseDto>>(CurrentFileSubTitles));
            fileOptions.AddRange(_mapper.Map<List<FileItemOptionsResponseDto>>(CurrentFileVideos));
            return fileOptions;
        }

        public Task SetFileOptions(int streamIndex, bool isAudio, bool isSubtitle, bool isQuality)
        {
            if (!isAudio && !isSubtitle && !isQuality)
                return Task.CompletedTask;

            if (_currentlyPlayedFile == null)
                return Task.CompletedTask;

            var options = isAudio
                ? CurrentFileAudios.FirstOrDefault(a => a.Id == streamIndex)
                : isSubtitle
                    ? CurrentFileSubTitles.FirstOrDefault(s => s.Id == streamIndex)
                    : CurrentFileQualities.FirstOrDefault(q => q.Id == streamIndex);
            return FileOptionsChanged(options);
        }

        public void UpdateSettings(
            bool startFilesFromTheStart,
            bool playNextFileAutomatically,
            bool forceVideoTranscode,
            bool forceAudioTranscode,
            VideoScaleType videoScale,
            bool enableHardwareAcceleration)
        {
            Messenger.Publish(new SettingsExternallyUpdatedMessage(
                this,
                startFilesFromTheStart,
                playNextFileAutomatically,
                forceVideoTranscode,
                forceAudioTranscode,
                videoScale,
                enableHardwareAcceleration));
            //TODO: IMPROVE THIS. The settings subscription sometimes gets lost... thats why i do this
            _settingsService.StartFilesFromTheStart = startFilesFromTheStart;
            _settingsService.PlayNextFileAutomatically = playNextFileAutomatically;
            _settingsService.ForceAudioTranscode = forceAudioTranscode;
            _settingsService.ForceVideoTranscode = forceVideoTranscode;
            _settingsService.VideoScale = videoScale;
            _settingsService.EnableHardwareAcceleration = enableHardwareAcceleration;
            _appWebServer.OnAppSettingsChanged?.Invoke();
        }
        #endregion

        private async Task GoToSeconds(long seconds)
        {
            Logger.Info($"{nameof(GoToSeconds)}: Trying to go to seconds = {seconds}");
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
            Logger.Info($"{nameof(GoToSeconds)}: Trying to skip {seconds} seconds");
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

            _appWebServer.OnPlayListAdded?.Invoke(vm.Id);
        }

        private async Task DeletePlayList(PlayListItemViewModel playlist)
        {
            if (playlist == null)
                return;
            long id = playlist.Id;
            await _playListsService.DeletePlayList(id);
            playlist.CleanUp();
            PlayLists.Remove(playlist);
            _appWebServer.OnPlayListDeleted?.Invoke(id);
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
        }

        private async Task HandleCloseApp()
        {
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
            _castService.OnFileLoaded -= OnFileLoaded;
            _castService.OnTimeChanged -= OnFileDurationChanged;
            _castService.OnPositionChanged -= OnFilePositionChanged;
            _castService.OnEndReached -= OnFileEndReached;
            _castService.OnPaused -= OnPaused;
            _castService.OnDisconnected -= OnDisconnected;
            _castService.OnVolumeChanged -= OnVolumeChanged;
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

            var playlist = PlayLists.FirstOrDefault(p => p.Id == _currentlyPlayedFile.PlayListId);
            if (playlist is null)
                return;

            _onSkipOrPrevious = true;
            var fileIndex = playlist.Items.IndexOf(_currentlyPlayedFile);
            if (fileIndex < 0)
            {
                int nextPosition = _currentlyPlayedFile.Position + 1;
                int closestPosition = playlist.Items
                    .Select(f => f.Position)
                    .GetClosest(nextPosition);

                var f = playlist.Items.FirstOrDefault(f => f.Position == closestPosition);
                f?.PlayCommand?.Execute();
                return;
            }

            int newIndex = nextTrack ? fileIndex + 1 : fileIndex - 1;

            var file = playlist.Shuffle && playlist.Items.Count > 1
                ? playlist.Items.PickRandom(fileIndex)
                : playlist.Items.ElementAtOrDefault(newIndex);

            if (file is null)
            {
                Logger.Info(
                    $"{nameof(GoTo)}: File at index = {fileIndex} in playListId {playlist.Id} was not found. " +
                    "Probably an end of playlist");
                _castService.StopRunningProcess();

                if (playlist.Loop)
                {
                    Logger.Info($"{nameof(GoTo)}: Looping playlistId = {playlist.Id}");
                    playlist.Items.FirstOrDefault()?.PlayCommand?.Execute();
                }
                return;
            }

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
            if (file is null)
            {
                Logger.Warn($"{nameof(PlayFile)}: Cant play file, it is null !!!");
                return false;
            }

            DisableLoopForAllFiles(file.Id);
            if (!file.Exists)
            {
                Logger.Info($"{nameof(PlayFile)}: Cant play file = {file.Path}. It doesnt exist");
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
            IsPaused = false;

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
                _appWebServer.OnFileLoading?.Invoke();

                if (file.CanStartPlayingFromCurrentPercentage &&
                    !file.IsUrlFile &&
                    !force &&
                    !_settingsService.StartFilesFromTheStart)
                {
                    Logger.Info($"{nameof(PlayFile)}: File will be resumed from = {file.PlayedPercentage} %");
                    await _castService.GoToPosition(
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
                    await _castService.StartPlay(
                        file.Path,
                        CurrentFileVideoStreamIndex,
                        CurrentFileAudioStreamIndex,
                        CurrentFileSubTitleStreamIndex,
                        CurrentFileQuality);
                }

                _castService.GenerateThumbmnails(file.Path);

                Logger.Info($"{nameof(PlayFile)}: File is being played...");

                return true;
            }
            catch (Exception e)
            {
                Logger.Error(e, $"{nameof(PlayFile)}: Unknown error occurred");
                _telemetryService.TrackError(e);
                playList.SelectedItem = null;
                _appWebServer.OnFileLoadingError?.Invoke(GetText("CouldntPlayFile"));
                await StopPlayBack();
                await ShowSnackbarMsg(GetText("CouldntPlayFile"));
                return false;
            }
            finally
            {
                IsBusy = false;
                _onSkipOrPrevious = false;
            }
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
            _appWebServer.OnEndReached?.Invoke();
            _castService.StopRunningProcess();
            _currentlyPlayedFile?.CleanUp();
            _currentlyPlayedFile = null;
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

        private void OnFileLoaded(
            string title,
            string thumbUrl,
            double duration,
            double volumeLevel,
            bool isMuted)
        {
            CurrentFileThumbnail = thumbUrl;
            VolumeLevel = volumeLevel;
            IsMuted = isMuted;
            if (_currentlyPlayedFile?.IsUrlFile == true)
            {
                CurrentlyPlayingFilename = title;
                _currentlyPlayedFile.SetDuration(duration);
                RaisePropertyChanged(() => CurrentFileDuration);
            }

            _appWebServer.OnFileLoaded?.Invoke();
        }

        private void OnFileDurationChanged(double seconds)
        {
            IsPaused = false;

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

            IsPaused = false;

            if (_currentlyPlayedFile?.Loop == true)
            {
                Logger.Info($"{nameof(OnFileEndReached)}: Looping file = {_currentlyPlayedFile?.Path}");
                _currentlyPlayedFile.PlayedPercentage = 0;
                _currentlyPlayedFile.PlayCommand.Execute();
                return;
            }

            if (_settingsService.PlayNextFileAutomatically)
            {
                Logger.Info($"{nameof(OnFileEndReached)}: Play next file is enabled. Playing the next file...");
                GoTo(true);
            }
            else
            {
                Logger.Info($"{nameof(OnFileEndReached)}: Play next file is disabled. Next file wont be played");
                _currentlyPlayedFile?.CleanUp();
                _currentlyPlayedFile = null;
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
            _appWebServer.OnServerMsg?.Invoke(msg);
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

            //Videos
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
            if (!FileUtils.IsVideoFile(_currentlyPlayedFile.Path))
                return;

            string localSubsPath = TryGetSubTitlesLocalPath();
            bool localSubExists = !string.IsNullOrEmpty(localSubsPath);
            isEnabled = _currentlyPlayedFile.FileInfo.SubTitles.Count > 1 || localSubExists;
            CurrentFileSubTitles.Add(new FileItemOptionsViewModel
            {
                Id = NoStreamSelectedId,
                IsSubTitle = true,
                IsSelected = !localSubExists && _currentlyPlayedFile.FileInfo.SubTitles.Count == 0,
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
                    Text = Path.GetFileName(localSubsPath),
                    Path = localSubsPath
                });
            }

            isEnabled = _currentlyPlayedFile.FileInfo.SubTitles.Count > 0;
            isSelected = !localSubExists;
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

        private string TryGetSubTitlesLocalPath()
        {
            if (!FileUtils.IsLocalFile(_currentlyPlayedFile.Path))
            {
                return null;
            }

            Logger.Info($"{nameof(TryGetSubTitlesLocalPath)}: Checking if subtitle exist in the same dir as file = {_currentlyPlayedFile.Path}");
            string filename = Path.GetFileNameWithoutExtension(_currentlyPlayedFile.Path);
            string dir = Path.GetDirectoryName(_currentlyPlayedFile.Path);

            foreach (var format in AppConstants.AllowedSubtitleFormats)
            {
                string possibleSubTitlePath = Path.Combine(dir, filename + format);

                if (!File.Exists(possibleSubTitlePath))
                    continue;

                Logger.Info($"{nameof(TryGetSubTitlesLocalPath)}: Found subtitles in path = {possibleSubTitlePath}");
                return possibleSubTitlePath;
            }

            Logger.Info($"{nameof(TryGetSubTitlesLocalPath)}: No subtitles were found for file = {_currentlyPlayedFile.Path}");

            return null;
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

            Logger.Info($"{nameof(FileOptionsChanged)}: StreamId = {selectedItem.Id}  was selected. Text = {selectedItem.Text}");
            selectedItem.IsSelected = true;

            return PlayFile(_currentlyPlayedFile, false, true);
        }

        private void OnQualitiesChanged(int selectedQuality, List<int> qualities)
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

        private void OnPaused()
            => IsPaused = true;

        private void OnDisconnected()
            => OnStoppedPlayBack();

        private void OnVolumeChanged(double level, bool isMuted)
        {
            VolumeLevel = level;
            IsMuted = isMuted;
        }

        private Task OnSubTitleFileSelected(string filePath)
        {
            string filename = Path.GetFileName(filePath);
            if (!AppConstants.AllowedSubtitleFormats.Contains(Path.GetExtension(filePath).ToLower()) ||
                CurrentFileSubTitles.Any(f => f.Text == filename))
            {
                Logger.Info($"{nameof(OnSubTitleFileSelected)}: Subtitle = {filePath} is not valid or is already in the current sub files");
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
