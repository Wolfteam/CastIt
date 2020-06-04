using CastIt.Common;
using CastIt.Common.Comparers;
using CastIt.Common.Utils;
using CastIt.Interfaces;
using CastIt.Models.Entities;
using MvvmCross.Commands;
using MvvmCross.Logging;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CastIt.ViewModels.Items
{
    public class PlayListItemViewModel : BaseViewModel
    {
        #region Members
        private readonly ICastService _castService;
        private readonly IPlayListsService _playListsService;
        private string _name;
        private bool _showEditPopUp;
        private bool _showAddUrlPopUp;
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
            ICastService castService,
            IPlayListsService playListsService)
            : base(textProvider, messenger, logger.GetLogFor<PlayListItemViewModel>())
        {
            _castService = castService;
            _playListsService = playListsService;
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

            ScrollToSelectedFileCommand = new MvxCommand(() => _scrollToSelectedItem.Raise(SelectedItem));
        }

        public void CleanUp()
        {
            _setDurationTokenSource.Cancel();
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
        }

        private async Task OnUrlAdded(string url)
        {
            //TODO: URL CAN BE A PLAYLIST, SO YOU WILL NEED TO PARSE IT
            bool isUrlFile = FileUtils.IsUrlFile(url);
            if (!isUrlFile)
                return;
            var vm = await _playListsService.AddFile(Id, url, Items.Count + 1);
            await vm.SetFileInfo(_setDurationTokenSource.Token);
            Items.Add(vm);
            ShowAddUrlPopUp = false;
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
        }

        private async Task RemoveAllMissing()
        {
            var items = Items.Where(f => !f.Exists).ToList();
            if (items.Count == 0)
                return;

            await _playListsService.DeleteFiles(items.Select(f => f.Id).ToList());
            Items.RemoveItems(items);
        }

        private void SelectAll()
        {
            foreach (var file in Items)
            {
                file.IsSelected = true;
            }
        }

        private async Task SavePlayList(string newName)
        {
            if (Id > 0)
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
        }
        #endregion
    }
}
