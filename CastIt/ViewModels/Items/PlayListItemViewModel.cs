using AutoMapper;
using CastIt.Application.Common.Utils;
using CastIt.Application.Interfaces;
using CastIt.Application.Server;
using CastIt.Domain.Dtos.Responses;
using CastIt.Domain.Enums;
using CastIt.Interfaces;
using CastIt.Models.Messages;
using CastIt.ViewModels.Dialogs;
using Microsoft.Extensions.Logging;
using MvvmCross;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CastIt.ViewModels.Items
{
    //TODO: MOVE THE MAPPER DEPENDENCY TO INFRA PROJECT
    public class PlayListItemViewModel : BaseViewModel
    {
        #region Members
        private readonly IYoutubeUrlDecoder _youtubeUrlDecoder;
        private readonly ITelemetryService _telemetryService;
        private readonly IMvxNavigationService _navigationService;
        private readonly IDesktopAppSettingsService _appSettings;
        private readonly IFileService _fileService;
        private readonly ICastItHubClientService _castItHub;
        private readonly IMapper _mapper;

        private string _name;
        private bool _showEditPopUp;
        private bool _showAddUrlPopUp;
        private bool _loop;
        private bool _shuffle;
        private bool _isBusy;
        private int _numberOfFiles;
        private string _imageUrl;
        private string _playedTime;
        private string _totalDuration;
        private FileItemViewModel _selectedItem;

        private readonly MvxInteraction _openFileDialog = new MvxInteraction();
        private readonly MvxInteraction _openFolderDialog = new MvxInteraction();
        private readonly MvxInteraction<FileItemViewModel> _scrollToSelectedItem = new MvxInteraction<FileItemViewModel>();
        private readonly MvxInteraction _selectAll = new MvxInteraction();
        #endregion

        #region Properties
        public long Id { get; set; }
        public int Position { get; set; }
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public bool ShowEditPopUp
        {
            get => _showEditPopUp;
            set => SetProperty(ref _showEditPopUp, value);
        }

        public bool ShowAddUrlPopUp
        {
            get => _showAddUrlPopUp;
            set => SetProperty(ref _showAddUrlPopUp, value);
        }

        public bool Loop
        {
            get => _loop;
            set
            {
                bool triggerChange = _loop != value;
                SetProperty(ref _loop, value);
                if (triggerChange && !Loading)
                    UpdatePlayListOptions();
            }
        }

        public bool Shuffle
        {
            get => _shuffle;
            set
            {
                bool triggerChange = _shuffle != value;
                SetProperty(ref _shuffle, value);
                if (triggerChange && !Loading)
                    UpdatePlayListOptions();
            }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public int NumberOfFiles
        {
            get => _numberOfFiles;
            set => this.RaiseAndSetIfChanged(ref _numberOfFiles, value);
        }

        public string ImageUrl
        {
            get => _imageUrl;
            set => this.RaiseAndSetIfChanged(ref _imageUrl, value);
        }

        public FileItemViewModel SelectedItem
        {
            get => _selectedItem;
            set
            {
                SetProperty(ref _selectedItem, value);
                _scrollToSelectedItem.Raise(value);
            }
        }

        public MvxObservableCollection<FileItemViewModel> Items { get; set; }
            = new MvxObservableCollection<FileItemViewModel>();
        public MvxObservableCollection<FileItemViewModel> SelectedItems { get; set; }
            = new MvxObservableCollection<FileItemViewModel>();

        public bool ShowTotalDuration
            => _appSettings.ShowPlayListTotalDuration;

        public string PlayedTime
        {
            get => _playedTime;
            set => this.RaiseAndSetIfChanged(ref _playedTime, value);
        }

        public string TotalDuration
        {
            get => _totalDuration;
            set => this.RaiseAndSetIfChanged(ref _totalDuration, value);
        }

        public bool Loading { get; private set; }
        #endregion

        #region Commands
        public IMvxCommand OpenEditPopUpCommand { get; private set; }
        public IMvxCommand AddFilesCommand { get; private set; }
        public IMvxCommand AddFolderCommand { get; private set; }
        public IMvxCommand ShowAddUrlPopUpCommand { get; set; }
        public IMvxAsyncCommand<string[]> OnFolderAddedCommand { get; private set; }
        public IMvxAsyncCommand<string[]> OnFilesAddedCommand { get; private set; }
        public IMvxAsyncCommand<string> AddUrlCommand { get; private set; }
        public IMvxCommand<FileItemViewModel> PlayFileCommand { get; set; }
        public IMvxAsyncCommand<FileItemViewModel> RemoveFileCommand { get; private set; }
        public IMvxAsyncCommand RemoveAllMissingCommand { get; private set; }
        public IMvxCommand SelectAllCommand { get; private set; }
        public IMvxAsyncCommand<string> RenameCommand { get; private set; }
        public IMvxCommand ScrollToSelectedFileCommand { get; set; }
        public IMvxAsyncCommand<SortModeType> SortFilesCommand { get; private set; }
        #endregion

        #region Interactors
        public IMvxInteraction OpenFileDialog
            => _openFileDialog;

        public IMvxInteraction OpenFolderDialog
            => _openFolderDialog;

        public IMvxInteraction<FileItemViewModel> ScrollToSelectedItem
            => _scrollToSelectedItem;

        public IMvxInteraction SelectAllItems
            => _selectAll;
        #endregion

        public PlayListItemViewModel(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            ILogger<PlayListItemViewModel> logger,
            IYoutubeUrlDecoder youtubeUrlDecoder,
            ITelemetryService telemetryService,
            IMvxNavigationService navigationService,
            IDesktopAppSettingsService appSettings,
            IFileService fileService,
            ICastItHubClientService castItHub,
            IMapper mapper)
            : base(textProvider, messenger, logger)
        {
            _youtubeUrlDecoder = youtubeUrlDecoder;
            _telemetryService = telemetryService;
            _navigationService = navigationService;
            _appSettings = appSettings;
            _fileService = fileService;
            _castItHub = castItHub;
            _mapper = mapper;
        }

        #region Methods
        public static PlayListItemViewModel From(GetAllPlayListResponseDto playList, IMapper mapper)
        {
            var vm = Mvx.IoCProvider.Resolve<PlayListItemViewModel>();
            vm.Loading = true;
            mapper.Map(playList, vm);
            vm.Loading = false;
            return vm;
        }

        public async void LoadFileItems()
        {
            try
            {
                IsBusy = true;
                Loading = true;
                var playList = await _castItHub.GetPlayList(Id).ConfigureAwait(false);
                var mapped = playList.Files.ConvertAll(f => FileItemViewModel.From(f, _mapper));
                ClosePlayList();
                Items.ReplaceWith(mapped);
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"{nameof(LoadFileItems)}: Unknown error occurred");
            }
            finally
            {
                IsBusy = false;
                Loading = false;
            }
        }

        public void ClosePlayList()
        {
            SelectedItem = null;
            SelectedItems.Clear();
            Items.Clear();
            Loading = false;
        }

        public override void SetCommands()
        {
            base.SetCommands();
            AddFilesCommand = new MvxCommand(() => _openFileDialog.Raise());

            AddFolderCommand = new MvxCommand(() => _openFolderDialog.Raise());

            ShowAddUrlPopUpCommand = new MvxCommand(() => ShowAddUrlPopUp = true);

            OnFolderAddedCommand = new MvxAsyncCommand<string[]>(OnFolderAdded);

            OnFilesAddedCommand = new MvxAsyncCommand<string[]>(OnFilesAdded);

            AddUrlCommand = new MvxAsyncCommand<string>(OnUrlAdded);

            OpenEditPopUpCommand = new MvxCommand(() => ShowEditPopUp = true);

            PlayFileCommand = new MvxCommand<FileItemViewModel>((item) => item.PlayCommand.Execute());

            RemoveFileCommand = new MvxAsyncCommand<FileItemViewModel>(async (_) => await RemoveSelectedFiles());

            RemoveAllMissingCommand = new MvxAsyncCommand(RemoveAllMissing);

            SelectAllCommand = new MvxCommand(SelectAll);

            RenameCommand = new MvxAsyncCommand<string>(RenamePlayList);

            ScrollToSelectedFileCommand = new MvxCommand(ScrollToSelectedFile);

            SortFilesCommand = new MvxAsyncCommand<SortModeType>(SortFiles);
        }

        public override void RegisterMessages()
        {
            base.RegisterMessages();
            SubscriptionTokens.AddRange(new[]
            {
                Messenger.Subscribe<ShowPlayListTotalDurationMessage>(async _ => await RaisePropertyChanged(() => ShowTotalDuration))
            });
        }

        public async void MoveFile(int currentIndex, int newIndex, bool isInTheTop)
        {
            bool move = currentIndex >= 0 && newIndex >= 0 && currentIndex != newIndex;
            if (!move)
                return;
            if (newIndex > currentIndex && isInTheTop)
            {
                newIndex--;
            }
            else if (newIndex < currentIndex && !isInTheTop)
            {
                newIndex++;
            }

            //if (moveToTheTop)
            //{
            //    itemIndex--;
            //    if (itemIndex < 0)
            //    {
            //        itemIndex = 0;
            //    }
            //}

            var file = Items.ElementAtOrDefault(currentIndex);
            if (file == null)
            {
                return;
            }

            Logger.LogInformation($"{nameof(MoveFile)}: Moving file located at index = {currentIndex} to = {newIndex}...");
            await _castItHub.UpdateFilePosition(Id, file.Id, newIndex);
        }

        private async Task OnFolderAdded(string[] folders)
        {
            if (folders == null || folders.Length == 0)
            {
                Messenger.Publish(new SnackbarMessage(this, GetText("NoFilesToBeAdded")));
                return;
            }

            //TODO:Show a dialog to include or exclude subfolders
            await _castItHub.AddFolders(Id, true, folders);
        }

        private async Task OnFilesAdded(string[] paths)
        {
            if (paths == null || paths.Length == 0)
            {
                Messenger.Publish(new SnackbarMessage(this, GetText("NoFilesToBeAdded")));
                return;
            }

            if (paths.Any(p => string.IsNullOrEmpty(p) || p.Length > AppWebServerConstants.MaxCharsPerString))
            {
                Messenger.Publish(new SnackbarMessage(this, GetText("InvalidFiles")));
                return;
            }

            await _castItHub.AddFiles(Id, paths);
        }

        private async Task OnUrlAdded(string url)
        {
            if (!NetworkUtils.IsInternetAvailable())
            {
                Messenger.Publish(new SnackbarMessage(this, GetText("NoInternetConnection")));
                return;
            }

            ShowAddUrlPopUp = false;
            bool isUrlFile = _fileService.IsUrlFile(url);
            if (!isUrlFile || !_youtubeUrlDecoder.IsYoutubeUrl(url))
            {
                Messenger.Publish(new SnackbarMessage(this, GetText("UrlNotSupported")));
                return;
            }

            try
            {
                Logger.LogInformation($"{nameof(OnUrlAdded)}: Trying to parse url = {url}");
                if (!_youtubeUrlDecoder.IsPlayList(url))
                {
                    Logger.LogInformation($"{nameof(OnUrlAdded)}: Url is not a playlist, parsing it...");
                    await _castItHub.AddUrlFile(Id, url, true);
                    return;
                }

                if (_youtubeUrlDecoder.IsPlayListAndVideo(url))
                {
                    Logger.LogInformation($"{nameof(OnUrlAdded)}: Url is a playlist and a video, asking which one should we parse..");
                    bool? result = await _navigationService
                        .Navigate<ParseYoutubeVideoOrPlayListDialogViewModel, bool?>();
                    switch (result)
                    {
                        //Only video
                        case true:
                            Logger.LogInformation($"{nameof(OnUrlAdded)}: Parsing only the video...");
                            await _castItHub.AddUrlFile(Id, url, true);
                            return;
                        //Cancel
                        case null:
                            Logger.LogInformation($"{nameof(OnUrlAdded)}: Cancel was selected, nothing will be parsed");
                            return;
                    }
                }
                Logger.LogInformation($"{nameof(OnUrlAdded)}: Parsing playlist...");
                await _castItHub.AddUrlFile(Id, url, false);
            }
            catch (Exception e)
            {
                Messenger.Publish(new SnackbarMessage(this, GetText("UrlCouldntBeParsed")));
                _telemetryService.TrackError(e);
                Logger.LogError(e, $"{nameof(OnUrlAdded)}: Couldn't parse url = {url}");
            }
        }

        private async Task RemoveSelectedFiles()
        {
            if (!SelectedItems.Any())
                return;

            var ids = SelectedItems.Select(f => f.Id).ToList();
            await _castItHub.RemoveFiles(Id, ids);
            SelectedItems.Clear();
        }

        private async Task RemoveAllMissing()
        {
            var fileIds = Items.Where(f => !f.Exists).Select(f => f.Id).ToList();
            if (fileIds.Count == 0)
                return;

            await _castItHub.RemoveFiles(Id, fileIds);
        }

        private void SelectAll()
        {
            _selectAll.Raise();
        }

        private async Task RenamePlayList(string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
            {
                Messenger.Publish(new SnackbarMessage(this, GetText("InvalidPlayListName")));
                return;
            }
            ShowEditPopUp = false;
            await _castItHub.UpdatePlayList(Id, newName);
        }

        private void ScrollToSelectedFile()
        {
            var currentPlayedFile = Items.FirstOrDefault(f => f.IsBeingPlayed);
            if (currentPlayedFile != null)
            {
                SelectedItem = currentPlayedFile;
            }
            _scrollToSelectedItem.Raise(SelectedItem);
        }

        private async Task SortFiles(SortModeType sortBy)
        {
            SelectedItems.Clear();
            SelectedItem = null;
            await _castItHub.SortFiles(Id, sortBy);
        }

        private async void UpdatePlayListOptions()
        {
            await _castItHub.SetPlayListOptions(Id, Loop, Shuffle);
        }
        #endregion
    }
}
