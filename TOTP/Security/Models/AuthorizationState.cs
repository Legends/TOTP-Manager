using System;
using System.Windows;
using System.Windows.Threading;

namespace TOTP.Security.Models;

public sealed class AuthorizationState
{
    public event EventHandler? Changed;

    public bool IsUnlocked { get; private set; }
    public bool IsConfigured { get; private set; }
    public AuthorizationGateKind ConfiguredGate { get; private set; } = AuthorizationGateKind.None;

    public void SetProfile(AuthorizationProfile? profile)
    {
        IsConfigured = profile?.IsConfigured == true;
        ConfiguredGate = profile?.Gate ?? AuthorizationGateKind.None;
        RaiseChanged();
    }

    public void Unlock()
    {
        if (IsUnlocked) return;
        IsUnlocked = true;
        RaiseChanged();
    }

    public void Lock()
    {
        if (!IsUnlocked) return;
        IsUnlocked = false;
        RaiseChanged();
    }

    private void RaiseChanged()
    {
        var handler = Changed;
        if (handler is null) return;

        var dispatcher = Application.Current?.Dispatcher;

        // If we don't have a dispatcher (e.g., unit tests) OR we're already on UI thread:
        if (dispatcher is null || dispatcher.CheckAccess())
        {
            handler(this, EventArgs.Empty);
        }
        else
        {
            dispatcher.BeginInvoke(
                (Action)(() => handler(this, EventArgs.Empty)),
                DispatcherPriority.DataBind);
        }
    }
}