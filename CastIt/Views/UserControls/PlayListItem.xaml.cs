﻿using CastIt.Common;
using CastIt.Common.Utils;
using CastIt.ViewModels.Items;
using Microsoft.Win32;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Platforms.Wpf.Views;
using MvvmCross.ViewModels;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CastIt.Views.UserControls
{
    public partial class PlayListItem : MvxWpfView<PlayListItemViewModel>
    {
        private Point _moveStartPoint;
        private int _selectedItemIndex = -1;
        private const string PlaylistItemMoveFormat = "PlaylistItemMoveFormat";

        private IMvxInteraction _openFileDialogRequest;
        public IMvxInteraction OpenFileDialogRequest
        {
            get => _openFileDialogRequest;
            set
            {
                if (_openFileDialogRequest != null)
                    _openFileDialogRequest.Requested -= (sender, args) => OpenFileDialog();

                _openFileDialogRequest = value;
                if (value != null)
                    _openFileDialogRequest.Requested += (sender, args) => OpenFileDialog();
            }
        }

        private IMvxInteraction _openFolderDialogRequest;
        public IMvxInteraction OpenFolderDialogRequest
        {
            get => _openFolderDialogRequest;
            set
            {
                if (_openFolderDialogRequest != null)
                    _openFolderDialogRequest.Requested -= (sender, args) => OpenFolderDialog();

                _openFolderDialogRequest = value;
                if (value != null)
                    _openFolderDialogRequest.Requested += (sender, args) => OpenFolderDialog();
            }
        }

        public PlayListItem()
        {
            InitializeComponent();

            var set = this.CreateBindingSet<PlayListItem, PlayListItemViewModel>();
            set.Bind(this).For(v => v.OpenFileDialogRequest).To(vm => vm.OpenFileDialog).OneWay();
            set.Bind(this).For(v => v.OpenFolderDialogRequest).To(vm => vm.OpenFolderDialog).OneWay();
            set.Apply();

            Loaded += (sender, args) => SetViewModel();
        }

        private void SetViewModel()
        {
            Loaded -= (sender, args) => SetViewModel();
            ViewModel = DataContext as PlayListItemViewModel;
        }

        private void OpenFileDialog()
        {
            var allowedFormats = AppConstants.AllowedFormatsString;
            string filter = $"Video or Music files ({allowedFormats})|{allowedFormats}|All files (*.*)|*.*";
            var openFileDialog = new OpenFileDialog
            {
                Filter = filter,
                Multiselect = true,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };
            var result = openFileDialog.ShowDialog();
            if (result == true)
            {
                ViewModel.OnFilesAddedCommand.Execute(openFileDialog.FileNames);
            }
        }

        private void OpenFolderDialog()
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            var result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                ViewModel.OnFolderAddedCommand.Execute(dialog.SelectedPath);
            }
        }

        private void PlaylistLv_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Get current mouse position
            _moveStartPoint = e.GetPosition(null);
        }

        private void PlaylistLv_MouseMove(object sender, MouseEventArgs e)
        {
            // Get the current mouse position
            Point mousePos = e.GetPosition(null);
            Vector diff = _moveStartPoint - mousePos;

            if (e.LeftButton != MouseButtonState.Pressed ||
                !DragIsBeyondMinimum(diff))
                return;

            // Get the dragged ListViewItem
            var listView = sender as ListView;
            var listViewItem = WindowsUtils.FindAnchestor<ListViewItem>((DependencyObject)e.OriginalSource);
            if (listViewItem == null)
                return;
            // Find the data behind the ListViewItem
            var item = (FileItemViewModel)listView.ItemContainerGenerator.ItemFromContainer(listViewItem);
            if (item == null)
                return;
            // Initialize the drag & drop operation
            _selectedItemIndex = PlaylistLv.SelectedIndex;
            var dragData = new DataObject(PlaylistItemMoveFormat, item);
            //Once this is called, the mouse move method will not 
            //be called until the drop method
            DragDrop.DoDragDrop(listViewItem, dragData, DragDropEffects.Copy | DragDropEffects.Move);
        }

        private void PlaylistLv_DragEnter(object sender, DragEventArgs e)
        {
            if (sender != e.Source)
                return;

            if (!e.Data.GetDataPresent(PlaylistItemMoveFormat) &&
                !e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                System.Diagnostics.Debug.WriteLine("Invalid drag enter...");
                e.Effects = DragDropEffects.None;
            }
            else
            {
                ToggleSeparatorLine(sender as ListView, e);
            }
        }

        private void PlaylistLv_DragLeave(object sender, DragEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Drag leave");
            ToggleSeparatorLine(sender as ListView, e);
        }

        private void PlaylistLv_Drop(object sender, DragEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Drop");
            HideAllSeparatorsLines();
            if (sender != e.Source)
                return;

            if (e.Data.GetDataPresent(PlaylistItemMoveFormat))
            {
                HandlePlaylistItemMove(sender as ListView, e);
            }
            else if (e.Data.GetDataPresent(DataFormats.FileDrop) &&
                e.Data.GetData(DataFormats.FileDrop) is string[] files &&
                files.Any())
            {
                ViewModel.OnFilesAddedCommand.Execute(files);
            }
        }

        private bool DragIsBeyondMinimum(Vector diff)
        {
            return Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance;
        }

        private void ToggleSeparatorLine(ListView listView, DragEventArgs e)
        {
            var listViewItem = WindowsUtils.FindAnchestor<ListViewItem>((DependencyObject)e.OriginalSource);
            if (listViewItem == null)
            {
                return;
            }
            var isInTheTop = IsDragInTheTopOfItem(e, listViewItem);
            var item = (FileItemViewModel)listView.ItemContainerGenerator.ItemFromContainer(listViewItem);
            int index = ViewModel.Items.IndexOf(item);
            HideAllSeparatorsLines();
            if (index != _selectedItemIndex)
            {
                bool showBottom = index == ViewModel.Items.Count - 1 && !isInTheTop;
                bool showTop = !showBottom && isInTheTop;
                item.ShowItemSeparators(showTop, showBottom);
                //item.ShowItemSeparators(isInTheTop, !isInTheTop);
            }
        }

        private void HandlePlaylistItemMove(ListView listView, DragEventArgs e)
        {
            // Get the drop ListViewItem destination
            var listViewItem = WindowsUtils.FindAnchestor<ListViewItem>((DependencyObject)e.OriginalSource);
            if (listViewItem == null)
            {
                // Abort
                e.Effects = DragDropEffects.None;
                return;
            }
            // Find the data behind the ListViewItem
            var item = (FileItemViewModel)listView.ItemContainerGenerator.ItemFromContainer(listViewItem);
            var isInTheTop = IsDragInTheTopOfItem(e, listViewItem);

            // Move item into observable collection 
            // (this will be automatically reflected to lstView.ItemsSource)
            e.Effects = DragDropEffects.Move;
            int newIndex = ViewModel.Items.IndexOf(item);
            System.Diagnostics.Debug.WriteLine($"Moving index = {_selectedItemIndex} to = {newIndex}");
            if (_selectedItemIndex >= 0 &&
                newIndex >= 0 &&
                _selectedItemIndex != newIndex)
            {
                if (newIndex > _selectedItemIndex && isInTheTop)
                {
                    newIndex--;
                }
                else if (newIndex < _selectedItemIndex && !isInTheTop)
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
                ViewModel.Items.Move(_selectedItemIndex, newIndex);
            }
            _selectedItemIndex = -1;
        }

        private bool IsDragInTheTopOfItem(DragEventArgs e, ListViewItem item)
        {
            Point dragPoint = e.GetPosition(item);
            Rect bounds = VisualTreeHelper.GetDescendantBounds(item);
            return dragPoint.Y < bounds.Height / 2 && bounds.Contains(dragPoint);
        }

        private void HideAllSeparatorsLines()
        {
            var f = ViewModel.Items
                .Where(f => f.IsSeparatorBottomLineVisible || f.IsSeparatorTopLineVisible)
                .ToList();
            foreach (var file in f)
            {
                file.HideItemSeparators();
            }
        }
    }
}