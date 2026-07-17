namespace TOTP.Core.Security.Models;

/// <summary>
/// Stable wire identifiers understood by reviewed platform quick-unlock adapters.
/// New providers and algorithms require explicit validation and security review.
/// </summary>
public static class PlatformQuickUnlockContract
{
    private const int WindowsRsa2048CiphertextSize = 256;
    private const int MaxKeyReferenceLength = 256;

    public const string WindowsHelloTpmProvider = "windows-hello-tpm";
    public const int WindowsHelloTpmProviderVersion = 1;
    public const string UserVerificationRequired = "user-verification-required";
    public const string RsaOaepSha256Algorithm = "rsa-oaep-sha256";

    public static bool IsSupported(PlatformQuickUnlockWrapperV2? wrapper)
    {
        if (wrapper?.WrappedKey is null)
        {
            return false;
        }

        return string.Equals(wrapper.Provider, WindowsHelloTpmProvider, StringComparison.Ordinal)
            && wrapper.ProviderVersion == WindowsHelloTpmProviderVersion
            && string.Equals(wrapper.AuthenticationPolicy, UserVerificationRequired, StringComparison.Ordinal)
            && !string.IsNullOrWhiteSpace(wrapper.KeyReference)
            && wrapper.KeyReference.Length <= MaxKeyReferenceLength
            && !wrapper.KeyReference.Any(char.IsControl)
            && string.Equals(wrapper.WrappedKey.Algorithm, RsaOaepSha256Algorithm, StringComparison.Ordinal)
            && wrapper.WrappedKey.Nonce is null
            && wrapper.WrappedKey.Ciphertext is { Length: WindowsRsa2048CiphertextSize };
    }
}
