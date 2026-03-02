using System;
using System.Windows;
using System.Windows.Media;

namespace TOTP.Helper;

public class Common
{
    public static bool PackUriExists(string packUri)
    {
        try
        {
            var streamInfo = Application.GetResourceStream(new Uri(packUri));
            return streamInfo != null;
        }
        catch
        {
            return false;
        }
    }

    public static T? FindParent<T>(DependencyObject child) where T : DependencyObject
    {
        DependencyObject parent = VisualTreeHelper.GetParent(child);
        while (parent != null && parent is not T)
        {
            parent = VisualTreeHelper.GetParent(parent);
        }
        return parent as T;
    }

}
