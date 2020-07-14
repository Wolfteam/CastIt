﻿using CastIt.Common;
using CastIt.Common.Comparers;
using CastIt.Common.Enums;
using CastIt.Common.Utils;
using CastIt.Interfaces;
using CastIt.Models.Entities;
using CastIt.Models.Messages;
using CastIt.ViewModels.Dialogs;
using MvvmCross.Commands;
using MvvmCross.Logging;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CastIt.ViewModels.Items
{
    public class PlayListItemViewModel : BaseViewModel
    {
        #region Members
        private readonly IPlayListsService _playListsService;
        private readonly IYoutubeUrlDecoder _youtubeUrlDecoder;
        private readonly ITelemetryService _telemetryService;
        private readonly IAppWebServer _appWebServer;
        private readonly IMvxNavigationService _navigationService;

        private string _name;
        private bool _showEditPopUp;
        private bool _showAddUrlPopUp;
        private bool _loop;
        private bool _shuffle;
        private FileItemViewModel _selectedItem;
        private readonly CancellationTokenSource _setDurationTokenSource = new CancellationTokenSource();

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
            IMvxLogProvider logger,
            IPlayListsService playListsService,
            IYoutubeUrlDecoder youtubeUrlDecoder,
            ITelemetryService telemetryService,
            IAppWebServer appWebServer,
            IMvxNavigationService navigationService)
            : base(textProvider, messenger, logger.GetLogFor<PlayListItemViewModel>())
        {
            _playListsService = playListsService;
            _youtubeUrlDecoder = youtubeUrlDecoder;
            _telemetryService = telemetryService;
            _appWebServer = appWebServer;
            _navigationService = navigationService;
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

        public void CleanUp()
        {
            _setDurationTokenSource.Cancel();

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
                Logger.Warn($"{nameof(RemoveFile)}: FileId = {id} not found");
                return;
            }

            await _playListsService.DeleteFile(id);
            Items.Remove(file);
            SelectedItems.Clear();
            SetPositionIfChanged();
            _appWebServer.OnFileDeleted?.Invoke(Id);
        }

        private async Task OnFolderAdded(string[] folders)
        {
            foreach (var folder in folders)
            {
                var files = Directory.EnumerateFiles(folder, "*.*", SearchOption.TopDirectoryOnly)
                    .Where(s => AppConstants.AllowedFormats.Contains(Path.GetExtension(s).ToLower()))
                    .ToArray();
                await OnFilesAdded(files);
            }
        }

        private async Task OnFilesAdded(string[] paths)
        {
            int startIndex = Items.Count + 1;
            var files = paths.Where(path =>
            {
                var ext = Path.GetExtension(path);
                return AppConstants.AllowedFormats.Contains(ext.ToLower()) &&
                    !Items.Any(f => f.Path == path);
            }).OrderBy(p => p, new WindowsExplorerComparer())
            .Select((path, index) =>
            {
                return new FileItem
                {
                    Position = startIndex + index,
                    PlayListId = Id,
                    Path = path,
                };
            }).ToList();

            var vms = await _playListsService.AddFiles(files);

            foreach (var vm in vms)
            {
                //vm.Position = Items.Count + 1;
                await vm.SetFileInfo(_setDurationTokenSource.Token);
                Items.Add(vm);
            }

            _appWebServer.OnFileAdded?.Invoke(Id);
        }

        private async Task OnUrlAdded(string url)
        {
            ShowAddUrlPopUp = false;
            bool isUrlFile = FileUtils.IsUrlFile(url);
            if (!isUrlFile || !_youtubeUrlDecoder.IsYoutubeUrl(url))
            {
                Messenger.Publish(new SnackbarMessage(this, GetText("UrlNotSupported")));
                return;
            }

            if (_youtubeUrlDecoder.IsPlayList(url))
            {
                if (_youtubeUrlDecoder.IsPlayListAndVideo(url))
                {
                    bool? result = await _navigationService.Navigate<ParseYoutubeVideoOrPlayListDialogViewModel, bool?>();
                    //Only video
                    if (result == true)
                    {
                        await AddUrl(url);
                        return;
                    }
                    //Cancel
                    else if (!result.HasValue)
                    {
                        return;
                    }
                }

                try
                {
                    Messenger.Publish(new IsBusyMessage(this, true));
                    if (!NetworkUtils.IsInternetAvailable())
                    {
                        Messenger.Publish(new SnackbarMessage(this, GetText("NoInternetConnection")));
                        return;
                    }

                    var links = await _youtubeUrlDecoder.ParseYouTubePlayList(url);
                    foreach (var link in links)
                    {
                        await AddUrl(link);
                    }
                }
                catch (Exception e)
                {
                    Messenger.Publish(new SnackbarMessage(this, GetText("CouldntParsePlayList")));
                    _telemetryService.TrackError(e);
                    Logger.Error(e, $"{nameof(OnUrlAdded)}: Couldnt parse youtube playlist");
                }
                finally
                {
                    Messenger.Publish(new IsBusyMessage(this, false));
                }
            }
            else
            {
                await AddUrl(url);
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
            newName = newName.Trim();
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

        private async Task AddUrl(string url)
        {
            var vm = await _playListsService.AddFile(Id, url, Items.Count + 1);
            await vm.SetFileInfo(_setDurationTokenSource.Token);
            Items.Add(vm);
            _appWebServer.OnFileAdded?.Invoke(Id);
        }

        private void SortFiles(SortModeType sortBy)
        {
            if (Items.Any(f => string.IsNullOrEmpty(f.Duration)))
            {
                Messenger.Publish(new SnackbarMessage(this, GetText("FileIsNotReadyYet")));
                return;
            }
            var sortedItems = sortBy switch
            {
                SortModeType.AlphabeticalAsc => Items.OrderBy(f => f.Path, new WindowsExplorerComparer()).ToList(),
                SortModeType.AlphabeticalDesc => Items.OrderByDescending(f => f.Path, new WindowsExplorerComparer()).ToList(),
                SortModeType.DurationAsc => Items.OrderBy(f => f.TotalSeconds).ToList(),
                SortModeType.DurationDesc => Items.OrderByDescending(f => f.TotalSeconds).ToList(),
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
        #endregion
    }
}
