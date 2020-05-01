using CastIt.ViewModels;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Platforms.Wpf.Views;
using MvvmCross.ViewModels;
using System.Windows;

namespace CastIt.Views
{
    [MvxViewFor(typeof(MainViewModel))]
    public partial class MainPage : MvxWpfView<MainViewModel>
    {
        private double _currentTabHeight;
        //TODO: IF YOU DRAG OUT OF THE WINDOW, THE SEPARATORS ARE SHOWN

        private IMvxInteraction _closeAppRequest;
        public IMvxInteraction CloseAppRequest
        {
            get => _closeAppRequest;
            set
            {
                if (_closeAppRequest != null)
                    _closeAppRequest.Requested -= (sender, args) => WindowButtons.CloseApp();

                _closeAppRequest = value;
                if (value != null)
                    _closeAppRequest.Requested += (sender, args) => WindowButtons.CloseApp();
            }
        }

        private IMvxInteraction<(double, double)> _setWindowWithAndHeightRequest;
        public IMvxInteraction<(double, double)> SetWindowWithAndHeightRequest
        {
            get => _setWindowWithAndHeightRequest;
            set
            {
                if (_setWindowWithAndHeightRequest != null)
                    _setWindowWithAndHeightRequest.Requested -= (sender, args)
                        => SetWindowWidthAndHeight(args.Value.Item1, args.Value.Item2);

                _setWindowWithAndHeightRequest = value;
                if (value != null)
                    _setWindowWithAndHeightRequest.Requested += (sender, args)
                        => SetWindowWidthAndHeight(args.Value.Item1, args.Value.Item2);
            }
        }

        public MainPage()
        {
            InitializeComponent();

            var set = this.CreateBindingSet<MainPage, MainViewModel>();
            set.Bind(this).For(v => v.CloseAppRequest).To(vm => vm.CloseApp).OneWay();
            set.Bind(this).For(v => v.SetWindowWithAndHeightRequest).To(vm => vm.SetWindowWidthAndHeight).OneWay();
            set.Apply();
        }

        private void SetWindowWidthAndHeight(double width, double height)
        {
            var window = System.Windows.Application.Current.MainWindow;
            window.Width = width;
            window.Height = height;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //TODO: IMPROVE THIS PIECE OF CODE
            var window = System.Windows.Application.Current.MainWindow as MainWindow;
            var currentTab = PlayListTabControl;
            ViewModel.IsExpanded = !ViewModel.IsExpanded;
            if (_currentTabHeight < currentTab.ActualHeight)
                _currentTabHeight = currentTab.ActualHeight;
            window.ToggleWindowHeight(_currentTabHeight);
        }
    }
}
