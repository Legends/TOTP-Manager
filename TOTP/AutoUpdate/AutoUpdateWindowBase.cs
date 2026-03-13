using System;
using System.Windows;
using System.Windows.Media;
using Syncfusion.Windows.Shared;

namespace TOTP.AutoUpdate;

public abstract class AutoUpdateWindowBase : ChromelessWindow
{
    private Rect? _centerAnchorBounds;

    protected void ConfigureOwner()
    {
        if (Owner is { IsVisible: false })
        {
            Owner = null;
        }

        if (Owner == null
            && Application.Current?.MainWindow is Window mainWindow
            && !ReferenceEquals(mainWindow, this)
            && mainWindow.IsVisible)
        {
            Owner = mainWindow;
        }

        WindowStartupLocation = WindowStartupLocation.Manual;
    }

    protected void CaptureCenterAnchor(Window? anchorWindow)
    {
        if (anchorWindow == null || ReferenceEquals(anchorWindow, this))
        {
            _centerAnchorBounds = null;
            return;
        }

        var anchorWidth = anchorWindow.ActualWidth > 0 ? anchorWindow.ActualWidth : anchorWindow.Width;
        var anchorHeight = anchorWindow.ActualHeight > 0 ? anchorWindow.ActualHeight : anchorWindow.Height;
        if (anchorWidth <= 0 || anchorHeight <= 0)
        {
            _centerAnchorBounds = null;
            return;
        }

        _centerAnchorBounds = new Rect(anchorWindow.Left, anchorWindow.Top, anchorWidth, anchorHeight);
    }

    protected void ClearCenterAnchor()
    {
        _centerAnchorBounds = null;
    }

    protected void InvokeOnUi(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);

        var dispatcher = Application.Current?.Dispatcher ?? Dispatcher;
        if (dispatcher.CheckAccess())
        {
            action();
            return;
        }

        dispatcher.Invoke(action);
    }

    protected void ShowOwnedWindow(bool activate = true)
    {
        InvokeOnUi(() =>
        {
            ConfigureOwner();
            if (!IsVisible)
            {
                Show();
            }

            CenterWindow();

            if (activate)
            {
                Activate();
            }
        });
    }

    protected void RecenterOwnedWindow()
    {
        InvokeOnUi(CenterWindow);
    }

    protected void ShowOwnedDialogWindow()
    {
        InvokeOnUi(() =>
        {
            ConfigureOwner();
            if (!IsVisible)
            {
                ShowDialog();
            }
        });
    }

    protected void CloseIfVisible()
    {
        InvokeOnUi(() =>
        {
            if (IsVisible)
            {
                Close();
            }
        });
    }

    private void CenterWindow()
    {
        UpdateLayout();

        var windowWidth = ActualWidth > 0 ? ActualWidth : Width;
        var windowHeight = ActualHeight > 0 ? ActualHeight : Height;
        if (windowWidth <= 0 || windowHeight <= 0)
        {
            return;
        }

        if (_centerAnchorBounds is { } anchorBounds)
        {
            Left = anchorBounds.Left + Math.Max(0, (anchorBounds.Width - windowWidth) / 2d);
            Top = anchorBounds.Top + Math.Max(0, (anchorBounds.Height - windowHeight) / 2d);
            return;
        }

        if (Owner is { IsVisible: true } owner)
        {
            var ownerLeft = owner.Left;
            var ownerTop = owner.Top;
            var ownerWidth = owner.ActualWidth > 0 ? owner.ActualWidth : owner.Width;
            var ownerHeight = owner.ActualHeight > 0 ? owner.ActualHeight : owner.Height;
            if (ownerWidth > 0 && ownerHeight > 0)
            {
                Left = ownerLeft + Math.Max(0, (ownerWidth - windowWidth) / 2d);
                Top = ownerTop + Math.Max(0, (ownerHeight - windowHeight) / 2d);
                return;
            }
        }

        var workArea = SystemParameters.WorkArea;
        Left = workArea.Left + Math.Max(0, (workArea.Width - windowWidth) / 2d);
        Top = workArea.Top + Math.Max(0, (workArea.Height - windowHeight) / 2d);
    }
}
