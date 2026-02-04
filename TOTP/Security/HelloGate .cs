using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Security.Credentials.UI;

namespace TOTP.Security;

public sealed class HelloGate : IHelloGate
{
    public async Task<bool> IsAvailableAsync(CancellationToken ct = default)
    {
        // UserConsentVerifier has no cancellation token, but that's fine.
        var availability = await UserConsentVerifier.CheckAvailabilityAsync();
        return availability == UserConsentVerifierAvailability.Available;
    }

    public async Task<AuthorizationResult> RequestVerificationAsync(CancellationToken ct = default)
    {
        // Again: no ct support in API, but we keep signature consistent.
        // Provide a friendly reason shown in the Hello prompt.
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
}
