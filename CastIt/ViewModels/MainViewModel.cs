﻿using AutoMapper;
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
        private readonly IFileWatcherService _fileWatcherService;

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
        public IMvxInteraction<(double, double)> SetWindowWidthAndHeight
            => _setWindowWidthAndHeight;
        public IMvxInteraction OpenSubTitleFileDialog
            => _openSubTitleFileDialog;
        public IMvxInteraction<PlayListItemViewModel> BeforeDeletingPlayList
            => _beforeDeletingPlayList;
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
            IMapper mapper,
            IFileWatcherService fileWatcher)
            : base(textProvider, messenger, logger.GetLogFor<MainViewModel>())
        {
            _castService = castService;
            _playListsService = playListsService;
            _settingsService = settingsService;
            _navigationService = navigationService;
            _telemetryService = telemetryService;
            _appWebServer = appWebServer;
            _mapper = mapper;
            _fileWatcherService = fileWatcher;
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
                playlist.Items.AddRange(files.OrderBy(f => f.Position));
            }

            foreach (var pl in PlayLists)
            {
                pl.SetPositionIfChanged();
            }
            //This needs to happen after the playlist/files are initialized, otherwise, you will be sending a lot of ws msgs
            Logger.Info($"{nameof(Initialize)}: Initializing web server...");
            _appWebServer.Init(this, _webServerCancellationToken.Token);

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
            _castService.OnFileLoadFailed += OnFileLoadFailed;

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

            InitializeOrUpdateFileWatcher(false);

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
                Shuffle = pl.Shuffle,
                TotalDuration = pl.TotalDuration
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
                TotalDuration = playlist.TotalDuration,
                Files = _mapper.Map<List<FileItemResponseDto>>(playlist.Items)
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

        public Task PlayFile(long id, long playlistId, bool force)
        {
            var playList = PlayLists.FirstOrDefault(pl => pl.Id == playlistId);
            if (playList == null)
            {
                Logger.Warn($"{nameof(PlayFile)}: Couldnt play fileId = {id} because playlistId = {playlistId} doesnt exists");
                _appWebServer.OnServerMsg?.Invoke(GetText("PlayListDoesntExist"));
                return Task.CompletedTask;
            }

            var file = playList.Items.FirstOrDefault(f => f.Id == id);
            if (file != null)
                return PlayFile(file, force);
            Logger.Warn($"{nameof(PlayFile)}: Couldnt play fileId = {id} because it doesnt exists");
            _appWebServer.OnServerMsg?.Invoke(GetText("FileDoesntExist"));
            return Task.CompletedTask;
        }

        public void SetPlayListOptions(long id, bool loop, bool shuffle)
        {
            var playlist = PlayLists.FirstOrDefault(pl => pl.Id == id);
            if (playlist == null)
            {
                Logger.Warn($"{nameof(SetPlayListOptions)}: PlaylistId = {id} doesnt exists");
                _appWebServer.OnServerMsg?.Invoke(GetText("PlayListDoesntExist"));
                return;
            }

            playlist.Loop = loop;
            playlist.Shuffle = shuffle;
        }

        public Task DeletePlayList(long id)
        {
            var playlist = PlayLists.FirstOrDefault(pl => pl.Id == id);
            if (playlist != null)
            {
                _beforeDeletingPlayList.Raise(playlist);
                return Task.CompletedTask;
            }
            Logger.Warn($"{nameof(DeletePlayList)}: Cant delete playlistId = {id} because it doesnt exists");
            return ShowSnackbarMsg(GetText("PlayListDoesntExist"));
        }

        public Task DeleteFile(long id, long playListId)
        {
            var playList = PlayLists.FirstOrDefault(pl => pl.Id == playListId);
            if (playList != null)
                return playList.RemoveFile(id);
            Logger.Warn($"{nameof(DeleteFile)}: Couldnt delete fileId = {id} because playlistId = {playListId} doesnt exists");
            return ShowSnackbarMsg(GetText("PlayListDoesntExist"));
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

        public Task RenamePlayList(long id, string newName)
        {
            var playlist = PlayLists.FirstOrDefault(pl => pl.Id == id);
            if (playlist != null)
                return playlist.SavePlayList(newName);
            Logger.Warn($"{nameof(RenamePlayList)}: Cant rename playlistId = {id} because it doesnt exists");
            return ShowSnackbarMsg(GetText("PlayListDoesntExist"));
        }
        #endregion

        private async Task GoToSeconds(long seconds)
        {
            Logger.Info($"{nameof(GoToSeconds)}: Trying to go to seconds = {seconds}");

            if (_currentlyPlayedFile == null)
            {
                Logger.Warn($"{nameof(GoToSeconds)}: Can't go to seconds = {seconds} because the current played file is null");
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
            Logger.Info($"{nameof(SkipSeconds)}: Trying to skip {seconds} seconds");
            if (_currentlyPlayedFile == null)
            {
                Logger.Warn($"{nameof(SkipSeconds)}: Can't go skip seconds = {seconds} because the current played file is null");
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
            Logger.Info($"{nameof(SetFileDurations)}: Setting file duration to all the files");
            var tasks = PlayLists.Select(pl => pl.SetFilesInfo(_setDurationTokenSource.Token)).ToList();

            await Task.WhenAll(tasks).ConfigureAwait(false);
            Logger.Info($"{nameof(SetFileDurations)}: File duration was set to all the files");
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
            Logger.Info($"{nameof(HandleCloseApp)} App is about to be closed, cleaning them all!");
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
            _castService.OnFileLoaded -= OnFileLoaded;
            _castService.OnTimeChanged -= OnFileDurationChanged;
            _castService.OnPositionChanged -= OnFilePositionChanged;
            _castService.OnEndReached -= OnFileEndReached;
            _castService.OnPaused -= OnPaused;
            _castService.OnDisconnected -= OnDisconnected;
            _castService.OnVolumeChanged -= OnVolumeChanged;
            _castService.OnFileLoadFailed -= OnFileLoadFailed;
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

                var closestFile = playlist.Items.FirstOrDefault(f => f.Position == closestPosition);
                closestFile?.PlayCommand?.Execute();
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

                if (!playlist.Loop)
                {
                    StopPlayBackCommand.Execute();
                    return;
                }
                Logger.Info($"{nameof(GoTo)}: Looping playlistId = {playlist.Id}");
                playlist.Items.FirstOrDefault()?.PlayCommand?.Execute();
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

            Logger.Info($"{nameof(PlayFile)}: Updating file info for file = {file.Filename}");
            await playList.SetFileInfo(file.Id, _setDurationTokenSource.Token);

            Logger.Info($"{nameof(PlayFile)}: Trying to play file = {file.Filename}");

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
                    Logger.Info($"{nameof(PlayFile)}: File will be resumed from = {file.PlayedPercentage} %");
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
                    Logger.Info($"{nameof(PlayFile)}: Playing file from the start");
                    await _castService.StartPlay(
                        file.Path,
                        CurrentFileVideoStreamIndex,
                        CurrentFileAudioStreamIndex,
                        CurrentFileSubTitleStreamIndex,
                        CurrentFileQuality,
                        file.FileInfo);
                }
                _currentlyPlayedFile.ListenEvents();
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

        private async void OnFileLoaded(
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
                _currentlyPlayedFile.Name = title;
                await _currentlyPlayedFile.SetDuration(duration);
                await RaisePropertyChanged(() => CurrentFileDuration);
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

            var playlist = PlayLists.FirstOrDefault(pl => pl.Id == _currentlyPlayedFile.PlayListId);
            playlist?.UpdatePlayedTime();
        }

        private void OnFilePositionChanged(double playedPercentage)
            => PlayedPercentage = playedPercentage;

        private void OnFileEndReached()
        {
            Logger.Info($"{nameof(OnFileEndReached)}: End reached for file = {_currentlyPlayedFile?.Path}");

            SetCurrentlyPlayingInfo(null, false);

            IsPaused = false;

            if (_currentlyPlayedFile != null)
            {
                var playlist = PlayLists.FirstOrDefault(pl => pl.Id == _currentlyPlayedFile.PlayListId);
                playlist?.UpdatePlayedTime();
            }

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

        private async Task SetAvailableAudiosAndSubTitles()
        {
            Logger.Info($"{nameof(SetAvailableAudiosAndSubTitles)}: Cleaning current file videos, audios and subs streams");
            CurrentFileVideos.Clear();
            CurrentFileAudios.Clear();
            CurrentFileSubTitles.Clear();
            CurrentFileQualities.Clear();

            if (_currentlyPlayedFile == null)
            {
                Logger.Warn($"{nameof(SetAvailableAudiosAndSubTitles)}: Current file is null");
                return;
            }

            if (_currentlyPlayedFile.FileInfo == null)
            {
                await _currentlyPlayedFile.SetFileInfo(_setDurationTokenSource.Token);
                if (_currentlyPlayedFile.FileInfo == null)
                {
                    Logger.Warn($"{nameof(SetAvailableAudiosAndSubTitles)}: Current file = {_currentlyPlayedFile.Path} doesnt have a fileinfo");
                    return;
                }
            }

            Logger.Info($"{nameof(SetAvailableAudiosAndSubTitles)}: Setting available file videos, audios and subs streams");

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
            if (!FileUtils.IsVideoFile(_currentlyPlayedFile.Path))
                return;

            string localSubsPath = TryGetSubTitlesLocalPath();
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
                    Text = Path.GetFileName(localSubsPath),
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

        private async void OnFileLoadFailed()
        {
            _appWebServer.OnFileLoadingError?.Invoke(GetText("CouldntPlayFile"));
            await StopPlayBack();
            await ShowSnackbarMsg(GetText("CouldntPlayFile"));
        }

        private void InitializeOrUpdateFileWatcher(bool update)
        {
            Logger.Info($"{nameof(InitializeOrUpdateFileWatcher)}: Getting directories to watch...");
            var dirs = PlayLists.SelectMany(pl => pl.Items)
                .Where(f => f.IsLocalFile)
                .Select(f => Path.GetDirectoryName(f.Path))
                .Distinct()
                .ToList();

            Logger.Info($"{nameof(InitializeOrUpdateFileWatcher)}: Got = {dirs.Count} directories...");
            if (!update)
            {
                Logger.Info($"{nameof(InitializeOrUpdateFileWatcher)}: Starting to watch for {dirs.Count} directories...");
                _fileWatcherService.StartListening(dirs);
                _fileWatcherService.OnFileCreated = OnFileCreated;
                _fileWatcherService.OnFileChanged = OnFwFileChanged;
                _fileWatcherService.OnFileDeleted = OnFwFileDeleted;
                _fileWatcherService.OnFileRenamed = OnFwFileRenamed;
            }
            else
            {
                Logger.Info($"{nameof(InitializeOrUpdateFileWatcher)}: Updating watched directories...");
                _fileWatcherService.UpdateWatchers(dirs);
            }
        }

        private Task OnFileCreated(string path)
        {
            return OnFwFileChanged(path);
        }

        private async Task OnFwFileChanged(string path)
        {
            var files = PlayLists.SelectMany(f => f.Items).Where(f => f.Path == path).ToList();
            foreach (var file in files)
            {
                var playlist = PlayLists.FirstOrDefault(f => f.Id == file.PlayListId);
                if (playlist == null)
                    continue;

                await playlist.SetFileInfo(file.Id, _setDurationTokenSource.Token);
                _appWebServer?.OnFileChanged(playlist.Id);
            }
        }

        private Task OnFwFileDeleted(string path)
        {
            return OnFwFileChanged(path);
        }

        private async Task OnFwFileRenamed(string oldPath, string newPath)
        {
            var files = PlayLists.SelectMany(f => f.Items).Where(f => f.Path == oldPath).ToList();
            foreach (var file in files)
            {
                var playlist = PlayLists.FirstOrDefault(f => f.Id == file.PlayListId);
                if (playlist == null)
                    continue;
                await playlist.OnFilesAddedCommand.ExecuteAsync(new[] { newPath });
                playlist.ExchangeLastFilePosition(file.Id);
                await playlist.RemoveFile(file.Id);
                _appWebServer?.OnFileChanged(playlist.Id);
            }
        }
        #endregion
    }
}
