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
            //TODO: THIS ALMOST FIXES THE POPUP PROBLEM
            window.MouseMove += (sender, args) =>
            {
                if (SliderPopup.IsOpen)
                {
                    SliderPopup.IsOpen = false;
                }
            };
        }

        private void MainSilder_MouseLeave(object sender, MouseEventArgs e)
        {
            var mousePosition = e.GetPosition(this.MainSilder);
            var mouseRect = new Rect(mousePosition, new Size(5, 5));
            var sliderRect = LayoutInformation.GetLayoutSlot(this.MainSilder);
            bool intersect = sliderRect.IntersectsWith(mouseRect);
            if (!intersect)
            {
                //System.Diagnostics.Debug.WriteLine(
                //    $"rect does not intersect, closing this thing. " +
                //    $"ismouseoverslider = {this.Slider_2.IsMouseOver}");
                SliderPopup.IsOpen = false;
                //SliderPopup.HorizontalOffset = 0;
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

            var offset = mousePosition.X - (SliderPopup.Child as FrameworkElement).ActualWidth / 2;

            //System.Diagnostics.Debug.WriteLine($"Offset = {offset}");
            //System.Diagnostics.Debug.WriteLine($"MousePosition = {mousePosition.X} - SliderWidth = {MainSilder.ActualWidth}");

            SliderPopup.HorizontalOffset = offset;
            //SliderPopup.VerticalOffset = mousePosition.Y;
            e.Handled = true;
        }

        private void MainSilder_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            var mousePosition = e.GetPosition(this.MainSilder);
            var seconds = MainViewModel.GetMainProgressBarSeconds(MainSilder.ActualWidth, mousePosition.X);
            System.Diagnostics.Debug.WriteLine($"Going to seconds = {seconds}");
            MainViewModel.GoToSeconds(seconds);
        }
    }
}
