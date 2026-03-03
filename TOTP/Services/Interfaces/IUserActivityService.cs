using System;
using TOTP.Core.Security.Models;

namespace TOTP.Services.Interfaces;

public interface IUserActivityService
{
    event EventHandler? LockRequested;

    TimeSpan TimeSinceLastActivity { get; }

    void NotifyActivity(ActivityKind kind);
    void StartMonitoring();
    void StopMonitoring();
}
