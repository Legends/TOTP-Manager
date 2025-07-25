using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TOTP.Converters;

public class IntToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is int length ? length == 0 ? Visibility.Visible : Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}


