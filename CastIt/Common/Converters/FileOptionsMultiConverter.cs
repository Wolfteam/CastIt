using System;
using System.Globalization;
using System.Windows.Data;

namespace CastIt.Common.Converters
{
    public class FileOptionsMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2)
                throw new ArgumentOutOfRangeException("The provided values must be at least two");
            int count = System.Convert.ToInt32(values[0]);
            bool isPlaying = (bool)values[1];
            return isPlaying && count > 0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
