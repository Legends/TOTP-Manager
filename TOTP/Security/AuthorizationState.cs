using System;

namespace TOTP.Security;

public sealed class AuthorizationState
{
    public bool IsUnlocked { get; private set; }
    public DateTimeOffset? LastUnlockedAt { get; private set; }

    public void Unlock()
    {
        IsUnlocked = true;
        LastUnlockedAt = DateTimeOffset.UtcNow;
    }

    public void Lock()
    {
        IsUnlocked = false;
        LastUnlockedAt = null;
    }
}
