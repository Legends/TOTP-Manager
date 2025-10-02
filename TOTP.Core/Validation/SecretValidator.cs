using TOTP.Core.Enums;
using TOTP.Core.Models;

namespace TOTP.Core.Validation;


public static class SecretValidator
{
    public static ValidationError ValidatePlatform(string? input)
    {
        return string.IsNullOrWhiteSpace(input)
            ? ValidationError.PlatformRequired
            : ValidationError.None;
    }

    public static ValidationError PlatformNameDuplicateExists(string platform, IEnumerable<SecretItem> source)
    {
        // Check duplicates in the bound list (ignore the current row)
        bool duplicate = source
            .Any(x => string.Equals(x.Platform, platform, StringComparison.OrdinalIgnoreCase));
        return duplicate ? ValidationError.PlatformAlreadyExists : ValidationError.None;
    }


    public static ValidationError ValidateSecret(string? secret)
    {
        return string.IsNullOrWhiteSpace(secret)
            ? ValidationError.SecretRequired
            : IsValidBase32Format(secret)
            ? ValidationError.None
            : ValidationError.SecretInvalidFormat;
    }

    public static bool IsValidBase32Format(string value)
    {
        try
        {
            var bytes = OtpNet.Base32Encoding.ToBytes(value);
            return bytes.Length > 0;
        }
        catch
        {
            return false;
        }
    }

    public static ValidationError ValidateID(Guid iD)
    {
        return iD == Guid.Empty ? ValidationError.IdRequired : ValidationError.None;
    }
}


