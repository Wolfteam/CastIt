using CastIt.Common;
using CastIt.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace CastIt.Views.UserControls
{
    public partial class MainProgressBar : UserControl
    {
        public MainViewModel MainViewModel
            => DataContext as MainViewModel;
        public MainProgressBar()
        {
            InitializeComponent();

            var window = System.Windows.Application.Current.MainWindow;
            window.MouseMove += (sender, args) => ClosePopUpIfOpened();
            window.Deactivated += (sender, args) => ClosePopUpIfOpened();
        }

        private void MainSilder_MouseLeave(object sender, MouseEventArgs e)
        {
            var mousePosition = e.GetPosition(this.MainSilder);
            var mouseRect = new Rect(mousePosition, new Size(5, 5));
            var sliderRect = LayoutInformation.GetLayoutSlot(this.MainSilder);
            bool intersect = sliderRect.IntersectsWith(mouseRect);
            if (!intersect)
            {
                SliderPopup.IsOpen = false;
            }
            e.Handled = true;
        }

        private void MainSilder_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                SliderPopup.IsOpen = false;
                return;
            }
            if (!SliderPopup.IsOpen)
                SliderPopup.IsOpen = true;

            var mousePosition = e.GetPosition(this.MainSilder);
            var seconds = MainViewModel.TrySetThumbnail(MainSilder.ActualWidth, mousePosition.X);
            if (seconds >= 0)
                SliderPopupText.Text = TimeSpan.FromSeconds(seconds).ToString(AppConstants.FullElapsedTimeFormat);

            SliderPopup.HorizontalOffset = mousePosition.X - (SliderPopup.Child as FrameworkElement).ActualWidth / 2;
            e.Handled = true;
        }

        private void MainSilder_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            var mousePosition = e.GetPosition(this.MainSilder);
            var seconds = MainViewModel.GetMainProgressBarSeconds(MainSilder.ActualWidth, mousePosition.X);
            MainViewModel.GoToSecondsCommand.Execute(seconds);
        }

        private void ClosePopUpIfOpened()
        {
            if (SliderPopup.IsOpen)
            {
                SliderPopup.IsOpen = false;
            }
        }
    }
}
