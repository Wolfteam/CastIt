using CastIt.Application.Common;
using CastIt.Application.Server;
using CastIt.Domain.Extensions;
using CastIt.ViewModels;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

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
            if (window == null)
                throw new Exception("Window should not be null");
            window.MouseMove += (sender, args) => ClosePopUpIfOpened();
            window.Deactivated += (sender, args) => ClosePopUpIfOpened();
        }

        private void MainSlider_MouseLeave(object sender, MouseEventArgs e)
        {
            var mousePosition = e.GetPosition(MainSlider);
            var mouseRect = new Rect(mousePosition, new Size(5, 5));
            var sliderRect = LayoutInformation.GetLayoutSlot(MainSlider);
            bool intersect = sliderRect.IntersectsWith(mouseRect);
            if (!intersect)
            {
                SliderPopup.IsOpen = false;
            }
            e.Handled = true;
        }

        private void MainSlider_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                SliderPopup.IsOpen = false;
                return;
            }
            if (!SliderPopup.IsOpen)
                SliderPopup.IsOpen = true;

            var mousePosition = e.GetPosition(MainSlider);
            var seconds = MainViewModel.GetMainProgressBarSecondsForThumbnails(MainSlider.ActualWidth, mousePosition.X);
            MainViewModel.SetPreviewThumbnailImage(seconds);

            if (seconds >= 0)
            {
                SliderPopupText.Text = TimeSpan.FromSeconds(seconds).ToString(FileFormatConstants.FullElapsedTimeFormat);
                SliderPopup.HorizontalOffset = mousePosition.X - (SliderPopup.Child as FrameworkElement)?.ActualWidth / 2 ?? 0;
                if (!(ImageThumbnail.Transform is TransformGroup))
                {
                    ImageThumbnail.Transform = new TransformGroup
                    {
                        Children = new TransformCollection(2)
                    };
                }
                var group = ImageThumbnail.Transform as TransformGroup;

                //The used transforms only applies to local video files
                if (MainViewModel.CurrentPlayedFile.Type.IsLocalVideo())
                {
                    var (x, y) = MainViewModel.GetPreviewThumbnailCoordinates(seconds);
                    if (!group!.Children.Any())
                    {
                        group.Children.Add(new ScaleTransform(AppWebServerConstants.ThumbnailsPerImageRow, AppWebServerConstants.ThumbnailsPerImageRow));
                        group.Children.Add(new MatrixTransform(1, 0, 0, 1, -x, -y));
                    }
                    else
                    {
                        var mt = group!.Children.Last() as MatrixTransform;
                        mt!.Matrix = new Matrix(1, 0, 0, 1, -x, -y);
                    }
                }
                else
                {
                    group?.Children.Clear();
                }
            }

            e.Handled = true;
        }

        private void MainSlider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            var mousePosition = e.GetPosition(MainSlider);
            var seconds = MainViewModel.GetMainProgressBarSeconds(MainSlider.ActualWidth, mousePosition.X);
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
