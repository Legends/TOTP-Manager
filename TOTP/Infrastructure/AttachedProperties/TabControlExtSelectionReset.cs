using System;
using System.Windows;
using System.Windows.Threading;
using Syncfusion.Windows.Tools.Controls;

namespace TOTP.Infrastructure.AttachedProperties;

public static class TabControlExtSelectionReset
{
    public static readonly DependencyProperty ResetSelectionOnActivateProperty =
        DependencyProperty.RegisterAttached(
            "ResetSelectionOnActivate",
            typeof(bool),
            typeof(TabControlExtSelectionReset),
            new PropertyMetadata(false, OnResetSelectionOnActivateChanged));

    public static bool GetResetSelectionOnActivate(DependencyObject obj) =>
        (bool)obj.GetValue(ResetSelectionOnActivateProperty);

    public static void SetResetSelectionOnActivate(DependencyObject obj, bool value) =>
        obj.SetValue(ResetSelectionOnActivateProperty, value);

    private static readonly DependencyProperty HandlersAttachedProperty =
        DependencyProperty.RegisterAttached(
            "HandlersAttached",
            typeof(bool),
            typeof(TabControlExtSelectionReset),
            new PropertyMetadata(false));

    private static void OnResetSelectionOnActivateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TabControlExt tabControl)
        {
            return;
        }

        var enabled = e.NewValue is true;
        if (enabled)
        {
            AttachHandlers(tabControl);
            ResetSelection(tabControl);
            return;
        }

        DetachHandlers(tabControl);
    }

    private static void AttachHandlers(TabControlExt tabControl)
    {
        if ((bool)tabControl.GetValue(HandlersAttachedProperty))
        {
            return;
        }

        tabControl.Loaded += TabControl_Loaded;
        tabControl.IsVisibleChanged += TabControl_IsVisibleChanged;
        tabControl.SetValue(HandlersAttachedProperty, true);
    }

    private static void DetachHandlers(TabControlExt tabControl)
    {
        if (!(bool)tabControl.GetValue(HandlersAttachedProperty))
        {
            return;
        }

        tabControl.Loaded -= TabControl_Loaded;
        tabControl.IsVisibleChanged -= TabControl_IsVisibleChanged;
        tabControl.SetValue(HandlersAttachedProperty, false);
    }

    private static void TabControl_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is TabControlExt tabControl)
        {
            ResetSelection(tabControl);
        }
    }

    private static void TabControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is TabControlExt tabControl && e.NewValue is true)
        {
            ResetSelection(tabControl);
        }
    }

    private static void ResetSelection(TabControlExt tabControl)
    {
        if (tabControl.Items.Count == 0)
        {
            return;
        }

        tabControl.Dispatcher.BeginInvoke(
            DispatcherPriority.Loaded,
            new Action(() => tabControl.SelectedIndex = 0));
    }
}
