using Avalonia.Media;

namespace EDTVelizy.Viewer;

public static class Extensions
{
    
    public static Color Darken(this Color color, double factor)
    {
        return Color.FromArgb(color.A, (byte)(color.R * factor), (byte)(color.G * factor), (byte)(color.B * factor));
    }
    
    public static IBrush Darken(this IBrush brush, double factor)
    {
        if (brush is SolidColorBrush solidColorBrush)
            return new SolidColorBrush(solidColorBrush.Color.Darken(factor));
        return brush;
    }
    
    public static Color ToAvaloniaColor(this System.Drawing.Color color)
    {
        return Color.FromArgb(color.A, color.R, color.G, color.B);
    }
    
    public static string Limit(this string str, int maxLength)
    {
        if (str.Length <= maxLength)
            return str;
        return str[..(maxLength - 3)] + "...";
    }
}