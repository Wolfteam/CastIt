using System.Windows;
using System.Windows.Controls;

namespace CastIt.Views.Popups
{
    public partial class RenamePlayListPopup : BasePopup
    {
        public static DependencyProperty CurrentNameProperty =
            DependencyProperty.Register(
                nameof(CurrentName),
                typeof(string),
                typeof(RenamePlayListPopup),
                new PropertyMetadata(string.Empty));

        public string CurrentName
        {
            get => (string)GetValue(CurrentNameProperty);
            set => SetValue(CurrentNameProperty, value);
        }

        public RenamePlayListPopup()
        {
            InitializeComponent();
        }
    }
}
