using System;
using System.Globalization;
using System.Windows.Data;

namespace TOTP.Infrastructure.Converters;

public class SecretMaskConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var s = value as string;
        return string.IsNullOrEmpty(s) ? string.Empty : new string('*', s.Length);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}



