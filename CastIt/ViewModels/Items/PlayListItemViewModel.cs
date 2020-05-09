using CastIt.Common;
using CastIt.Interfaces;
using CastIt.Models.Entities;
using MvvmCross.Commands;
using MvvmCross.Logging;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using System.IO;
using System.Linq;
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

        private readonly MvxInteraction _openFileDialog = new MvxInteraction();
        private readonly MvxInteraction _openFolderDialog = new MvxInteraction();
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
            set => SetProperty(ref _selectedItem, value);
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
        public IMvxAsyncCommand<string> OnFolderAddedCommand { get; private set; }
        public IMvxAsyncCommand<string[]> OnFilesAddedCommand { get; private set; }
        public IMvxAsyncCommand<string> AddUrlCommand { get; private set; }
        public IMvxCommand<FileItemViewModel> PlayFileCommand { get; set; }
        public IMvxAsyncCommand<FileItemViewModel> RemoveFileCommand { get; private set; }
        public IMvxAsyncCommand RemoveAllMissingCommand { get; private set; }
        public IMvxCommand SelectAllCommand { get; private set; }
        public IMvxAsyncCommand<string> RenameCommand { get; private set; }
        #endregion

        #region Interactors
        public IMvxInteraction OpenFileDialog
            => _openFileDialog;

        public IMvxInteraction OpenFolderDialog
            => _openFolderDialog;
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

            OnFolderAddedCommand = new MvxAsyncCommand<string>(OnFolderAdded);

            OnFilesAddedCommand = new MvxAsyncCommand<string[]>(OnFilesAdded);

            AddUrlCommand = new MvxAsyncCommand<string>(OnUrlAdded);

            OpenEditPopUpCommand = new MvxCommand(() => ShowEditPopUp = true);

            PlayFileCommand = new MvxCommand<FileItemViewModel>((item) => item.PlayCommand.Execute());

            RemoveFileCommand = new MvxAsyncCommand<FileItemViewModel>(async (_) => await RemoveSelectedFiles().ConfigureAwait(false));

            RemoveAllMissingCommand = new MvxAsyncCommand(RemoveAllMissing);

            SelectAllCommand = new MvxCommand(SelectAll);

            RenameCommand = new MvxAsyncCommand<string>(SavePlayList);
        }

        private Task OnFolderAdded(string folder)
        {
            var files = Directory.EnumerateFiles(folder, "*.*", SearchOption.TopDirectoryOnly)
                .Where(s => AppConstants.AllowedFormats.Contains(Path.GetExtension(s).ToLower()))
                .ToArray();
            return OnFilesAdded(files);
        }

        private async Task OnFilesAdded(string[] paths)
        {
            int startIndex = Items.Count + 1;
            var files = paths.Where(path =>
            {
                var ext = Path.GetExtension(path);
                return AppConstants.AllowedFormats.Contains(ext.ToLower()) &&
                    !Items.Any(f => f.Path == path);
            }).OrderBy(p => p)
            .Select((path, index) =>
            {
                return new FileItem
                {
                    Position = startIndex + index,
                    PlayListId = Id,
                    Path = path,
                };
            }).ToList();

            var vms = await _playListsService.AddFiles(files).ConfigureAwait(false);

            foreach (var vm in vms)
            {
                //vm.Position = Items.Count + 1;
                await vm.SetDuration().ConfigureAwait(false);
                Items.Add(vm);
            }
        }

        private async Task OnUrlAdded(string url)
        {
            //TODO: URL CAN BE A PLAYLIST, SO YOU WILL NEED TO PARSE IT
            bool isUrlFile = _castService.IsUrlFile(url);
            if (!isUrlFile)
                return;
            var vm = await _playListsService.AddFile(Id, url, Items.Count + 1).ConfigureAwait(false);
            await vm.SetDuration().ConfigureAwait(false);
            Items.Add(vm);
            ShowAddUrlPopUp = false;
        }

        private async Task RemoveSelectedFiles()
        {
            if (!SelectedItems.Any())
                return;

            var ids = SelectedItems.Select(f => f.Id).ToList();
            await _playListsService.DeleteFiles(ids).ConfigureAwait(false);
            var itemsToDelete = Items.Where(f => ids.Contains(f.Id)).ToList();
            Items.RemoveItems(itemsToDelete);
            SelectedItems.Clear();
        }

        private async Task RemoveAllMissing()
        {
            var items = Items.Where(f => !f.Exists).ToList();
            if (items.Count == 0)
                return;

            await _playListsService.DeleteFiles(items.Select(f => f.Id).ToList()).ConfigureAwait(false);
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
                await _playListsService.UpdatePlayList(Id, newName, Position).ConfigureAwait(false);
            }
            else
            {
                var playList = await _playListsService.AddNewPlayList(newName, Position).ConfigureAwait(false);
                Id = playList.Id;
            }
            Name = newName;
            ShowEditPopUp = false;
        }
        #endregion
    }
}
