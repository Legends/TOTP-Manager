using System.Security.Cryptography;
using OtpNet;
using TOTP.Core.Models;
using TOTP.Core.Services.Interfaces;
using TOTP.Core.Validation;

namespace TOTP.Infrastructure.Services;

public sealed class OtpNetTotpGenerator : ITotpGenerator
{
    private const int PeriodSeconds = 30;

    public TotpGenerationResult Generate(string base32Secret)
    {
        if (!SecretValidation.IsValidBase32Secret(base32Secret))
        {
            throw new FormatException("Secret is not valid Base32 data.");
        }

        var normalized = SecretValidation.NormalizeBase32Secret(base32Secret);
        var secretBytes = Base32Encoding.ToBytes(normalized);
        try
        {
            var totp = new Totp(secretBytes, step: PeriodSeconds);
            return new TotpGenerationResult(
                totp.ComputeTotp(),
                totp.RemainingSeconds(),
                PeriodSeconds);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(secretBytes);
        }
    }
}
