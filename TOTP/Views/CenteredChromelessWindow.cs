using Syncfusion.Windows.Shared;
using System;
using System.Windows;

namespace TOTP.Views;

public abstract class CenteredChromelessWindow : ChromelessWindow
{
    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        CenterWithinOwnerOrScreen();
        Opacity = 1d;
    }

    private void CenterWithinOwnerOrScreen()
    {
        if (ActualWidth <= 0 || ActualHeight <= 0)
            UpdateLayout();

        var target = GetTargetBounds();
        Left = target.Left + ((target.Width - ActualWidth) / 2d);
        Top = target.Top + ((target.Height - ActualHeight) / 2d);
    }

    private Rect GetTargetBounds()
    {
        if (Owner is { IsVisible: true } owner)
        {
            var ownerWidth = owner.ActualWidth > 0 ? owner.ActualWidth : owner.Width;
            var ownerHeight = owner.ActualHeight > 0 ? owner.ActualHeight : owner.Height;

            if (ownerWidth > 0 && ownerHeight > 0 &&
                !double.IsNaN(ownerWidth) && !double.IsNaN(ownerHeight))
            {
                return new Rect(owner.Left, owner.Top, ownerWidth, ownerHeight);
            }

            if (owner.RestoreBounds is { Width: > 0, Height: > 0 } restoreBounds)
                return restoreBounds;
        }

        return SystemParameters.WorkArea;
    }
}
