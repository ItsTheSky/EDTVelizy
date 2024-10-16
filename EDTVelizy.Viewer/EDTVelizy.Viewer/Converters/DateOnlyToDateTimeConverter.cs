using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace EDTVelizy.Viewer.Converters;

public class DateOnlyToDateTimeConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DateOnly date)
        {
            return new DateTimeOffset(date.ToDateTime(new TimeOnly()));
        }
        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DateTimeOffset dateTime)
        {
            return DateOnly.FromDateTime(dateTime.DateTime);
        }
        return null;
    }
}