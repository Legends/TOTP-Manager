using Windows.Security.Credentials.UI;

namespace TOTP.Security;

public sealed class HelloService : IHelloService
{
    public async Task<bool> IsAvailableAsync()
    {
        var availability = await UserConsentVerifier.CheckAvailabilityAsync();
        return availability == UserConsentVerifierAvailability.Available;
    }

    public async Task<AuthorizationResult> RequestVerificationAsync()
    {
        var result = await UserConsentVerifier.RequestVerificationAsync(
            "Unlock authenticator");

        return result switch
        {
            UserConsentVerificationResult.Verified => AuthorizationResult.Success,
            UserConsentVerificationResult.Canceled => AuthorizationResult.Cancelled,
            _ => AuthorizationResult.Failed
        };
    }
}
