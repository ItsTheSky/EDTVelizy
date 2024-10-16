using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace EDTVelizy.Viewer.Converters;

public class CourseTypeToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string type)
        {
            return type switch
            {
                "Cours Magistraux (CM)" => new SolidColorBrush(Color.Parse("#b91c1c")),
                "Travaux Dirigés (TD)" => new SolidColorBrush(Color.Parse("#15803d")),
                "Travaux Pratiques (TP)" => new SolidColorBrush(Color.Parse("#6d28d9")),
                _ => new SolidColorBrush(Color.Parse("#0e7490"))
            };
        }

        return new SolidColorBrush(Color.FromRgb(0, 50, 0));
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
    
    public static SolidColorBrush GetColorForType(string type)
    {
        return new CourseTypeToColorConverter().Convert(type, typeof(SolidColorBrush), null, CultureInfo.InvariantCulture) as SolidColorBrush ?? new SolidColorBrush(Color.FromRgb(0, 50, 0));
    }
}