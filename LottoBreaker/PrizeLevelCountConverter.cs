using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace LottoBreaker.Converters
{
    public class PrizeLevelCountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count)
            {
                return count > 1 ? $"{count} Levels" : "1 Level"; // Simplified logic
            }
            return "Multiple Levels"; // Default case if value isn't an int or is null
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException(); // We don't need to convert back for this use case
        }
    }
}