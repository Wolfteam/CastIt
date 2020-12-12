using CastIt.Common;
using CastIt.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace CastIt.Views.UserControls
{
    public partial class ButtonsBar : UserControl
    {
        private double _currentTabHeight;
        public ButtonsBar()
        {
            InitializeComponent();
        }

        public void DisposeViewModels()
        {
            var devicesVm = DevicesUserControl.DataContext as DevicesViewModel;
            devicesVm?.CleanUp();
        }

        private void ToggleCollapse(object sender, RoutedEventArgs e)
        {
            var window = System.Windows.Application.Current.MainWindow as MainWindow;
            if (!(window?.Content is MainPage view))
                return;
            var currentTab = view.PlayListTabControl;
            view.ViewModel.IsExpanded = !view.ViewModel.IsExpanded;
            if (view.ViewModel.IsExpanded && _currentTabHeight < 50)
            {
                _currentTabHeight = AppConstants.MinWindowHeight;
            }

            if (currentTab.ActualHeight >= 50)
                _currentTabHeight = currentTab.ActualHeight;
            window.ToggleWindowHeight(_currentTabHeight);
        }
    }
}
