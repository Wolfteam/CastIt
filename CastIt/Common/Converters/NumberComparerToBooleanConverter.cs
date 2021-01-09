using System;
using System.Globalization;
using System.Windows.Data;

namespace CastIt.Common.Converters
{
    public class NumberComparerToBooleanConverter : IValueConverter
    {
        public bool GreaterThan { get; set; }
        public bool GreaterThanOrEqual { get; set; }
        public bool Equal { get; set; }
        public bool LessThan { get; set; }
        public bool LessThanOrEqual { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var currentItems = System.Convert.ToInt32(value);
            var expected = System.Convert.ToInt32(parameter);

            if (GreaterThan)
                return currentItems > expected;

            if (GreaterThanOrEqual)
                return currentItems >= expected;

            if (Equal)
                return currentItems == expected;

            if (LessThan)
                return currentItems < expected;

            if (LessThanOrEqual)
                return currentItems <= expected;

            throw new InvalidOperationException("You need to set one of the converters properties");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
