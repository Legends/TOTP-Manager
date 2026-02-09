using System;

namespace TOTP.Security;

public interface IUserActivityService
{
    event EventHandler? LockRequested;

    TimeSpan TimeSinceLastActivity { get; }

    void NotifyActivity(ActivityKind kind);
    void StartMonitoring();
    void StopMonitoring();
}
