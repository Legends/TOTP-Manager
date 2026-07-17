using FluentResults;
using Microsoft.Extensions.Logging;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using TOTP.Core.Common;
using TOTP.Core.Services.Interfaces;
using TOTP.Core.Services.Models;

namespace TOTP.Presentation.Platform;

public sealed class WpfClipboard : IPlatformClipboard
{
    private readonly ILogger<WpfClipboard> _logger;

    public WpfClipboard(ILogger<WpfClipboard> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public ClipboardCapabilities Capabilities =>
        ClipboardCapabilities.WriteText | ClipboardCapabilities.ConditionalClear;

    public Result<ClipboardWriteReceipt> SetText(string text) =>
        InvokeOnDispatcher(
            () =>
            {
                Clipboard.SetText(text);
                return Result.Ok(new ClipboardWriteReceipt(GetClipboardSequenceNumber()));
            },
            AppErrorCode.ClipboardWriteFailed,
            "Clipboard text could not be written.");

    public Result<bool> ClearIfUnchanged(ClipboardWriteReceipt receipt) =>
        InvokeOnDispatcher(
            () =>
            {
                if (GetClipboardSequenceNumber() != receipt.ChangeToken)
                {
                    return Result.Ok(false);
                }

                Clipboard.Clear();
                return Result.Ok(true);
            },
            AppErrorCode.ClipboardClearFailed,
            "Clipboard text could not be cleared.");

    private Result<T> InvokeOnDispatcher<T>(Func<Result<T>> operation, AppErrorCode errorCode, string message)
    {
        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher == null || dispatcher.HasShutdownStarted || dispatcher.HasShutdownFinished)
        {
            return Result.Fail(new AppError(AppErrorCode.ClipboardUnavailable, "The WPF clipboard dispatcher is unavailable."));
        }

        try
        {
            return dispatcher.CheckAccess() ? operation() : dispatcher.Invoke(operation);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "A platform clipboard operation failed.");
            return Result.Fail(new AppError(errorCode, message, ex));
        }
    }

    [DllImport("user32.dll")]
    private static extern uint GetClipboardSequenceNumber();
}
