using System.Windows;
using System.Windows.Controls;

namespace CastIt.Views.Popups
{
    public class BasePopup : UserControl
    {
        public static DependencyProperty PlacementTargetProperty =
            DependencyProperty.Register(nameof(PlacementTarget),
                typeof(FrameworkElement),
                typeof(BasePopup),
                new PropertyMetadata(null));

        public static DependencyProperty IsOpenProperty =
            DependencyProperty.Register(nameof(IsOpen),
                typeof(bool),
                typeof(BasePopup),
                new FrameworkPropertyMetadata(false)
                {
                    BindsTwoWayByDefault = true
                });
        public FrameworkElement PlacementTarget
        {
            get => (FrameworkElement)GetValue(PlacementTargetProperty);
            set => SetValue(PlacementTargetProperty, value);
        }

        public bool IsOpen
        {
            get => (bool)GetValue(IsOpenProperty);
            set => SetValue(IsOpenProperty, value);
        }
    }
}
