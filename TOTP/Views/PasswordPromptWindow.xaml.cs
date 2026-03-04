using Syncfusion.Windows.Shared;
using System;
using System.Windows;
using System.Windows.Threading;

namespace TOTP.Views;

public partial class PasswordPromptWindow : ChromelessWindow
{
    public PasswordPromptWindow()
    {
        InitializeComponent();
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Wait until layout/theme finalizes actual size before centering.
        Dispatcher.BeginInvoke(new Action(CenterWithinOwnerOrScreen), DispatcherPriority.ApplicationIdle);
    }

    private void OnContentRendered(object? sender, EventArgs e)
    {
        CenterWithinOwnerOrScreen();
    }

    private void CenterWithinOwnerOrScreen()
    {
        if (ActualWidth <= 0 || ActualHeight <= 0)
        {
            UpdateLayout();
        }

        var target = GetTargetBounds();
        var left = target.Left + ((target.Width - ActualWidth) / 2d);
        var top = target.Top + ((target.Height - ActualHeight) / 2d);

        Left = left;
        Top = top;
    }

    private Rect GetTargetBounds()
    {
        if (Owner is { IsVisible: true } owner)
        {
            var ownerWidth = owner.ActualWidth > 0 ? owner.ActualWidth : owner.Width;
            var ownerHeight = owner.ActualHeight > 0 ? owner.ActualHeight : owner.Height;

            if (ownerWidth > 0 && ownerHeight > 0 && !double.IsNaN(ownerWidth) && !double.IsNaN(ownerHeight))
            {
                return new Rect(owner.Left, owner.Top, ownerWidth, ownerHeight);
            }

            var restoreBounds = owner.RestoreBounds;
            if (restoreBounds.Width > 0 && restoreBounds.Height > 0)
            {
                return restoreBounds;
            }
        }

        return new Rect(SystemParameters.WorkArea.Left, SystemParameters.WorkArea.Top, SystemParameters.WorkArea.Width, SystemParameters.WorkArea.Height);
    }
}
