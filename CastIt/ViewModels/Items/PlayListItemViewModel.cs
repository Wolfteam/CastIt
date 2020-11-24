using CastIt.Application.Common;
using CastIt.Application.Common.Utils;
using CastIt.Application.Interfaces;
using CastIt.Common;
using CastIt.Common.Comparers;
using CastIt.Domain.Entities;
using CastIt.Domain.Enums;
using CastIt.Infrastructure.Interfaces;
using CastIt.Interfaces;
using CastIt.Models.Messages;
using CastIt.Server.Interfaces;
using CastIt.ViewModels.Dialogs;
using Microsoft.Extensions.Logging;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CastIt.ViewModels.Items
{
    public class PlayListItemViewModel : BaseViewModel
    {
        #region Members
        private readonly IAppDataService _playListsService;
        private readonly IYoutubeUrlDecoder _youtubeUrlDecoder;
        private readonly ITelemetryService _telemetryService;
        private readonly IAppWebServer _appWebServer;
        private readonly IMvxNavigationService _navigationService;
        private readonly IAppSettingsService _appSettings;
        private readonly IFileWatcherService _fileWatcherService;
        private readonly IFileService _fileService;

        private string _name;
        private bool _showEditPopUp;
        private bool _showAddUrlPopUp;
        private bool _loop;
        private bool _shuffle;
        private bool _isBusy;
        private FileItemViewModel _selectedItem;
        private readonly CancellationTokenSource _cancellationToken = new CancellationTokenSource();

        private readonly MvxInteraction _openFileDialog = new MvxInteraction();
        private readonly MvxInteraction _openFolderDialog = new MvxInteraction();
        private readonly MvxInteraction<FileItemViewModel> _scrollToSelectedItem = new MvxInteraction<FileItemViewModel>();
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
                SetProperty(ref _loop, value);
                _appWebServer.OnPlayListChanged?.Invoke(Id);
            }
        }

        public bool Shuffle
        {
            get => _shuffle;
            set
            {
                SetProperty(ref _shuffle, value);
                _appWebServer.OnPlayListChanged?.Invoke(Id);
            }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
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
            get
            {
                var playedSeconds = Items.Sum(i => i.PlayedSeconds);
                var formatted = FileFormatConstants.FormatDuration(playedSeconds);
                return $"{formatted}";
            }
        }

        public string TotalDuration
        {
            get
            {
                var totalSeconds = Items.Where(i => i.TotalSeconds >= 0).Sum(i => i.TotalSeconds);
                var formatted = FileFormatConstants.FormatDuration(totalSeconds);
                return $"{PlayedTime} / {formatted}";
            }
        }
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
        public IMvxCommand<SortModeType> SortFilesCommand { get; private set; }
        #endregion

        #region Interactors
        public IMvxInteraction OpenFileDialog
            => _openFileDialog;

        public IMvxInteraction OpenFolderDialog
            => _openFolderDialog;

        public IMvxInteraction<FileItemViewModel> ScrollToSelectedItem
            => _scrollToSelectedItem;
        #endregion

        public PlayListItemViewModel(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            ILogger<PlayListItemViewModel> logger,
            IAppDataService playListsService,
            IYoutubeUrlDecoder youtubeUrlDecoder,
            ITelemetryService telemetryService,
            IAppWebServer appWebServer,
            IMvxNavigationService navigationService,
            IAppSettingsService appSettings,
            IFileWatcherService fileWatcherService,
            IFileService fileService)
            : base(textProvider, messenger, logger)
        {
            _playListsService = playListsService;
            _youtubeUrlDecoder = youtubeUrlDecoder;
            _telemetryService = telemetryService;
            _appWebServer = appWebServer;
            _navigationService = navigationService;
            _appSettings = appSettings;
            _fileWatcherService = fileWatcherService;
            _fileService = fileService;
        }

        #region Methods
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

            RenameCommand = new MvxAsyncCommand<string>(SavePlayList);

            ScrollToSelectedFileCommand = new MvxCommand(ScrollToSelectedFile);

            SortFilesCommand = new MvxCommand<SortModeType>(SortFiles);
        }

        public override void RegisterMessages()
        {
            base.RegisterMessages();
            SubscriptionTokens.AddRange(new[]
            {
                Messenger.Subscribe<ShowPlayListTotalDurationMessage>(async _ =>
                {
                    await RaisePropertyChanged(() => ShowTotalDuration);
                    await UpdatePlayedTime();
                })
            });
        }

        public async Task SetFilesInfo(CancellationToken token)
        {
            IsBusy = true;
            foreach (var item in Items)
            {
                if (token.IsCancellationRequested)
                    break;
                await item.SetFileInfo(token, false);
            }

            await UpdatePlayedTime();
            IsBusy = false;
        }

        public async Task SetFileInfo(long fileId, CancellationToken token)
        {
            IsBusy = true;
            var file = Items.FirstOrDefault(f => f.Id == fileId);
            if (file != null)
                await file.SetFileInfo(token);
            await UpdatePlayedTime();
            IsBusy = false;
        }

        public void CleanUp()
        {
            _cancellationToken.Cancel();

            foreach (var file in Items)
                file.CleanUp();
        }

        public void SetPositionIfChanged()
        {
            for (int i = 0; i < Items.Count; i++)
            {
                int newValue = i + 1;
                if (Items[i].Position != newValue)
                {
                    Items[i].Position = newValue;
                    Items[i].PositionChanged = true;
                }
            }
        }

        public async Task RemoveFile(long id)
        {
            var file = Items.FirstOrDefault(f => f.Id == id);
            if (file == null)
            {
                Logger.LogWarning($"{nameof(RemoveFile)}: FileId = {id} not found");
                return;
            }

            await _playListsService.DeleteFile(id);
            Items.Remove(file);
            SelectedItems.Clear();
            SetPositionIfChanged();
            _appWebServer.OnFileDeleted?.Invoke(Id);
            await UpdatePlayedTime();
        }

        public Task UpdatePlayedTime()
        {
            var dirs = Items.Where(f => f.IsLocalFile)
                .Select(f => Path.GetDirectoryName(f.Path))
                .Distinct()
                .ToList();
            _fileWatcherService.UpdateWatchers(dirs, false);
            return !_appSettings.ShowPlayListTotalDuration ? Task.CompletedTask : RaisePropertyChanged(() => TotalDuration);
        }

        private Task OnFolderAdded(string[] folders)
        {
            var files = new List<string>();
            foreach (var folder in folders)
            {
                Logger.LogInformation($"{nameof(OnFolderAdded)}: Getting all the media files from folder = {folder}");
                var filesInDir = Directory.EnumerateFiles(folder, "*.*", SearchOption.AllDirectories)
                    .Where(s => FileFormatConstants.AllowedFormats.Contains(Path.GetExtension(s).ToLower()))
                    .ToList();
                files.AddRange(filesInDir);
            }
            return OnFilesAdded(files.ToArray());
        }

        private async Task OnFilesAdded(string[] paths)
        {
            if (paths == null || paths.Length == 0)
            {
                Messenger.Publish(new SnackbarMessage(this, GetText("NoFilesToBeAdded")));
                return;
            }

            if (paths.Any(p => string.IsNullOrEmpty(p) || p.Length > AppConstants.MaxCharsPerString))
            {
                Messenger.Publish(new SnackbarMessage(this, GetText("InvalidFiles")));
                return;
            }

            try
            {
                IsBusy = true;
                int startIndex = Items.Count + 1;
                var files = paths.Where(path =>
                {
                    var ext = Path.GetExtension(path);
                    return FileFormatConstants.AllowedFormats.Contains(ext.ToLower()) && Items.All(f => f.Path != path);
                }).OrderBy(p => p, new WindowsExplorerComparer())
                .Select((path, index) => new FileItem
                {
                    Position = startIndex + index,
                    PlayListId = Id,
                    Path = path,
                    CreatedAt = DateTime.Now
                }).ToList();

                var vms = await _playListsService.AddFiles(files);

                foreach (var vm in vms)
                {
                    //vm.Position = Items.Count + 1;
                    await vm.SetFileInfo(_cancellationToken.Token);
                    Items.Add(vm);
                }

                _appWebServer.OnFileAdded?.Invoke(Id);
            }
            catch (Exception e)
            {
                Messenger.Publish(new SnackbarMessage(this, GetText("SomethingWentWrong")));
                _telemetryService.TrackError(e);
                Logger.LogError(e, $"{nameof(OnFilesAdded)}: Couldn't parse the following paths = {string.Join(",", paths)}");
            }
            finally
            {
                IsBusy = false;
                await UpdatePlayedTime();
            }
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
                IsBusy = true;

                Logger.LogInformation($"{nameof(OnUrlAdded)}: Trying to parse url = {url}");
                if (!_youtubeUrlDecoder.IsPlayList(url))
                {
                    Logger.LogInformation($"{nameof(OnUrlAdded)}: Url is not a playlist, parsing it...");
                    await AddYoutubeUrl(url);
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
                            await AddYoutubeUrl(url);
                            return;
                        //Cancel
                        case null:
                            Logger.LogInformation($"{nameof(OnUrlAdded)}: Cancel was selected, nothing will be parsed");
                            return;
                    }
                }
                Logger.LogInformation($"{nameof(OnUrlAdded)}: Parsing playlist...");
                var links = await _youtubeUrlDecoder.ParseYouTubePlayList(url, _cancellationToken.Token);
                foreach (var link in links)
                {
                    if (_cancellationToken.IsCancellationRequested)
                        break;
                    Logger.LogInformation($"{nameof(OnUrlAdded)}: Parsing playlist url = {link}");
                    await AddYoutubeUrl(link);
                }
            }
            catch (Exception e)
            {
                Messenger.Publish(new SnackbarMessage(this, GetText("UrlCouldntBeParsed")));
                _telemetryService.TrackError(e);
                Logger.LogError(e, $"{nameof(OnUrlAdded)}: Couldn't parse url = {url}");
            }
            finally
            {
                IsBusy = false;
                await UpdatePlayedTime();
            }
        }

        private async Task RemoveSelectedFiles()
        {
            if (!SelectedItems.Any())
                return;

            var ids = SelectedItems.Select(f => f.Id).ToList();
            await _playListsService.DeleteFiles(ids);
            var itemsToDelete = Items.Where(f => ids.Contains(f.Id)).ToList();
            Items.RemoveItems(itemsToDelete);
            SelectedItems.Clear();
            SetPositionIfChanged();

            _appWebServer.OnFileDeleted?.Invoke(Id);
            await UpdatePlayedTime();
        }

        private async Task RemoveAllMissing()
        {
            var items = Items.Where(f => !f.Exists).ToList();
            if (items.Count == 0)
                return;

            await _playListsService.DeleteFiles(items.Select(f => f.Id).ToList());
            Items.RemoveItems(items);
            SetPositionIfChanged();

            _appWebServer.OnFileDeleted?.Invoke(Id);
            await UpdatePlayedTime();
        }

        private void SelectAll()
        {
            foreach (var file in Items)
            {
                file.IsSelected = true;
            }
        }

        public async Task SavePlayList(string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
            {
                Messenger.Publish(new SnackbarMessage(this, GetText("InvalidPlayListName")));
                return;
            }
            newName = newName.Trim();
            if (newName.Length > AppConstants.MaxCharsPerString)
            {
                newName = newName.Substring(0, AppConstants.MaxCharsPerString);
            }
            bool added = Id <= 0;
            if (!added)
            {
                await _playListsService.UpdatePlayList(Id, newName, Position);
            }
            else
            {
                var playList = await _playListsService.AddNewPlayList(newName, Position);
                Id = playList.Id;
            }
            Name = newName;
            ShowEditPopUp = false;

            if (added)
                _appWebServer.OnPlayListAdded?.Invoke(Id);
            else
                _appWebServer.OnPlayListChanged?.Invoke(Id);
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

        private async Task AddYoutubeUrl(string url)
        {
            var media = await _youtubeUrlDecoder.Parse(url, null, false);
            if (media == null)
            {
                Logger.LogInformation($"{nameof(AddYoutubeUrl)}: Couldn't parse url = {url}");
                Messenger.Publish(new SnackbarMessage(this, GetText("UrlCouldntBeParsed")));
                return;
            }
            if (!string.IsNullOrEmpty(media.Title) && media.Title.Length > AppConstants.MaxCharsPerString)
            {
                media.Title = media.Title.Substring(0, AppConstants.MaxCharsPerString);
            }
            var vm = await _playListsService.AddFile(Id, url, Items.Count + 1, media.Title);
            await vm.SetFileInfo(_cancellationToken.Token);
            Items.Add(vm);
            _appWebServer.OnFileAdded?.Invoke(Id);
        }

        private void SortFiles(SortModeType sortBy)
        {
            if ((sortBy == SortModeType.DurationAsc || sortBy == SortModeType.DurationDesc) &&
                Items.Any(f => string.IsNullOrEmpty(f.Duration)))
            {
                Messenger.Publish(new SnackbarMessage(this, GetText("FileIsNotReadyYet")));
                return;
            }
            var sortedItems = sortBy switch
            {
                SortModeType.AlphabeticalPathAsc => Items.OrderBy(f => f.Path, new WindowsExplorerComparer()).ToList(),
                SortModeType.AlphabeticalPathDesc => Items.OrderByDescending(f => f.Path, new WindowsExplorerComparer()).ToList(),
                SortModeType.DurationAsc => Items.OrderBy(f => f.TotalSeconds).ToList(),
                SortModeType.DurationDesc => Items.OrderByDescending(f => f.TotalSeconds).ToList(),
                SortModeType.AlphabeticalNameAsc => Items.OrderBy(f => f.Filename, new WindowsExplorerComparer()).ToList(),
                SortModeType.AlphabeticalNameDesc => Items.OrderByDescending(f => f.Filename, new WindowsExplorerComparer()).ToList(),
                _ => throw new ArgumentOutOfRangeException(nameof(sortBy), sortBy, "Invalid sort mode"),
            };

            SelectedItems.Clear();
            SelectedItem = null;

            foreach (var item in sortedItems)
            {
                int currentIndex = Items.IndexOf(item);
                int newIndex = sortedItems.IndexOf(item);
                Items.Move(currentIndex, newIndex);
            }
            SetPositionIfChanged();
        }

        public void ExchangeLastFilePosition(long toFileId)
        {
            var newFile = Items.LastOrDefault();
            if (newFile == null)
                return;
            ExchangeFilePosition(newFile.Id, toFileId);
        }

        public void ExchangeFilePosition(long fromFileId, long toFileId)
        {
            var fromFile = Items.FirstOrDefault(f => f.Id == fromFileId);
            var toFile = Items.FirstOrDefault(f => f.Id == toFileId);
            if (fromFile == null || toFile == null)
            {
                return;
            }

            Items.Move(Items.IndexOf(fromFile), Items.IndexOf(toFile));
            SetPositionIfChanged();
        }
        #endregion
    }
}
