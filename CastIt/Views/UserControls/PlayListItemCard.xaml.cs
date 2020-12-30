using CastIt.ViewModels.Items;
using MvvmCross.Platforms.Wpf.Views;
using System.Windows.Input;

namespace CastIt.Views.UserControls
{
    public partial class PlayListItemCard : MvxWpfView
    {
        public PlayListItemCard()
        {
            InitializeComponent();
        }

        private async void Control_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var window = System.Windows.Application.Current.MainWindow as MainWindow;
            if (!(window?.Content is MainPage view))
                return;

            var vm = DataContext as PlayListItemViewModel;
            await view.ViewModel.GoToPlayList(vm);
        }
    }
}
