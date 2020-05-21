using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace CastIt.Common.Converters
{
    public class CenterPopUpConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.FirstOrDefault(v => v == DependencyProperty.UnsetValue) != null)
            {
                return double.NaN;
            }
            bool positive = bool.Parse(parameter as string);
            double placementTargetWidth = (double)values[0];
            double toolTipWidth = (double)values[1];
            return ((placementTargetWidth / 2.0) - (toolTipWidth / 2.0)) * (positive ? 1 : -1);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
