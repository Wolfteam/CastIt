using CastIt.ViewModels;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace CastIt.Views.UserControls
{
    public partial class WindowButtons : UserControl
    {
        public WindowButtons()
        {
            InitializeComponent();
        }

        private void Minimize_Clicked(object sender, RoutedEventArgs e)
        {
            var window = System.Windows.Application.Current.MainWindow;
            if (window == null)
                return;
            if (window.WindowState == WindowState.Normal ||
                window.WindowState == WindowState.Maximized)
            {
                window.WindowState = WindowState.Minimized;
            }
            else if (window.WindowState == WindowState.Minimized)
            {
                window.WindowState = WindowState.Normal;
            }
        }

        private void Maximize_Clicked(object sender, RoutedEventArgs e)
        {
            var window = System.Windows.Application.Current.MainWindow;
            if (window == null)
                return;
            if (window.WindowState == WindowState.Normal)
            {
                window.WindowState = WindowState.Maximized;
            }
            else if (window.WindowState == WindowState.Maximized)
            {
                window.WindowState = WindowState.Normal;
            }
        }

        public void CloseApp()
        {
            Dispatcher.Invoke(async () =>
            {
                var window = System.Windows.Application.Current.MainWindow as MainWindow;
                if (!(window?.Content is MainPage view))
                {
                    System.Windows.Application.Current.Shutdown();
                    return;
                }

                var vm = DataContext as MainViewModel;
                await vm!.SaveChangesBeforeClosing(window.CurrentWidth, window.CurrentHeight);
                view.ButtonsBar.DisposeViewModels();
                System.Windows.Application.Current.Shutdown();
            });
        }
    }
}
