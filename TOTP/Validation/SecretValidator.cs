using TOTP.Resources;
using TOTP.Services;

namespace TOTP.Validation
{
    public static class SecretValidator
    {
        public static string? ValidatePlatform(string? input)
        {
            return string.IsNullOrWhiteSpace(input) ? UI.msg_PlatformRequired : null;
        }

        public static string? ValidateSecret(string? input)
        {
            return string.IsNullOrWhiteSpace(input)
                ? UI.msg_SecretRequired
                : !SecretsManager.IsValidBase32Format(input) ? UI.msg_SecretInvalidFormat : null;
        }
    }
}
