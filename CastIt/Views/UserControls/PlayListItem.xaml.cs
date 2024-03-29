﻿using CastIt.Common.Utils;
using CastIt.ViewModels.Items;
using MvvmCross.Base;
using MvvmCross.Binding.BindingContext;
using MvvmCross.ViewModels;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace CastIt.Views.UserControls
{
    public partial class PlayListItem : BasePlayListItem
    {
        private Point _moveStartPoint;
        private int _selectedItemIndex = -1;
        private bool _loaded;
        private const string PlaylistItemMoveFormat = "PlaylistItemMoveFormat";

        private IMvxInteraction<FileItemViewModel> _scrollToSelectedItemRequest;
        private IMvxInteraction _selectAllRequest;

        public IMvxInteraction<FileItemViewModel> ScrollToSelectedItemRequest
        {
            get => _scrollToSelectedItemRequest;
            set
            {
                _scrollToSelectedItemRequest = value;
                if (value != null)
                {
                    Disposables.Add(_scrollToSelectedItemRequest.WeakSubscribe(ScrollToSelectedItem));
                }
            }
        }

        public IMvxInteraction SelectAllItemsRequest
        {
            get => _selectAllRequest;
            set
            {
                _selectAllRequest = value;
                if (value != null)
                {
                    Disposables.Add(_selectAllRequest.WeakSubscribe(SelectAllItems));
                }
            }
        }

        public PlayListItem()
        {
            InitializeComponent();
            this.DelayBind(() =>
            {
                var set = this.CreateBindingSet<PlayListItem, PlayListItemViewModel>();
                set.Bind(this).For(v => v.OpenFileDialogRequest).To(vm => vm.OpenFileDialog).OneWay();
                set.Bind(this).For(v => v.OpenFolderDialogRequest).To(vm => vm.OpenFolderDialog).OneWay();
                set.Bind(this).For(v => v.ScrollToSelectedItemRequest).To(vm => vm.ScrollToSelectedItem).OneWay();
                set.Bind(this).For(v => v.SelectAllItemsRequest).To(vm => vm.SelectAllItems).OneWay();
                set.Apply();
            });
        }

        public override void OnLoaded(object sender, RoutedEventArgs e)
        {
            base.OnLoaded(sender, e);
            var view = CollectionViewSource.GetDefaultView(PlaylistLv.ItemsSource);
            view.Filter = FilterFiles;
            _loaded = true;
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
            if (!(sender is ListView listView))
                return;
            var listViewItem = WindowsUtils.FindAncestor<ListViewItem>((DependencyObject)e.OriginalSource);
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
            if (Vm == null)
                return;

            HideAllSeparatorsLines();
            if (sender != e.Source)
                return;

            //MouseDoubleClick event on the card item triggers the drop event, this is a workaround for that
            if (!_loaded)
                return;

            if (e.Data.GetDataPresent(PlaylistItemMoveFormat))
            {
                HandlePlaylistItemMove(sender as ListView, e);
            }
            else if (e.Data.GetDataPresent(DataFormats.FileDrop) &&
                e.Data.GetData(DataFormats.FileDrop) is string[] items &&
                items.Length > 0)
            {
                var folders = items
                    .Where(f => (File.GetAttributes(f) & FileAttributes.Directory) != 0)
                    .ToArray();

                var files = items
                    .Where(f => (File.GetAttributes(f) & FileAttributes.Directory) == 0)
                    .ToArray();

                if (folders.Any())
                    Vm.OnFolderAddedCommand.Execute(folders);

                if (files.Any())
                    Vm.OnFilesAddedCommand.Execute(files);
            }
        }

        private bool DragIsBeyondMinimum(Vector diff)
        {
            return Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance;
        }

        private void ToggleSeparatorLine(ListView listView, DragEventArgs e)
        {
            var listViewItem = WindowsUtils.FindAncestor<ListViewItem>((DependencyObject)e.OriginalSource);
            if (listViewItem == null || Vm == null)
            {
                return;
            }
            var isInTheTop = IsDragInTheTopOfItem(e, listViewItem);
            var item = (FileItemViewModel)listView.ItemContainerGenerator.ItemFromContainer(listViewItem);
            int index = Vm.Items.IndexOf(item);
            HideAllSeparatorsLines();
            if (index != _selectedItemIndex)
            {
                bool showBottom = index == Vm.Items.Count - 1 && !isInTheTop;
                bool showTop = !showBottom && isInTheTop;
                item.ShowItemSeparators(showTop, showBottom);
                //item.ShowItemSeparators(isInTheTop, !isInTheTop);
            }
        }

        private void HandlePlaylistItemMove(ListView listView, DragEventArgs e)
        {
            // Get the drop ListViewItem destination
            var listViewItem = WindowsUtils.FindAncestor<ListViewItem>((DependencyObject)e.OriginalSource);
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
            int newIndex = Vm.Items.IndexOf(item);

            Vm.MoveFile(_selectedItemIndex, newIndex, isInTheTop);
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
            var f = Vm.Items
                .Where(item => item.IsSeparatorBottomLineVisible || item.IsSeparatorTopLineVisible)
                .ToList();
            foreach (var file in f)
            {
                file.HideItemSeparators();
            }
        }

        private bool FilterFiles(object item)
        {
            if (string.IsNullOrEmpty(PlayListFilter.Text))
                return true;

            var vm = item as FileItemViewModel;
            return vm.Filename.Contains(PlayListFilter.Text, StringComparison.OrdinalIgnoreCase);
        }

        private void PlayListFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            CollectionViewSource.GetDefaultView(PlaylistLv.ItemsSource)?.Refresh();
        }

        private void ScrollToSelectedItem(object sender, MvxValueEventArgs<FileItemViewModel> e)
        {
            if (e.Value != null)
                Dispatcher.Invoke(() => PlaylistLv.ScrollIntoView(e.Value));
        }

        private void SelectAllItems(object sender, EventArgs e)
        {
            PlaylistLv.SelectedItems.Clear();

            foreach (var item in Vm.Items)
            {
                PlaylistLv.SelectedItems.Add(item);
            }
        }

        private void VolumeSlider_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            MainWindow.MainViewModel.VolumeLevelSliderIsBeingMoved = true;
        }

        private void VolumeSlider_OnPreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            MainWindow.MainViewModel.SetVolumeCommand.Execute();
            MainWindow.MainViewModel.VolumeLevelSliderIsBeingMoved = false;
        }
    }
}
