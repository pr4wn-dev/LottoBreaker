using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace LottoBreaker;
public class DebugValueConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string strValue)
        {
            System.Diagnostics.Debug.WriteLine($"Converting value: {strValue}");
            return strValue; // Return the original value or modify if needed
        }
        return "N/A";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}