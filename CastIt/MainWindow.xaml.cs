using CastIt.Common;
using MvvmCross.Platforms.Wpf.Views;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace CastIt
{
    public partial class MainWindow : MvxWindow
    {
        private Storyboard _showWin;
        private Storyboard _hideWin;
        private bool _isCollapsed;

        public double CurrentWidth;
        public double CurrentHeight;

        public MainWindow()
        {
            InitializeComponent();
        }

        public void ToggleWindowHeight(double tabHeight)
        {
            if (!_isCollapsed)
            {
                BeginStoryboard(_hideWin);
                _hideWin.Begin();
            }
            else
            {
                (_showWin.Children.First() as DoubleAnimation).To = tabHeight + AppConstants.MinWindowHeight;
                _showWin.Begin();
            }
            _isCollapsed = !_isCollapsed;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void AppMainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _showWin = Resources["ShowWinStoryboard"] as Storyboard;
            _hideWin = Resources["HideWinStoryboard"] as Storyboard;
        }

        private void AppMainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            CurrentWidth = e.NewSize.Width;
            CurrentHeight = e.NewSize.Height;
        }
    }
}
