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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //TODO: IMPROVE THIS PIECE OF CODE
            //var window = System.Windows.Application.Current.MainWindow as MainWindow;
            //var view = window.Content as MainPage;
            //var currentTab = view.PlayListTabControl;
            //ViewModel.IsExpanded = !ViewModel.IsExpanded;
            //if (ViewModel.IsExpanded && currentTab.ActualHeight < 50)
            //{
            //    _currentTabHeight = AppConstants.MinWindowHeight;
            //}

            //if (_currentTabHeight < currentTab.ActualHeight)
            //    _currentTabHeight = currentTab.ActualHeight;
            //window.ToggleWindowHeight(_currentTabHeight);
        }
    }
}
