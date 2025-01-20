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
                return count > 1 ? $"{count} Levels" : "1 Level";
            }
            return "1 Level"; // Default to one level if the count is null or not an int
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}