using NetSparkleUpdater.Interfaces;
using System;

namespace TOTP.AutoUpdate;

public partial class TOTPCheckingForUpdatesWindow : AutoUpdateWindowBase, ICheckingForUpdates
{
    public event EventHandler? UpdatesUIClosing;

    public TOTPCheckingForUpdatesWindow()
    {
        InitializeComponent();
        Closed += (_, _) => UpdatesUIClosing?.Invoke(this, EventArgs.Empty);
    }

    public new void Show()
    {
        ShowOwnedWindow();
    }

    public new void Close()
    {
        CloseIfVisible();
    }
}
