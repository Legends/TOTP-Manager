using System;
using System.Windows;
using Syncfusion.Windows.Shared;

namespace TOTP.AutoUpdate;

public abstract class AutoUpdateWindowBase : ChromelessWindow
{
    protected void ConfigureOwner()
    {
        if (Owner == null && Application.Current?.MainWindow is Window mainWindow && !ReferenceEquals(mainWindow, this))
        {
            Owner = mainWindow;
            return;
        }

        WindowStartupLocation = WindowStartupLocation.CenterScreen;
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

            if (activate)
            {
                Activate();
            }
        });
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
}
