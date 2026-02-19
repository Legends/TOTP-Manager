using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace TOTP.Infrastructure.Converters;

public sealed class ProgressValueToBrushConverter : IValueConverter
{
    private static readonly SolidColorBrush SoftRed = new((Color)ColorConverter.ConvertFromString("#E16C6C"));
    // maximum = 30
    // 0..19  => lime green
    // 20..24 => orange
    // 25..30 => red
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var v = 0.0;

        if (value is double d) v = d;
        else if (value is float f) v = f;
        else if (value is int i) v = i;
        else if (value != null && double.TryParse(value.ToString(), out var parsed)) v = parsed;

        return v >= 25 ? SoftRed : v >= 20 ? Brushes.Orange : Brushes.LimeGreen;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => Binding.DoNothing;
}