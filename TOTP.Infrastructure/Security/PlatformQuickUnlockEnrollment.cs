using System.Security.Cryptography;
using FluentResults;
using Microsoft.Extensions.Logging;
using TOTP.Core.Security.Interfaces;
using TOTP.Core.Security.Models;

namespace TOTP.Infrastructure.Security;

public sealed class PlatformQuickUnlockEnrollment : IPlatformQuickUnlockEnrollment
{
    private const int VaultKeySize = 32;

    private readonly IAuthorizationEnvelopeStore _envelopeStore;
    private readonly IMasterPasswordService _passwordService;
    private readonly IStoredVaultKeyVerifier _vaultVerifier;
    private readonly IPlatformQuickUnlock _platformQuickUnlock;
    private readonly ILogger<PlatformQuickUnlockEnrollment> _logger;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public PlatformQuickUnlockEnrollment(
        IAuthorizationEnvelopeStore envelopeStore,
        IMasterPasswordService passwordService,
        IStoredVaultKeyVerifier vaultVerifier,
        IPlatformQuickUnlock platformQuickUnlock,
        ILogger<PlatformQuickUnlockEnrollment> logger)
    {
        _envelopeStore = envelopeStore ?? throw new ArgumentNullException(nameof(envelopeStore));
        _passwordService = passwordService ?? throw new ArgumentNullException(nameof(passwordService));
        _vaultVerifier = vaultVerifier ?? throw new ArgumentNullException(nameof(vaultVerifier));
        _platformQuickUnlock = platformQuickUnlock ?? throw new ArgumentNullException(nameof(platformQuickUnlock));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result> EnableAsync(
        string recoveryPassword,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(recoveryPassword))
        {
            return Fail(
                PlatformQuickUnlockEnrollmentErrorCode.RecoveryPasswordRequired,
                "The recovery password is required.");
        }

        await _lock.WaitAsync(cancellationToken);
        AuthorizationEnvelopeV2? envelope = null;
        AuthorizationEnvelopeV2? updatedEnvelope = null;
        PlatformQuickUnlockWrapperV2? registeredWrapper = null;
        byte[]? recoveredKey = null;
        var persisted = false;
        try
        {
            var loaded = await _envelopeStore.LoadAsync(cancellationToken);
            if (loaded.IsFailed)
            {
                return Fail(
                    PlatformQuickUnlockEnrollmentErrorCode.EnvelopeLoadFailed,
                    "The authorization envelope could not be loaded.",
                    loaded.Errors);
            }

            envelope = loaded.Value;
            if (envelope is null)
            {
                return Fail(
                    PlatformQuickUnlockEnrollmentErrorCode.NotConfigured,
                    "Password recovery is not configured.");
            }

            if (envelope.QuickUnlockWrapper is not null)
            {
                return Fail(
                    PlatformQuickUnlockEnrollmentErrorCode.AlreadyEnabled,
                    "Platform quick unlock is already configured.");
            }

            recoveredKey = await _passwordService.UnwrapKeyV2Async(
                envelope.PasswordWrapper,
                recoveryPassword,
                cancellationToken);
            if (recoveredKey is null)
            {
                return Fail(
                    PlatformQuickUnlockEnrollmentErrorCode.InvalidRecoveryPassword,
                    "The recovery password is invalid.");
            }

            if (recoveredKey.Length != VaultKeySize)
            {
                return Fail(
                    PlatformQuickUnlockEnrollmentErrorCode.InvalidRecoveredKey,
                    "The recovery wrapper returned an invalid vault key.");
            }

            var vaultVerification = await _vaultVerifier.VerifyAsync(recoveredKey, cancellationToken);
            if (vaultVerification.IsFailed)
            {
                return Fail(
                    PlatformQuickUnlockEnrollmentErrorCode.VaultVerificationFailed,
                    "The vault could not be verified before enabling platform quick unlock.",
                    vaultVerification.Errors);
            }

            if (vaultVerification.Value is not VaultKeyVerificationStatus.Verified
                and not VaultKeyVerificationStatus.VaultNotFound)
            {
                return Fail(
                    PlatformQuickUnlockEnrollmentErrorCode.VaultVerificationFailed,
                    "The recovery key did not verify the existing vault.");
            }

            var availability = await _platformQuickUnlock.GetAvailabilityAsync(cancellationToken);
            if (availability != PlatformQuickUnlockAvailability.Available)
            {
                return Fail(
                    PlatformQuickUnlockEnrollmentErrorCode.PlatformUnavailable,
                    "Platform quick unlock is unavailable.");
            }

            var registration = await _platformQuickUnlock.RegisterAsync(recoveredKey, cancellationToken);
            if (registration.IsFailed)
            {
                return Fail(
                    PlatformQuickUnlockEnrollmentErrorCode.RegistrationFailed,
                    "Platform quick unlock could not be registered.",
                    registration.Errors);
            }

            registeredWrapper = registration.Value;
            if (!string.Equals(
                    registeredWrapper.Provider,
                    _platformQuickUnlock.ProviderId,
                    StringComparison.Ordinal)
                || !PlatformQuickUnlockContract.IsSupported(registeredWrapper))
            {
                var cleanupErrors = await RemoveRegistrationAsync(registeredWrapper);
                AuthorizationEnvelopeBufferCleaner.Clear(registeredWrapper);
                registeredWrapper = null;
                return Fail(
                    PlatformQuickUnlockEnrollmentErrorCode.RegistrationFailed,
                    "The platform returned invalid quick-unlock metadata.",
                    cleanupErrors);
            }

            updatedEnvelope = envelope with { QuickUnlockWrapper = registeredWrapper };
            var saved = await _envelopeStore.SaveAsync(updatedEnvelope, cancellationToken);
            if (saved.IsFailed)
            {
                var cleanupErrors = await RemoveRegistrationAsync(registeredWrapper);
                registeredWrapper = null;
                return Fail(
                    PlatformQuickUnlockEnrollmentErrorCode.PersistenceFailed,
                    "The quick-unlock registration could not be persisted.",
                    saved.Errors.Concat(cleanupErrors));
            }

            persisted = true;
            return Result.Ok();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            if (registeredWrapper is not null && !persisted)
                await RemoveRegistrationAsync(registeredWrapper);
            throw;
        }
        catch (Exception ex)
        {
            var cleanupErrors = registeredWrapper is not null && !persisted
                ? await RemoveRegistrationAsync(registeredWrapper)
                : [];
            _logger.LogError(ex, "Platform quick-unlock enrollment failed unexpectedly.");
            return Fail(
                PlatformQuickUnlockEnrollmentErrorCode.UnexpectedFailure,
                "Platform quick-unlock enrollment failed unexpectedly.",
                cleanupErrors,
                ex);
        }
        finally
        {
            if (recoveredKey is not null) CryptographicOperations.ZeroMemory(recoveredKey);
            if (updatedEnvelope is not null)
                AuthorizationEnvelopeBufferCleaner.Clear(updatedEnvelope);
            else if (envelope is not null)
                AuthorizationEnvelopeBufferCleaner.Clear(envelope);
            _lock.Release();
        }
    }

    public void Dispose() => _lock.Dispose();

    private async Task<IReadOnlyList<IError>> RemoveRegistrationAsync(
        PlatformQuickUnlockWrapperV2 wrapper)
    {
        try
        {
            var removed = await _platformQuickUnlock.RemoveAsync(wrapper, CancellationToken.None);
            if (removed.IsSuccess) return [];

            _logger.LogError("Failed to remove an uncommitted platform quick-unlock registration.");
            return removed.Errors;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove an uncommitted platform quick-unlock registration.");
            return
            [
                new PlatformQuickUnlockEnrollmentError(
                    PlatformQuickUnlockEnrollmentErrorCode.CleanupFailed,
                    "The uncommitted platform quick-unlock registration could not be removed.",
                    ex)
            ];
        }
    }

    private static Result Fail(
        PlatformQuickUnlockEnrollmentErrorCode code,
        string message,
        IEnumerable<IError>? causes = null,
        Exception? exception = null)
    {
        var errors = new List<IError>
        {
            new PlatformQuickUnlockEnrollmentError(code, message, exception)
        };
        if (causes is not null) errors.AddRange(causes);
        return Result.Fail(errors);
    }
}
