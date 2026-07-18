namespace TOTP.Core.Services.Interfaces;

public sealed record QrPayloadValidationResult(
    bool IsValid,
    string Issuer,
    string AccountName)
{
    public static QrPayloadValidationResult Invalid { get; } = new(false, string.Empty, string.Empty);
}

public interface IQrPayloadValidator
{
    QrPayloadValidationResult Validate(string decodedPayload);
}
