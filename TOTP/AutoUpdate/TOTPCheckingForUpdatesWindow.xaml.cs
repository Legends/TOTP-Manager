using NetSparkleUpdater.Interfaces;
using System;
using System.Windows;
using Syncfusion.Windows.Shared;

namespace TOTP.AutoUpdate;

public partial class TOTPCheckingForUpdatesWindow : ChromelessWindow, ICheckingForUpdates
{
    public event EventHandler? UpdatesUIClosing;

    public TOTPCheckingForUpdatesWindow()
    {
        InitializeComponent();
        Closed += (_, _) => UpdatesUIClosing?.Invoke(this, EventArgs.Empty);
    }

    public new void Show()
    {
        InvokeOnUi(() =>
        {
            ConfigureOwner();
            if (!IsVisible)
            {
                base.Show();
            }

            Activate();
        });
    }

    public new void Close()
    {
        InvokeOnUi(() =>
        {
            if (IsVisible)
            {
                base.Close();
            }
        });
    }

    private void ConfigureOwner()
    {
        if (Owner == null && Application.Current?.MainWindow is Window mainWindow && !ReferenceEquals(mainWindow, this))
        {
            if (mainWindow.IsVisible)
            {
                Owner = mainWindow;
            }
            else
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
        }
    }

    private void InvokeOnUi(Action action)
    {
        var dispatcher = Application.Current?.Dispatcher ?? Dispatcher;
        if (dispatcher.CheckAccess())
        {
            action();
            return;
        }

        dispatcher.Invoke(action);
    }
}
