using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace TOTP.Infrastructure.AttachedProperties;

public static class FocusTriggerExtension
{
    public static readonly DependencyProperty FocusTriggerProperty =
        DependencyProperty.RegisterAttached(
            "FocusTrigger",
            typeof(object),
            typeof(FocusTriggerExtension),
            new PropertyMetadata(null, OnFocusTriggerChanged));

    public static object GetFocusTrigger(DependencyObject obj) => obj.GetValue(FocusTriggerProperty);
    public static void SetFocusTrigger(DependencyObject obj, object value) => obj.SetValue(FocusTriggerProperty, value);

    private static void OnFocusTriggerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not IInputElement inputElement)
            return;

        void FocusNow()
        {
            // FocusManager/Keyboard focus
            Keyboard.Focus(inputElement);

            // If it’s also a UIElement, call Focus() too (sets logical focus)
            if (inputElement is UIElement ui)
                ui.Focus();
        }

        // If it’s a FrameworkElement, we can reliably wait for Loaded.
        if (d is FrameworkElement fe)
        {
            if (fe.IsLoaded)
            {
                fe.Dispatcher.BeginInvoke((Action)FocusNow, DispatcherPriority.Input);
            }
            else
            {
                RoutedEventHandler? loaded = null;
                loaded = (_, __) =>
                {
                    fe.Loaded -= loaded;
                    fe.Dispatcher.BeginInvoke((Action)FocusNow, DispatcherPriority.Input);
                };
                fe.Loaded += loaded;
            }

            return;
        }

        // FrameworkContentElement (less common, e.g. FlowDocument stuff)
        if (d is FrameworkContentElement fce)
        {
            RoutedEventHandler? loaded = null;
            loaded = (_, __) =>
            {
                fce.Loaded -= loaded;
                fce.Dispatcher.BeginInvoke((Action)FocusNow, DispatcherPriority.Input);
            };
            fce.Loaded += loaded;
            return;
        }

        // Fallback: no Loaded event -> just try via Dispatcher
        Dispatcher.CurrentDispatcher.BeginInvoke((Action)FocusNow, DispatcherPriority.Input);
    }
}
