using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace WOL.Converters
{
    public class StatusColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count)
            {
                return count > 0 ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Gray);
            }
            
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 