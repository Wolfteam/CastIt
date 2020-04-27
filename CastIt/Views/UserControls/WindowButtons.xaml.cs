using System;
using System.Windows;
using System.Windows.Controls;

namespace CastIt.Views.UserControls
{
    /// <summary>
    /// Interaction logic for WindowButtons.xaml
    /// </summary>
    public partial class WindowButtons : UserControl
    {
        public WindowButtons()
        {
            InitializeComponent();
        }

        private void Minimize_Clicked(object sender, RoutedEventArgs e)
        {
            var window = System.Windows.Application.Current.MainWindow;
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
            var window =  System.Windows.Application.Current.MainWindow;
            if (window.WindowState == WindowState.Normal)
            {
                window.WindowState = WindowState.Maximized;
            }
            else if (window.WindowState == WindowState.Maximized)
            {
                window.WindowState = WindowState.Normal;
            }
        }

        private void Exit_Clicked(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }
    }
}
