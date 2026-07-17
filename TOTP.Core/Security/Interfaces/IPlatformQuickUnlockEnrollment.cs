using FluentResults;

namespace TOTP.Core.Security.Interfaces;

/// <summary>
/// Enables platform quick unlock only after the persisted password-recovery
/// wrapper has recovered a key that verifies the existing vault.
/// </summary>
public interface IPlatformQuickUnlockEnrollment : IDisposable
{
    Task<Result> EnableAsync(
        string recoveryPassword,
        CancellationToken cancellationToken = default);
}
