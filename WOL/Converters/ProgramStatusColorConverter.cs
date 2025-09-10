using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using WOL.Models;

namespace WOL.Converters
{
    class ProgramStatusColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ProgramStatus status)
            {
                switch (status)
                {
                    case ProgramStatus.Running:
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981"));
                    case ProgramStatus.Stopped:
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6B7280"));
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
