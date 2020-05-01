using AutoMapper;
using CastIt.Common;
using CastIt.Interfaces;
using CastIt.Models.Entities;
using MvvmCross.Commands;
using MvvmCross.Logging;
using MvvmCross.Navigation;
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
        private readonly IMapper _mapper;
        private string _name;

        private readonly MvxInteraction _openFileDialog = new MvxInteraction();
        private readonly MvxInteraction _openFolderDialog = new MvxInteraction();
        #endregion

        #region Properties
        public long Id { get; set; }
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public MvxObservableCollection<FileItemViewModel> Items { get; set; }
            = new MvxObservableCollection<FileItemViewModel>();
        #endregion

        #region Commands
        public IMvxCommand RenameCommand { get; private set; }
        public IMvxCommand AddFilesCommand { get; private set; }
        public IMvxCommand AddFolderCommand { get; private set; }
        public IMvxAsyncCommand<string> OnFolderAddedCommand { get; private set; }
        public IMvxAsyncCommand<string[]> OnFilesAddedCommand { get; private set; }
        public IMvxCommand<FileItemViewModel> PlayFileCommand { get; set; }
        public IMvxCommand<FileItemViewModel> RemoveFileCommand { get; private set; }
        public IMvxCommand RemoveAllMissingCommand { get; private set; }
        public IMvxCommand SelectAllCommand { get; private set; }
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
            IMvxNavigationService navigationService,
            IMapper mapper)
            : base(textProvider, messenger, logger.GetLogFor<PlayListItemViewModel>(), navigationService)
        {
            _mapper = mapper;
        }

        #region Methods
        public override void SetCommands()
        {
            base.SetCommands();
            AddFilesCommand = new MvxCommand(() => _openFileDialog.Raise());

            AddFolderCommand = new MvxCommand(() => _openFolderDialog.Raise());

            OnFolderAddedCommand = new MvxAsyncCommand<string>(OnFolderAdded);

            OnFilesAddedCommand = new MvxAsyncCommand<string[]>(OnFilesAdded);

            RenameCommand = new MvxCommand(() =>
            {
                System.Diagnostics.Debug.WriteLine("Rename playlist");
            });

            PlayFileCommand = new MvxCommand<FileItemViewModel>((item) => item.PlayCommand.Execute());

            RemoveFileCommand = new MvxCommand<FileItemViewModel>((item) => Items.Remove(item));

            RemoveAllMissingCommand = new MvxCommand(RemoveAllMissing);

            SelectAllCommand = new MvxCommand(SelectAll);
        }

        private async Task OnFolderAdded(string folder)
        {
            var files = Directory.EnumerateFiles(folder, "*.*", SearchOption.TopDirectoryOnly)
                .Where(s => AppConstants.AllowedFormats.Contains(Path.GetExtension(s).ToLower()))
                .ToArray();
            await OnFilesAdded(files);
        }

        private async Task OnFilesAdded(string[] paths)
        {
            foreach (var path in paths)
            {
                var ext = Path.GetExtension(path);
                if (!AppConstants.AllowedFormats.Contains(ext.ToLower()))
                {
                    continue;
                }

                var file = new FileItem
                {
                    Path = path,
                    PlayListId = Id,
                    Position = Items.Count,
                };

                var vm = _mapper.Map<FileItemViewModel>(file);
                await vm.SetDuration();
                Items.Add(vm);
            }
        }

        private void RemoveAllMissing()
        {
            var items = Items.Where(f => !f.Exists);
            Items.RemoveItems(items);
        }

        private void SelectAll()
        {
            foreach (var file in Items)
            {
                file.IsSelected = true;
            }
        }
        #endregion
    }
}
