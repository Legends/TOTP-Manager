using System;
using System.Globalization;
using System.Windows.Data;

namespace TOTP.Infrastructure.Converters;

public sealed class MultiplyConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (!double.TryParse(value?.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var input))
        {
            return Binding.DoNothing;
        }

        if (!double.TryParse(parameter?.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var factor))
        {
            factor = 1d;
        }

        return input * factor;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => Binding.DoNothing;
}
