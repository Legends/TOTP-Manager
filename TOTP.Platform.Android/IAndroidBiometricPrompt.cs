using TOTP.Core.Security.Models;

namespace TOTP.Platform.Android;

public interface IAndroidBiometricPrompt
{
    Task<PlatformQuickUnlockAvailability> GetAvailabilityAsync(
        CancellationToken cancellationToken = default);

    Task<AndroidBiometricPromptResult> AuthenticateAsync(
        Func<byte[]> completeCryptographicOperation,
        CancellationToken cancellationToken = default);
}
