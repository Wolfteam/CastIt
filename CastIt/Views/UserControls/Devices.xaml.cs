using CastIt.ViewModels;
using MvvmCross;
using MvvmCross.Platforms.Wpf.Views;

namespace CastIt.Views.UserControls
{
    public partial class Devices : MvxWpfView<DevicesViewModel>
    {
        public Devices()
        {
            InitializeComponent();
            ViewModel = Mvx.IoCProvider.Resolve<DevicesViewModel>();
        }
    }
}
