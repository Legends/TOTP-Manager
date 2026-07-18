using TOTP.Core.Models;

namespace TOTP.Core.Services.Models;

public enum AccountImportStatus
{
    Completed,
    Cancelled,
    InvalidTargets,
    ExistingAccountsUnavailable,
    RecoveryBackupFailed
}

public sealed record AccountImportPreview(
    int TotalCount,
    int ConflictCount,
    ImportConflictStrategy ConflictStrategy);

public sealed record AccountImportOutcome(
    AccountImportStatus Status,
    int Added = 0,
    int Replaced = 0,
    int Skipped = 0,
    int Failed = 0);
