using CastIt.Domain;
using CastIt.ViewModels.Items;
using Microsoft.Win32;
using MvvmCross.Platforms.Wpf.Views;
using MvvmCross.ViewModels;
using System;
using System.Collections.Generic;
using System.Windows;

namespace CastIt.Views.UserControls
{
    public class BasePlayListItem : MvxWpfView
    {
        private IMvxInteraction _openFileDialogRequest;
        private IMvxInteraction _openFolderDialogRequest;

        //With the use of the disposables, we avoid weird bugs that execute multiple times the same interactions
        protected readonly List<IDisposable> Disposables = new List<IDisposable>();

        //If you override the ViewModel, mvvmcross will think that this view belongs to the PlayListItemViewModel
        //That´s why we do it like this, otherwise PlayListItemViewModel.ViewDestroy will get called, and we will loose the subscriptions
        public PlayListItemViewModel Vm
            => DataContext as PlayListItemViewModel;

        public IMvxInteraction OpenFileDialogRequest
        {
            get => _openFileDialogRequest;
            set
            {
                _openFileDialogRequest = value;
                if (value != null)
                {
                    Disposables.Add(_openFileDialogRequest.WeakSubscribe(OpenFileDialogHandler));
                }
            }
        }

        public IMvxInteraction OpenFolderDialogRequest
        {
            get => _openFolderDialogRequest;
            set
            {
                _openFolderDialogRequest = value;
                if (value != null)
                {
                    Disposables.Add(_openFolderDialogRequest.WeakSubscribe(OpenFolderDialogHandler));
                }
            }
        }

        public BasePlayListItem()
        {
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        public virtual void OnLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnLoaded;

            if (Vm is null)
            {
                throw new InvalidOperationException("The view model should not be null");
            }

            //This is required to trigger the datacontext change event that triggers the delayed binding
            BindingContext.DataContext = DataContext;
        }

        public virtual void OnUnloaded(object sender, RoutedEventArgs e)
        {
            Unloaded -= OnUnloaded;
            foreach (var disposable in Disposables)
            {
                disposable.Dispose();
            }
            Disposables.Clear();
        }

        private void OpenFileDialogHandler(object sender, EventArgs e)
        {
            var allowedFormats = FileFormatConstants.AllowedFormatsString;
            string filter = $"{Vm.GetText("VideoOrMusicFiles")} ({allowedFormats})|{allowedFormats}|{Vm.GetText("AllFiles")} (*.*)|*.*";
            var openFileDialog = new OpenFileDialog
            {
                Filter = filter,
                Multiselect = true,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };
            var result = openFileDialog.ShowDialog();
            if (result == true)
            {
                Vm.OnFilesAddedCommand.Execute(openFileDialog.FileNames);
            }
        }

        private void OpenFolderDialogHandler(object sender, EventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            var result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                Vm.OnFolderAddedCommand.Execute(new[] { dialog.SelectedPath });
            }
        }
    }
}
