using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using TOTP.Security.Interfaces;
using Windows.Security.Credentials.UI;

namespace TOTP.Security.Models;

public sealed class HelloGate : IHelloGate
{
    public async Task<bool> IsAvailableAsync(CancellationToken ct = default)
    {
        var availability = await UserConsentVerifier.CheckAvailabilityAsync();
        return availability == UserConsentVerifierAvailability.Available;
    }

    public async Task<AuthorizationResult> RequestVerificationAsync(CancellationToken ct = default)
    {
        const string message = "Unlock TOTP Manager";
        var result = await UserConsentVerifier.RequestVerificationAsync(message);

        return result switch
        {
            UserConsentVerificationResult.Verified => AuthorizationResult.Success,
            UserConsentVerificationResult.DeviceNotPresent => AuthorizationResult.NotAvailable,
            UserConsentVerificationResult.DisabledByPolicy => AuthorizationResult.DisabledByPolicy,
            UserConsentVerificationResult.RetriesExhausted => AuthorizationResult.TooManyAttempts,
            UserConsentVerificationResult.Canceled => AuthorizationResult.Cancelled,
            _ => AuthorizationResult.Failed
        };
    }

    /// <summary>
    /// Encrypts the DEK using Windows Data Protection API (DPAPI).
    /// This is tied to the current Windows user profile.
    /// </summary>
    public byte[] ProtectKey(byte[] rawDek)
    {
        return ProtectedData.Protect(rawDek, null, DataProtectionScope.CurrentUser);
    }

    /// <summary>
    /// Decrypts the DEK using DPAPI.
    /// </summary>
    public byte[] UnprotectKey(byte[] wrappedDek)
    {
        try
        {
            return ProtectedData.Unprotect(wrappedDek, null, DataProtectionScope.CurrentUser);
        }
        catch (CryptographicException)
        {
            // Occurs if the Windows user changed or the profile is inaccessible
            return null;
        }
    }
}