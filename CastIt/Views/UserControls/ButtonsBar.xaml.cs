using CastIt.Common;
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

        private void ToggleCollapse(object sender, RoutedEventArgs e)
        {
            var window = System.Windows.Application.Current.MainWindow as MainWindow;
            var view = window.Content as MainPage;
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
