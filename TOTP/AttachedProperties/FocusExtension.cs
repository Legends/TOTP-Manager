using System;
using System.Windows;
using System.Windows.Input;

namespace Github2FA.AttachedProperties;


public static class FocusExtension
{
    public static readonly DependencyProperty IsFocusedProperty =
        DependencyProperty.RegisterAttached(
            "IsFocused",
            typeof(bool),
            typeof(FocusExtension),
            new UIPropertyMetadata(false, OnIsFocusedPropertyChanged));

    public static bool GetIsFocused(DependencyObject obj)
    {
        return (bool)obj.GetValue(IsFocusedProperty);
    }

    public static void SetIsFocused(DependencyObject obj, bool value)
    {
        obj.SetValue(IsFocusedProperty, value);
    }

    private static void OnIsFocusedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var uiElement = d as UIElement;
        if (uiElement == null || !(bool)e.NewValue)
            return;

        uiElement.Dispatcher.BeginInvoke(
            new Action(() =>
            {
                uiElement.Focus();
                Keyboard.Focus(uiElement);
            }),
            System.Windows.Threading.DispatcherPriority.Input);
    }
}