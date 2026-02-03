using System;

namespace TOTP.Security;

public sealed class AuthorizationState
{
    public event EventHandler? Changed;

    public bool IsUnlocked { get; private set; }
    public DateTimeOffset? LastUnlockedAt { get; private set; }

    public void Unlock()
    {
        IsUnlocked = true;
        LastUnlockedAt = DateTimeOffset.UtcNow;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void Lock()
    {
        IsUnlocked = false;
        LastUnlockedAt = null;
        Changed?.Invoke(this, EventArgs.Empty);
    }
}
