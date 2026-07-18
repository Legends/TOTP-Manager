using FluentResults;
using TOTP.Core.Models;
using TOTP.Core.Services.Interfaces;
using TOTP.Core.Services.Models;
using TOTP.Core.Validation;
using TOTP.Infrastructure.Parser;

namespace TOTP.Infrastructure.Services;

public sealed class QrAccountImportService(IAccountManager accounts) : IQrAccountImportService
{
    private const int MaximumPayloadLength = 4096;

    public async Task<Result<QrAccountImportOutcome>> ImportAsync(
        string decodedPayload,
        Func<QrAccountConflict, CancellationToken, Task<QrAccountConflictDecision>> resolveConflict,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(resolveConflict);
        if (string.IsNullOrWhiteSpace(decodedPayload) || decodedPayload.Length > MaximumPayloadLength)
            return Result.Fail<QrAccountImportOutcome>("The QR payload is invalid.");

        OtpauthParser.TOTPData parsed;
        try
        {
            parsed = OtpauthParser.Parse(decodedPayload);
        }
        catch (Exception)
        {
            return Result.Fail<QrAccountImportOutcome>("The QR payload is invalid.");
        }

        if (!OtpAuthSupportPolicy.IsSupported(parsed))
            return Result.Fail<QrAccountImportOutcome>("The TOTP parameters are unsupported.");

        cancellationToken.ThrowIfCancellationRequested();
        var issuer = parsed.Issuer?.Trim() ?? string.Empty;
        var accountName = parsed.Label.Trim();
        var loaded = await accounts.GetAllOtpEntriesSortedAsync();
        if (loaded.IsFailed)
            return Result.Fail<QrAccountImportOutcome>(loaded.Errors);

        var existing = loaded.Value.FirstOrDefault(account =>
            string.Equals(account.Issuer.Trim(), issuer, StringComparison.OrdinalIgnoreCase)
            && string.Equals(
                (account.AccountName ?? string.Empty).Trim(),
                accountName,
                StringComparison.OrdinalIgnoreCase));
        var incoming = new Account(Guid.NewGuid(), issuer, parsed.SecretBase32, EmptyToNull(accountName));
        if (existing is null)
        {
            var added = await accounts.AddNewAsync(incoming);
            return added.IsSuccess
                ? Result.Ok(new QrAccountImportOutcome(QrAccountImportStatus.Added, issuer, accountName))
                : Result.Fail<QrAccountImportOutcome>(added.Errors);
        }

        if (string.Equals(
            NormalizeSecret(existing.Secret),
            NormalizeSecret(incoming.Secret),
            StringComparison.Ordinal))
        {
            return Result.Ok(new QrAccountImportOutcome(
                QrAccountImportStatus.DuplicateUnchanged,
                issuer,
                accountName));
        }

        var decision = await resolveConflict(
            new QrAccountConflict(issuer, accountName),
            cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        if (decision == QrAccountConflictDecision.Cancel)
        {
            return Result.Ok(new QrAccountImportOutcome(
                QrAccountImportStatus.Cancelled,
                issuer,
                accountName));
        }

        FluentResults.Result saved;
        QrAccountImportStatus status;
        if (decision == QrAccountConflictDecision.UpdateExisting)
        {
            var updated = new Account(existing.ID, issuer, incoming.Secret, EmptyToNull(accountName));
            saved = await accounts.UpdateAsync(existing, updated);
            status = QrAccountImportStatus.Updated;
        }
        else
        {
            saved = await accounts.AddNewAsync(incoming);
            status = QrAccountImportStatus.KeptBoth;
        }

        return saved.IsSuccess
            ? Result.Ok(new QrAccountImportOutcome(status, issuer, accountName))
            : Result.Fail<QrAccountImportOutcome>(saved.Errors);
    }

    private static string? EmptyToNull(string value) => value.Length == 0 ? null : value;

    private static string NormalizeSecret(string value) => SecretValidation.NormalizeBase32Secret(value);

}
