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
    Cancelled,
    BulkImported
}

public sealed record QrAccountConflict(string Issuer, string AccountName);

public sealed record QrAccountImportOutcome(
    QrAccountImportStatus Status,
    Guid AccountId,
    string Issuer,
    string AccountName,
    int TotalCount = 1,
    int AddedCount = 0,
    int UpdatedCount = 0,
    int KeptBothCount = 0,
    int DuplicateCount = 0,
    int FailedCount = 0,
    int BatchIndex = 0,
    int BatchSize = 1)
{
    public int ImportedCount => AddedCount + UpdatedCount + KeptBothCount;

    public bool HasMoreBatches => BatchIndex + 1 < BatchSize;
}
