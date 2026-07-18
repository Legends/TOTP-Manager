using FluentResults;
using TOTP.Core.Services.Models;

namespace TOTP.Core.Services.Interfaces;

public interface IAsyncClipboardService
{
    ClipboardCapabilities Capabilities { get; }
    Task<Result> CopyAndScheduleClearAsync(
        string text,
        TimeSpan duration,
        CancellationToken cancellationToken = default);
}
