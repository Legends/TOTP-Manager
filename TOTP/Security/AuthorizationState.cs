using System;

namespace TOTP.Security;

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
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void Unlock()
    {
        if (IsUnlocked) return;
        IsUnlocked = true;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void Lock()
    {
        if (!IsUnlocked) return;
        IsUnlocked = false;
        Changed?.Invoke(this, EventArgs.Empty);
    }
}