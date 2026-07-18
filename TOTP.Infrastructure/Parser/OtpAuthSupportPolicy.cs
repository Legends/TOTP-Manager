namespace TOTP.Infrastructure.Parser;

internal static class OtpAuthSupportPolicy
{
    public static bool IsSupported(OtpauthParser.TOTPData parsed) =>
        string.Equals(parsed.Algorithm, "SHA1", StringComparison.OrdinalIgnoreCase)
        && parsed.Digits == 6
        && parsed.Period == 30;
}
