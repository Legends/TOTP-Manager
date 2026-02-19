using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TOTP.Infrastructure.Converters
{
    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool b ? !b : DependencyProperty.UnsetValue;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool b ? !b : DependencyProperty.UnsetValue;
    }

}
