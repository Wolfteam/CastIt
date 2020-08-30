using MaterialDesignThemes.Wpf.Converters;
using System.Windows;

namespace CastIt.Common.Converters
{
    public class InverseBooleanToVisibilityConverter : BooleanConverter<Visibility>
    {
        public InverseBooleanToVisibilityConverter()
            : base(Visibility.Collapsed, Visibility.Visible)
        {
        }
    }
}
