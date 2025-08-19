using TOTP.Core.Enums;

namespace TOTP.Core.Validation;


public static class SecretValidator
{
    public static ValidationError ValidatePlatform(string? input)
    {
        return string.IsNullOrWhiteSpace(input)
            ? ValidationError.PlatformRequired
            : ValidationError.None;
    }

    public static ValidationError ValidateSecret(string? input)
    {
        return string.IsNullOrWhiteSpace(input)
            ? ValidationError.SecretRequired
            : IsValidBase32Format(input)
            ? ValidationError.None
            : ValidationError.SecretInvalidFormat;
    }

    private static bool IsValidBase32Format(string value)
    {
        try
        {
            _ = OtpNet.Base32Encoding.ToBytes(value);
            return true;
        }
        catch
        {
            return false;
        }
    }
}


