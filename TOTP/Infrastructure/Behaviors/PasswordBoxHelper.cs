using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.ScrollAxis;
using System.Windows;
using System.Windows.Controls;
using TOTP.Helper;

namespace TOTP.Infrastructure.Behaviors;

public static class PasswordBoxHelper
{
    public static readonly DependencyProperty BoundPasswordProperty =
        DependencyProperty.RegisterAttached(
            "BoundPassword",
            typeof(string),
            typeof(PasswordBoxHelper),
            new FrameworkPropertyMetadata(string.Empty, OnBoundPasswordChanged));

    public static string GetBoundPassword(DependencyObject d) => (string)d.GetValue(BoundPasswordProperty);
    public static void SetBoundPassword(DependencyObject d, string value) => d.SetValue(BoundPasswordProperty, value);

    public static readonly DependencyProperty BindPasswordProperty = DependencyProperty.RegisterAttached(
                                                                "BindPassword",
                                                                typeof(bool),
                                                                typeof(PasswordBoxHelper),
                                                                new PropertyMetadata(false, OnBindPasswordChanged));

    public static bool GetBindPassword(DependencyObject d) => (bool)d.GetValue(BindPasswordProperty);
    public static void SetBindPassword(DependencyObject d, bool value) => d.SetValue(BindPasswordProperty, value);

    private static void OnBindPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not PasswordBox pb) return;

        if ((bool)e.NewValue)
        {
            pb.PasswordChanged += PasswordChanged;
            pb.LostFocus += OnLostFocus;
        }
        else
        {
            pb.PasswordChanged -= PasswordChanged;
            pb.LostFocus -= OnLostFocus;
        }
    }

    private static void OnLostFocus(object sender, RoutedEventArgs e)
    {
        var grid = Common.FindParent<SfDataGrid>((DependencyObject)sender);
        if (grid == null || grid.CurrentCellInfo == null) return;

        // Move to another cell to trigger validation
        var currentIndex = 1;
        var nextIndex = currentIndex + 1 < grid.Columns.Count ? currentIndex + 1 : currentIndex - 1;

        if (nextIndex >= 0 && nextIndex < grid.Columns.Count)
        {
            grid.MoveCurrentCell(new RowColumnIndex(0, 0), true);
        }
    }

    private static void OnBoundPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not PasswordBox pb) return;

        // avoid feedback loop
        if (!GetUpdatingPassword(pb))
            pb.Password = e.NewValue?.ToString() ?? string.Empty;
    }

    private static readonly DependencyProperty UpdatingPasswordProperty =
        DependencyProperty.RegisterAttached("UpdatingPassword", typeof(bool), typeof(PasswordBoxHelper));

    private static bool GetUpdatingPassword(DependencyObject d) => (bool)d.GetValue(UpdatingPasswordProperty);
    private static void SetUpdatingPassword(DependencyObject d, bool value) => d.SetValue(UpdatingPasswordProperty, value);

    private static void PasswordChanged(object sender, RoutedEventArgs e)
    {
        var pb = (PasswordBox)sender;
        SetUpdatingPassword(pb, true);
        SetBoundPassword(pb, pb.Password);
        SetUpdatingPassword(pb, false);
    }
}
