using WOL.Models;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace WOL.Converters
{
    public class DeviceStatusColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DeviceStatus status)
            {
                switch (status)
                {
                    case DeviceStatus.Online:
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981"));
                    case DeviceStatus.Offline:
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6B7280"));
                    case DeviceStatus.Checking:
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B2B2B2"));
                }
            }

            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
