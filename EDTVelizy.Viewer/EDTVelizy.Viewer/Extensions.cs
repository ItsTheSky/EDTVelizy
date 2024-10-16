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
    
}