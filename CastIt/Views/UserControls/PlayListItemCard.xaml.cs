using CastIt.ViewModels.Items;
using MvvmCross.Binding.BindingContext;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CastIt.Views.UserControls
{
    public partial class PlayListItemCard : BasePlayListItem
    {
        public PlayListItemCard()
        {
            InitializeComponent();

            this.DelayBind(() =>
            {
                var set = this.CreateBindingSet<PlayListItemCard, PlayListItemViewModel>();
                set.Bind(this).For(v => v.OpenFileDialogRequest).To(vm => vm.OpenFileDialog).OneWay();
                set.Bind(this).For(v => v.OpenFolderDialogRequest).To(vm => vm.OpenFolderDialog).OneWay();
                set.Apply();
            });
        }

        private async void Control_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            await GoToPlayList();
        }

        private async void OnFabClick(object sender, System.Windows.RoutedEventArgs e)
        {
            await GoToPlayList();
        }

        private async Task GoToPlayList()
        {
            var window = System.Windows.Application.Current.MainWindow as MainWindow;
            if (window?.Content is not MainPage view)
                return;

            await view.ViewModel.GoToPlayList(Vm);
        }
    }
}
