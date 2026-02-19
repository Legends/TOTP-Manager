using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TOTP.Infrastructure.Converters
{
    [ValueConversion(typeof(string), typeof(Visibility))]
    public class StringToVisibilityConverter : IValueConverter
    {
        public bool Collapse { get; set; } = true;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var hasText = value is string s && !string.IsNullOrWhiteSpace(s);
            return hasText ? Visibility.Visible : Collapse ? Visibility.Collapsed : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}