using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace EDTVelizy.Viewer.Converters;

public class StringLimiterConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string str || parameter is not int limit) 
            return value;
        
        if (str.Length > limit)
        {
            return str[..limit] + "...";
        }

        return value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}