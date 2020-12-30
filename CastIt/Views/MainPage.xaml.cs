using CastIt.Application.Common;
using CastIt.ViewModels;
using CastIt.ViewModels.Items;
using Microsoft.Win32;
using MvvmCross.Base;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Platforms.Wpf.Views;
using MvvmCross.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CastIt.Views
{
    [MvxViewFor(typeof(MainViewModel))]
    public partial class MainPage : MvxWpfView<MainViewModel>
    {
        //TODO: IF YOU DRAG OUT OF THE WINDOW, THE SEPARATORS ARE SHOWN

        private IMvxInteraction _closeAppRequest;
        private IMvxInteraction _openSubTitleFileDialogRequest;
        private IMvxInteraction<PlayListItemViewModel> _beforeDeletingPlayListRequest;

        public IMvxInteraction CloseAppRequest
        {
            get => _closeAppRequest;
            set
            {
                if (_closeAppRequest != null)
                    _closeAppRequest.Requested -= CloseAppHandler;

                _closeAppRequest = value;
                if (value != null)
                    _closeAppRequest.Requested += CloseAppHandler;
            }
        }

        public IMvxInteraction OpenSubTitleFileDialogRequest
        {
            get => _openSubTitleFileDialogRequest;
            set
            {
                if (_openSubTitleFileDialogRequest != null)
                    _openSubTitleFileDialogRequest.Requested -= OpenSubtitleFileDialog;

                _openSubTitleFileDialogRequest = value;
                if (value != null)
                    _openSubTitleFileDialogRequest.Requested += OpenSubtitleFileDialog;
            }
        }

        public IMvxInteraction<PlayListItemViewModel> BeforeDeletingPlayListRequest
        {
            get => _beforeDeletingPlayListRequest;
            set
            {
                if (_beforeDeletingPlayListRequest != null)
                    _beforeDeletingPlayListRequest.Requested -= BeforeDeletingPlayList;

                _beforeDeletingPlayListRequest = value;
                if (value != null)
                    _beforeDeletingPlayListRequest.Requested += BeforeDeletingPlayList;
            }
        }

        public MainPage()
        {
            InitializeComponent();

            var set = this.CreateBindingSet<MainPage, MainViewModel>();
            set.Bind(this).For(v => v.CloseAppRequest).To(vm => vm.CloseApp).OneWay();
            set.Bind(this).For(v => v.OpenSubTitleFileDialogRequest).To(vm => vm.OpenSubTitleFileDialog).OneWay();
            set.Bind(this).For(v => v.BeforeDeletingPlayListRequest).To(vm => vm.BeforeDeletingPlayList).OneWay();
            set.Apply();
        }

        public Dictionary<PlayListItemViewModel, int> GetTabsPosition()
        {
            if (ContentFrame.Content is PlayListsPage page)
            {
                return page.PlayListTabControl
                    .GetOrderedHeaders()
                    .ToDictionary(a => (a.Content as PlayListItemViewModel), a => a.LogicalIndex);
            }
            return new Dictionary<PlayListItemViewModel, int>();
        }

        private void OpenSubtitleFileDialog(object sender, EventArgs e)
        {
            var allowedFormats = FileFormatConstants.AllowedSubtitleFormatsString;
            string filter = $"{ViewModel.GetText("Subtitles")} ({allowedFormats})|{allowedFormats}|{ViewModel.GetText("AllFiles")} (*.*)|*.*";
            var openFileDialog = new OpenFileDialog
            {
                Filter = filter,
                Multiselect = false,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };
            var result = openFileDialog.ShowDialog();
            if (result == true)
            {
                ViewModel.SetSubTitlesCommand.Execute(openFileDialog.FileName);
            }
        }

        private void CloseAppHandler(object sender, EventArgs e) => WindowButtons.CloseApp();

        private async void BeforeDeletingPlayList(object sender, MvxValueEventArgs<PlayListItemViewModel> e)
        {
            if (e.Value == null)
                return;
            var tabs = GetTabsPosition();
            var (playlist, logicalIndex) = tabs.FirstOrDefault(t => t.Key.Id == e.Value.Id);
            await ViewModel.DeletePlayList(logicalIndex, playlist);
        }
    }
}
