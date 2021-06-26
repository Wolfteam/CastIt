using CastIt.Common.Miscellaneous;
using CastIt.ViewModels;
using CastIt.ViewModels.Items;
using Dragablz;
using MvvmCross.Platforms.Wpf.Views;

namespace CastIt.Views
{
    [CustomMvxContentPresentation(typeof(PlayListsViewModel), true, NoHistory = true)]
    public partial class PlayListsPage : MvxWpfView<PlayListsViewModel>
    {
        public PlayListsPage()
        {
            InitializeComponent();
            PlayListTabControl.AddHandler(DragablzItem.DragCompleted, new DragablzDragCompletedEventHandler(ItemDragCompleted), true);
        }

        private async void ItemDragCompleted(object sender, DragablzDragCompletedEventArgs e)
        {
            var vm = e.DragablzItem.DataContext as PlayListItemViewModel;
            var logicalIndex = e.DragablzItem.LogicalIndex;
            if (vm != null)
            {
                await MainWindow.MainViewModel.MovePlayList(logicalIndex, vm);
            }
        }
    }
}
