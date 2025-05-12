using System;
using System.Globalization;
using System.Windows.Data;
using OnAir.Models;

namespace OnAir.Converters
{
    public class BroadcastToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is BroadcastItem item)
            {
                return $"{item.Title} — {item.Description} — {item.Duration:hh\\:mm}";
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 