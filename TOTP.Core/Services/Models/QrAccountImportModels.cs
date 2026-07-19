namespace TOTP.Core.Services.Models;

public enum QrAccountConflictDecision
{
    Cancel,
    UpdateExisting,
    KeepBoth
}

public enum QrAccountImportStatus
{
    Added,
    Updated,
    KeptBoth,
    DuplicateUnchanged,
    Cancelled
}

public sealed record QrAccountConflict(string Issuer, string AccountName);

public sealed record QrAccountImportOutcome(
    QrAccountImportStatus Status,
    Guid AccountId,
    string Issuer,
    string AccountName);
