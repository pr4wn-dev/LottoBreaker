using System;
using System.Globalization;
using Microsoft.Maui.Controls;


namespace LottoBreaker
{
    public class WinningChanceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string chanceString)
            {
                if (chanceString == "N/A")
                {
                    return chanceString; // Keep N/A as is
                }
                else if (double.TryParse(chanceString.Split(' ')[0], out double chance) && chance > 1e6) // Assuming 1 million is 'extremely low'
                {
                    return "Extremely Low"; // If the chance is very high (low probability)
                }
                return chanceString; // Return the original string if it's neither N/A nor extremely low
            }
            return "N/A"; // Default case
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}