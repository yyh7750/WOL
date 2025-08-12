using System;
using System.Globalization;
using System.Windows.Data;

namespace WOL.Converters
{
    public class EqualityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2)
            {
                return false;
            }

            if (values[0] == null || values[1] == null)
            {
                return object.Equals(values[0], values[1]);
            }

            return values[0].Equals(values[1]);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
