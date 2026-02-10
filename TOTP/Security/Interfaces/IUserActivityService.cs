using System;
using TOTP.Security.Models;

namespace TOTP.Security.Interfaces;

public interface IUserActivityService
{
    event EventHandler? LockRequested;

    TimeSpan TimeSinceLastActivity { get; }

    void NotifyActivity(ActivityKind kind);
    void StartMonitoring();
    void StopMonitoring();
}
