using System;
using System.Text.RegularExpressions;
using OtpNet;

namespace TOTP.Core.Validation;

public static class SecretValidation
{
    private const int MinimumSecretBytes = 10;

    public static string NormalizeBase32Secret(string secret)
    {
        if (secret is null)
            throw new ArgumentNullException(nameof(secret));

        return secret
            .Trim()
            .Replace(" ", "")
            .Replace("-", "")
            .ToUpperInvariant()
            .TrimEnd('=');
    }

    public static bool IsValidBase32Secret(string? secret)
    {
        if (string.IsNullOrWhiteSpace(secret))
            return false;

        var normalized = NormalizeBase32Secret(secret);

        if (!Regex.IsMatch(normalized, "^[A-Z2-7]+$"))
            return false;

        try
        {
            var bytes = Base32Encoding.ToBytes(normalized);
            return bytes.Length >= MinimumSecretBytes;
        }
        catch
        {
            return false;
        }
    }
}

