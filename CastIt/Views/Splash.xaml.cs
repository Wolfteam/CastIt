using CastIt.Common;
using CastIt.Common.Miscellaneous;
using CastIt.Common.Utils;
using CastIt.ViewModels;
using MvvmCross.Platforms.Wpf.Views;
using MvvmCross.ViewModels;
using System;
using System.Windows;

namespace CastIt.Views
{
    [CustomMvxContentPresentation(typeof(SplashViewModel), NoHistory = true)]
    [MvxViewFor(typeof(SplashViewModel))]
    public partial class Splash : MvxWpfView<SplashViewModel>
    {
        private IMvxInteraction _beforeNavigatingToMainViewModelRequest;

        public IMvxInteraction BeforeNavigatingToMainViewModelRequest
        {
            get => _beforeNavigatingToMainViewModelRequest;
            set
            {
                if (_beforeNavigatingToMainViewModelRequest != null)
                    _beforeNavigatingToMainViewModelRequest.Requested -= BeforeNavigatingToMainViewModel;

                _beforeNavigatingToMainViewModelRequest = value;
                if (value != null)
                    _beforeNavigatingToMainViewModelRequest.Requested += BeforeNavigatingToMainViewModel;
            }
        }

        public Splash()
        {
            InitializeComponent();

            //Small values to make the splash  screen look good
            SetWindowWidthAndHeight(450, 300, false, false, false);

            var set = CreateBindingSet();
            set.Bind(this).For(v => v.BeforeNavigatingToMainViewModelRequest).To(vm => vm.BeforeNavigatingToMainViewModel).OneWay();
            set.Apply();
        }

        private void BeforeNavigatingToMainViewModel(object sender, EventArgs e)
        {
            var (width, height) = ViewModel.GetWindowWidthAndHeight();
            SetWindowWidthAndHeight(width, height, true, true, true);
        }

        private void SetWindowWidthAndHeight(double width, double height, bool canResize, bool updateMinSizes, bool showInTaskBar)
        {
            Dispatcher.Invoke(() =>
            {
                var window = System.Windows.Application.Current.MainWindow;
                window.Width = width;
                window.Height = height;
                if (updateMinSizes)
                {
                    window.MinWidth = AppConstants.MinWindowWidth;
                    window.MinHeight = AppConstants.MinWindowHeight;
                }

                window.ResizeMode = canResize ? ResizeMode.CanResize : ResizeMode.NoResize;
                window.ShowInTaskbar = showInTaskBar;

                WindowsUtils.CenterWindow(window);
            });
        }
    }
}
