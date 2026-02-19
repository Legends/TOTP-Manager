using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TOTP.Infrastructure.Converters;

public class BoolToVisibilityConverterCollapsed : IValueConverter
{
    public bool Invert { get; set; } = false;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var val = value is bool b && b;
        if (Invert)
            val = !val;

        return val ? Visibility.Visible : Visibility.Collapsed; // ❗ returns Collapsed, not Hidden
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is Visibility v && v == Visibility.Visible;
    }
}