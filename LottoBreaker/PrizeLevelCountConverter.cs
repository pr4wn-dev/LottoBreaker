using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace LottoBreaker.Converters
{
    public class PrizeLevelCountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count && count > 1)
            {
                return $"{count} Levels";
            }
            return "1 Level";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException(); // We don't need to convert back for this use case
        }
    }
}