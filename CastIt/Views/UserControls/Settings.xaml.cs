using CastIt.ViewModels;
using MvvmCross;
using MvvmCross.Base;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Platforms.Wpf.Views;
using MvvmCross.ViewModels;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace CastIt.Views.UserControls
{
    public partial class Settings : MvxWpfView<SettingsViewModel>
    {
        private IMvxInteraction<string> _changeAccentColorRequest;
        public IMvxInteraction<string> ChangeAccentColorRequest
        {
            get => _changeAccentColorRequest;
            set
            {
                if (_changeAccentColorRequest != null)
                    _changeAccentColorRequest.Requested -= ChangeSelectedAccentColor;

                _changeAccentColorRequest = value;
                if (value != null)
                    _changeAccentColorRequest.Requested += ChangeSelectedAccentColor;
            }
        }

        public Settings()
        {
            InitializeComponent();

            ViewModel = Mvx.IoCProvider.Resolve<SettingsViewModel>();

            var set = this.CreateBindingSet<Settings, SettingsViewModel>();
            set.Bind(this).For(v => v.ChangeAccentColorRequest).To(vm => vm.ChangeSelectedAccentColor).OneWay();
            set.Apply();

            ChangeSelectedAccentColor(ViewModel.CurrentAccentColor);

            Loaded += (sender, args) => ChangeSelectedAccentColor(ViewModel.CurrentAccentColor);
        }

        private void ChangeSelectedAccentColor(object sender, MvxValueEventArgs<string> e)
        {
            string hexColor = e.Value;
            ChangeSelectedAccentColor(hexColor);
        }

        private void ChangeSelectedAccentColor(string hexColor)
        {
            foreach (var t in AccentColorsIc.Items)
            {
                var c = (ContentPresenter)AccentColorsIc.ItemContainerGenerator.ContainerFromItem(t);
                var tb = c?.ContentTemplate.FindName("AccentColorButton", c) as ToggleButton;
                if (tb == null)
                    continue;
                tb.IsChecked = (string)tb.DataContext == hexColor;
            }
        }
    }
}
