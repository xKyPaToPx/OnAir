using System;
using System.Globalization;
using System.Windows.Data;
using OnAir.Models;

namespace OnAir.Converters
{
    public class BroadcastItemTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is BroadcastItemType type)
            {
                return type switch
                {
                    BroadcastItemType.Default => "Стандартный",
                    BroadcastItemType.Advertising => "Реклама",
                    _ => type.ToString()
                };
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 