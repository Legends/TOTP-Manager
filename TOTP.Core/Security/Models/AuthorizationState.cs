using System;
using TOTP.Core.Enums;
using TOTP.Core.Security.Models;

namespace TOTP.Core.Security;

public sealed class AuthorizationState
{
    public event EventHandler? Changed;

    public bool IsUnlocked { get; private set; }
    public bool IsConfigured { get; private set; }
    public PreferredUnlockMethod PreferredUnlockMethod { get; private set; } = PreferredUnlockMethod.Password;

    /// <summary>
    /// Stable projection consumed by desktop authorization view models.
    /// </summary>
    public AuthorizationGateKind ConfiguredGate { get; private set; } = AuthorizationGateKind.None;

    public void SetConfiguration(
        bool isConfigured,
        PreferredUnlockMethod preferredUnlockMethod)
    {
        IsConfigured = isConfigured;
        var normalizedPreference = isConfigured && Enum.IsDefined(preferredUnlockMethod)
            ? preferredUnlockMethod
            : PreferredUnlockMethod.Password;
        PreferredUnlockMethod = normalizedPreference;
        ConfiguredGate = !isConfigured
            ? AuthorizationGateKind.None
            : normalizedPreference == PreferredUnlockMethod.PlatformQuickUnlock
                ? AuthorizationGateKind.Hello
                : AuthorizationGateKind.Password;
        RaiseChanged();
    }

    /// <summary>
    /// Compatibility adapter for the unpublished development-era profile flow.
    /// </summary>
    public void SetProfile(AuthorizationProfile? profile)
    {
        var isConfigured = profile?.IsConfigured == true;
        var preferredUnlockMethod = profile?.Gate == AuthorizationGateKind.Hello
            ? PreferredUnlockMethod.PlatformQuickUnlock
            : PreferredUnlockMethod.Password;
        SetConfiguration(isConfigured, preferredUnlockMethod);
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
