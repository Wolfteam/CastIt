using CastIt.Common;
using CastIt.Common.Utils;
using CastIt.Views;
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

        public double CurrentWidth;
        public double CurrentHeight;

        public MainWindow()
        {
            InitializeComponent();

            MinimizeToTray.Enable(this);
        }

        public void BringToForeground()
        {
            if (WindowState == WindowState.Minimized || Visibility == Visibility.Hidden)
            {
                Show();
                WindowState = WindowState.Normal;
            }

            // According to some sources these steps gurantee that an app will be brought to foreground.
            Activate();
            Topmost = true;
            Topmost = false;
            Focus();
        }

        public void ToggleWindowHeight(double tabHeight)
        {
            var view = Content as MainPage;
            //when this method gets called, the IsExpanded property already changed
            //thats why we negate its value
            bool collapse = !view.ViewModel.IsExpanded;
            if (collapse)
            {
                BeginStoryboard(_hideWin);
                _hideWin.Begin();
            }
            else
            {
                (_showWin.Children.First() as DoubleAnimation).To = tabHeight + AppConstants.MinWindowHeight;
                _showWin.Begin();
            }
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
            var view = Content as MainPage;
            if (view == null)
                return;

            if (e.NewSize.Height <= AppConstants.MinWindowHeight &&
                view.ViewModel.IsExpanded)
            {
                view.ViewModel.IsExpanded = false;
            }
            else if (e.NewSize.Height > AppConstants.MinWindowHeight &&
                !view.ViewModel.IsExpanded)
            {
                view.ViewModel.IsExpanded = true;
            }
            CurrentWidth = e.NewSize.Width;
            CurrentHeight = e.NewSize.Height;
        }
    }
}
