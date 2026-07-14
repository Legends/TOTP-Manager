using System;
using TOTP.Core.Security.Models;

namespace TOTP.Core.Security;

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

        handler(this, EventArgs.Empty);
    }
}
