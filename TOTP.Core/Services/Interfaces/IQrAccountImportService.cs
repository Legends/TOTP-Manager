using FluentResults;
using TOTP.Core.Services.Models;

namespace TOTP.Core.Services.Interfaces;

public interface IQrAccountImportService
{
    Task<Result<QrAccountImportOutcome>> ImportAsync(
        string decodedPayload,
        Func<QrAccountConflict, CancellationToken, Task<QrAccountConflictDecision>> resolveConflict,
        CancellationToken cancellationToken = default);
}
