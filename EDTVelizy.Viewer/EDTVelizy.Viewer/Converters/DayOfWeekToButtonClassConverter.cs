using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using EDTVelizy.Viewer.Views;

namespace EDTVelizy.Viewer.Converters;

public class DayOfWeekToButtonClassConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DayOfWeek dayOfWeek)
        {
            if (MainView.Instance.ViewModel.SelectedDate.DayOfWeek == dayOfWeek)
            {
                return Application.Current?.FindResource("SolidButton");
            }

            return Application.Current?.FindResource("OutlineButton");
        }

        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}