using CastIt.ViewModels;
using MvvmCross;
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
                    _changeAccentColorRequest.Requested -= (sender, args) => ChangeSelectedAccentColor(args.Value);

                _changeAccentColorRequest = value;
                if (value != null)
                    _changeAccentColorRequest.Requested += (sender, args) => ChangeSelectedAccentColor(args.Value);
            }
        }

        public Settings()
        {
            InitializeComponent();

            ViewModel = Mvx.IoCProvider.IoCConstruct<SettingsViewModel>();

            var set = this.CreateBindingSet<Settings, SettingsViewModel>();
            set.Bind(this).For(v => v.ChangeAccentColorRequest).To(vm => vm.ChangeSelectedAccentColor).OneWay();
            set.Apply();

            ChangeSelectedAccentColor(ViewModel.CurrentAccentColor);

            Loaded += (sender, args) => ChangeSelectedAccentColor(ViewModel.CurrentAccentColor);
        }

        private void ChangeSelectedAccentColor(string hexColor)
        {
            for (int i = 0; i < AccentColorsIc.Items.Count; i++)
            {
                var c = (ContentPresenter)AccentColorsIc.ItemContainerGenerator.ContainerFromItem(AccentColorsIc.Items[i]);
                if (c is null)
                    continue;
                var tb = c.ContentTemplate.FindName("AccentColorButton", c) as ToggleButton;
                tb.IsChecked = (string)tb.DataContext == hexColor;
            }
        }
    }
}
