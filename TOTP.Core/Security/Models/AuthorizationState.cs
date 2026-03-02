using System;
using TOTP.Core.Interfaces;
using TOTP.Security.Models;

namespace TOTP.Core.Security;

public sealed class AuthorizationState
{
    private readonly IDispatcherService? _dispatcherService;
    public event EventHandler? Changed;

    public bool IsUnlocked { get; private set; }
    public bool IsConfigured { get; private set; }
    public AuthorizationGateKind ConfiguredGate { get; private set; } = AuthorizationGateKind.None;

    // The dispatcher is optional to allow for easier Unit Testing
    public AuthorizationState(IDispatcherService? dispatcherService = null)
    {
        _dispatcherService = dispatcherService;
    }

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

        if (_dispatcherService == null || _dispatcherService.CheckAccess())
        {
            handler(this, EventArgs.Empty);
        }
        else
        {
            _dispatcherService.InvokeOnUI(() => handler(this, EventArgs.Empty));
        }
    }
}