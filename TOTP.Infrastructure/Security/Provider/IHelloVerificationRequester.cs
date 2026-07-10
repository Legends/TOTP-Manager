using Windows.Security.Credentials.UI;

namespace TOTP.Infrastructure.Security.Provider;

public interface IHelloVerificationRequester
{
    Task<UserConsentVerificationResult> RequestAsync(
        nint windowHandle,
        string message,
        CancellationToken ct);
}
