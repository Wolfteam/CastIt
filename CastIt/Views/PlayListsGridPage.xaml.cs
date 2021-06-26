using CastIt.Common.Miscellaneous;
using CastIt.Common.Utils;
using CastIt.ViewModels;
using CastIt.ViewModels.Items;
using CastIt.Views.UserControls;
using MvvmCross.Platforms.Wpf.Views;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CastIt.Views
{
    [CustomMvxContentPresentation(typeof(PlayListsGridViewModel), true, NoHistory = true)]
    public partial class PlayListsGridPage : MvxWpfView<PlayListsGridViewModel>
    {
        private const string PlayListCardMoveFormat = "PlayListCardMoveFormat";

        public PlayListsGridPage()
        {
            InitializeComponent();
        }

        private void OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
                return;

            if (!(sender is ItemsControl))
                return;

            var card = WindowsUtils.FindAncestor<PlayListItemCard>((DependencyObject)e.OriginalSource);
            if (card?.Vm == null)
                return;

            var dragData = new DataObject(PlayListCardMoveFormat, card.Vm);
            DragDrop.DoDragDrop(card, dragData, DragDropEffects.Copy | DragDropEffects.Move);
        }

        private void OnDrop(object sender, DragEventArgs e)
        {
            if (sender != e.Source)
            {
                e.Effects = DragDropEffects.None;
                return;
            }

            if (!e.Data.GetDataPresent(PlayListCardMoveFormat))
            {
                e.Effects = DragDropEffects.None;
                return;
            }

            var mainVm = MainWindow.MainViewModel;

            var vm = e.Data.GetData(PlayListCardMoveFormat) as PlayListItemViewModel;
            var targetCard = WindowsUtils.FindAncestor<PlayListItemCard>((DependencyObject)e.OriginalSource);
            if (targetCard == null || vm == null)
            {
                e.Effects = DragDropEffects.None;
                return;
            }

            e.Effects = DragDropEffects.Move;

            mainVm.MovePlayList(vm, targetCard.Vm);
        }
    }
}
