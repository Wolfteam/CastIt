using CastIt.ViewModels;
using MvvmCross;
using MvvmCross.Platforms.Wpf.Views;

namespace CastIt.Views.UserControls
{
    public partial class DiscoveredDevices : MvxWpfView<DevicesViewModel>
    {
        public DiscoveredDevices()
        {
            InitializeComponent();
            ViewModel = Mvx.IoCProvider.IoCConstruct<DevicesViewModel>();
        }
    }
}
