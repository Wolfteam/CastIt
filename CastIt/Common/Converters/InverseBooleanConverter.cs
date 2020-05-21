using System;
using System.Globalization;
using System.Windows.Data;

namespace CastIt.Common.Converters
{
    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool current = (bool)value;
            return !current;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool current = (bool)value;
            return !current;
        }
    }
}
