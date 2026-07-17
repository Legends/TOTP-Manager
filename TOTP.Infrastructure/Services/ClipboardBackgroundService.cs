using FluentResults;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TOTP.Core.Common;
using TOTP.Core.Services.Interfaces;
using TOTP.Core.Services.Models;

namespace TOTP.Infrastructure.Services;

public sealed class ClipboardBackgroundService : BackgroundService, IClipboardService
{
    private static readonly ClipboardCapabilities AutoClearCapabilities =
        ClipboardCapabilities.WriteText | ClipboardCapabilities.ConditionalClear;

    private readonly IPlatformClipboard _platformClipboard;
    private readonly ILogger<ClipboardBackgroundService> _logger;
    private readonly TimeProvider _timeProvider;
    private readonly TimeSpan _pollInterval;
    private readonly object _sync = new();
    private ClipboardWriteReceipt? _scheduledReceipt;
    private DateTimeOffset? _clearAt;

    public ClipboardBackgroundService(
        IPlatformClipboard platformClipboard,
        ILogger<ClipboardBackgroundService> logger,
        TimeProvider? timeProvider = null,
        TimeSpan? pollInterval = null)
    {
        _platformClipboard = platformClipboard ?? throw new ArgumentNullException(nameof(platformClipboard));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _timeProvider = timeProvider ?? TimeProvider.System;
        _pollInterval = pollInterval ?? TimeSpan.FromSeconds(1);

        if (_pollInterval <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(pollInterval));
        }
    }

    public ClipboardCapabilities Capabilities => _platformClipboard.Capabilities;

    public Result CopyAndScheduleClear(string text, TimeSpan? duration = null)
    {
        if (string.IsNullOrEmpty(text))
        {
            return Result.Ok();
        }

        if ((Capabilities & AutoClearCapabilities) != AutoClearCapabilities)
        {
            return Result.Fail(new AppError(
                AppErrorCode.ClipboardUnavailable,
                "The platform clipboard does not support safe conditional clearing."));
        }

        var writeResult = _platformClipboard.SetText(text);
        if (writeResult.IsFailed)
        {
            return Result.Fail(writeResult.Errors);
        }

        var clearAfter = duration ?? TimeSpan.FromSeconds(30);
        lock (_sync)
        {
            _scheduledReceipt = writeResult.Value;
            _clearAt = _timeProvider.GetUtcNow().Add(clearAfter);
        }

        _logger.LogInformation(
            "Sensitive clipboard data copied with an automatic clear scheduled after {DurationSeconds} seconds.",
            clearAfter.TotalSeconds);
        return Result.Ok();
    }

    public Result SetText(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return Result.Ok();
        }

        if (!Capabilities.HasFlag(ClipboardCapabilities.WriteText))
        {
            return Result.Fail(new AppError(AppErrorCode.ClipboardUnavailable, "Clipboard text writes are unavailable."));
        }

        var writeResult = _platformClipboard.SetText(text);
        if (writeResult.IsFailed)
        {
            return Result.Fail(writeResult.Errors);
        }

        CancelSchedule();
        _logger.LogInformation("Clipboard text copied without an automatic clear schedule.");
        return Result.Ok();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_pollInterval, _timeProvider);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            ProcessScheduledClear();
        }
    }

    internal void ProcessScheduledClear()
    {
        ClipboardWriteReceipt? receipt;
        DateTimeOffset? clearAt;
        lock (_sync)
        {
            receipt = _scheduledReceipt;
            clearAt = _clearAt;
        }

        if (!receipt.HasValue || !clearAt.HasValue || _timeProvider.GetUtcNow() < clearAt.Value)
        {
            return;
        }

        var clearResult = _platformClipboard.ClearIfUnchanged(receipt.Value);
        if (clearResult.IsFailed)
        {
            _logger.LogWarning(
                "Scheduled clipboard clear failed with error code {ErrorCode}; it will be retried.",
                clearResult.GetErrorCode());
            return;
        }

        CancelSchedule(receipt.Value);
        _logger.LogInformation(
            clearResult.Value
                ? "Clipboard cleared automatically."
                : "Clipboard changed before timeout; automatic clear was skipped.");
    }

    private void CancelSchedule(ClipboardWriteReceipt? expectedReceipt = null)
    {
        lock (_sync)
        {
            if (expectedReceipt.HasValue && _scheduledReceipt != expectedReceipt)
            {
                return;
            }

            _scheduledReceipt = null;
            _clearAt = null;
        }
    }

    public override void Dispose()
    {
        CancelSchedule();
        base.Dispose();
    }
}
