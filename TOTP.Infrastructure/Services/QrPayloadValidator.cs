using TOTP.Core.Services.Interfaces;
using TOTP.Infrastructure.Parser;

namespace TOTP.Infrastructure.Services;

public sealed class QrPayloadValidator : IQrPayloadValidator
{
    public QrPayloadValidationResult Validate(string decodedPayload)
    {
        if (string.IsNullOrWhiteSpace(decodedPayload) || decodedPayload.Length > 4096)
            return QrPayloadValidationResult.Invalid;

        try
        {
            var parsed = OtpauthParser.Parse(decodedPayload);
            if (!OtpAuthSupportPolicy.IsSupported(parsed)) return QrPayloadValidationResult.Invalid;
            return new QrPayloadValidationResult(
                true,
                parsed.Issuer?.Trim() ?? string.Empty,
                parsed.Label.Trim());
        }
        catch (Exception)
        {
            return QrPayloadValidationResult.Invalid;
        }
    }

}
