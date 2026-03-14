using Microsoft.Extensions.Logging;
using NetSparkleUpdater;
using NetSparkleUpdater.Enums;
using NetSparkleUpdater.Events;
using NetSparkleUpdater.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TOTP.AutoUpdate;

internal sealed class UnifiedCheckingForUpdates : ICheckingForUpdates
{
    private readonly AutoUpdateDialogWindow _dialog;

    public UnifiedCheckingForUpdates(AutoUpdateDialogWindow dialog)
    {
        _dialog = dialog;
    }

    public event EventHandler? UpdatesUIClosing;

    public void Show()
    {
        _dialog.ShowChecking();
    }

    public void Close()
    {
        _dialog.CloseCheckingIfVisible();
        UpdatesUIClosing?.Invoke(this, EventArgs.Empty);
    }
}

internal sealed class UnifiedUpdateAvailable : IUpdateAvailable
{
    private readonly AutoUpdateDialogWindow _dialog;
    private readonly List<AppCastItem> _updates;
    private readonly bool _isUpdateAlreadyDownloaded;
    private bool _hideReleaseNotes;
    private bool _hideRemindMeLaterButton;
    private bool _hideSkipButton;

    public UnifiedUpdateAvailable(AutoUpdateDialogWindow dialog, List<AppCastItem> updates, bool isUpdateAlreadyDownloaded)
    {
        _dialog = dialog;
        _updates = updates;
        _isUpdateAlreadyDownloaded = isUpdateAlreadyDownloaded;
        CurrentItem = updates[0];
    }

    public event UserRespondedToUpdate? UserResponded;

    public UpdateAvailableResult Result { get; private set; }

    public AppCastItem CurrentItem { get; set; }

    public void Show(bool isOnMainThread)
    {
        _dialog.UpdateResponseHandler = (result, item) =>
        {
            Result = result;
            CurrentItem = item;
            UserResponded?.Invoke(this, new UpdateResponseEventArgs(result, item));
        };

        _dialog.ShowUpdateAvailable(
            _updates,
            _isUpdateAlreadyDownloaded,
            _hideReleaseNotes,
            _hideRemindMeLaterButton,
            _hideSkipButton);
    }

    public void HideReleaseNotes()
    {
        _hideReleaseNotes = true;
    }

    public void HideRemindMeLaterButton()
    {
        _hideRemindMeLaterButton = true;
    }

    public void HideSkipButton()
    {
        _hideSkipButton = true;
    }

    public void BringToFront()
    {
        _dialog.BringToFront();
    }

    public void Close()
    {
        if (Result == UpdateAvailableResult.InstallUpdate)
        {
            return;
        }

        _dialog.CloseDialog();
    }
}

internal sealed class UnifiedDownloadProgress : IDownloadProgress
{
    private static readonly TimeSpan DownloadStartTimeout = TimeSpan.FromSeconds(20);
    private readonly AutoUpdateDialogWindow _dialog;
    private readonly AppCastItem _item;
    private readonly SparkleUpdater _sparkle;
    private readonly Func<AppCastItem, string?, Task<bool>>? _customInstallHandler;
    private readonly ILogger<AutoUpdateDialogWindow>? _logger;
    private string? _downloadedFilePath;
    private bool _downloadFinished;
    private bool _downloadedFileValid;
    private CancellationTokenSource? _downloadStartTimeoutCts;

    public UnifiedDownloadProgress(
        AutoUpdateDialogWindow dialog,
        SparkleUpdater sparkle,
        AppCastItem item,
        Func<AppCastItem, string?, Task<bool>>? customInstallHandler,
        ILogger<AutoUpdateDialogWindow>? logger)
    {
        _dialog = dialog;
        _sparkle = sparkle;
        _item = item;
        _customInstallHandler = customInstallHandler;
        _logger = logger;
    }

    public event DownloadInstallEventHandler? DownloadProcessCompleted;

    public void SetDownloadAndInstallButtonEnabled(bool shouldBeEnabled)
    {
        _dialog.SetDownloadAndInstallButtonEnabled(shouldBeEnabled);
    }

    public void Show(bool isOnMainThread)
    {
        _dialog.ProgressActionHandler = HandleActionButtonAsync;
        _dialog.ShowDownloadProgress(_item);
        StartDownloadStartWatchdog();
        _logger?.LogInformation(
            "Auto-update progress dialog: shown. version={Version}",
            _item.ShortVersion ?? _item.Version?.ToString() ?? "unknown");
    }

    public void OnDownloadProgressChanged(object sender, ItemDownloadProgressEventArgs args)
    {
        CancelDownloadStartWatchdog();
        _dialog.OnDownloadProgressChanged(args);
    }

    public void Close()
    {
        _dialog.CloseDialog();
    }

    public void FinishedDownloadingFile(bool isDownloadedFileValid)
    {
        CancelDownloadStartWatchdog();
        _downloadFinished = true;
        _downloadedFileValid = isDownloadedFileValid;
        _dialog.FinishedDownloadingFile(isDownloadedFileValid);
    }

    public void NotifyDownloadStarted(string? downloadPath)
    {
        CancelDownloadStartWatchdog();
        if (!string.IsNullOrWhiteSpace(downloadPath))
        {
            _downloadedFilePath = downloadPath;
        }

        _logger?.LogInformation(
            "Auto-update progress dialog: download started. version={Version} path={Path}",
            _item.ShortVersion ?? _item.Version?.ToString() ?? "unknown",
            _downloadedFilePath);
    }

    public void SetDownloadedFilePath(string? downloadedFilePath)
    {
        CancelDownloadStartWatchdog();
        _downloadedFilePath = downloadedFilePath;
        _dialog.SetDownloadedFilePath(downloadedFilePath);
    }

    public bool DisplayErrorMessage(string errorMessage)
    {
        CancelDownloadStartWatchdog();
        return _dialog.DisplayErrorMessage(errorMessage);
    }

    private void StartDownloadStartWatchdog()
    {
        CancelDownloadStartWatchdog();
        _downloadStartTimeoutCts = new CancellationTokenSource();
        var token = _downloadStartTimeoutCts.Token;

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(DownloadStartTimeout, token);
                if (token.IsCancellationRequested || _downloadFinished)
                {
                    return;
                }

                const string message = "The update download did not start in time. Check the app log for updater diagnostics and try again.";
                _logger?.LogWarning(
                    "Auto-update progress dialog: download start timeout. version={Version} timeout_seconds={TimeoutSeconds} path={Path}",
                    _item.ShortVersion ?? _item.Version?.ToString() ?? "unknown",
                    DownloadStartTimeout.TotalSeconds,
                    _downloadedFilePath);
                _dialog.DisplayErrorMessage(message);
            }
            catch (OperationCanceledException)
            {
            }
        }, token);
    }

    private void CancelDownloadStartWatchdog()
    {
        if (_downloadStartTimeoutCts == null)
        {
            return;
        }

        _downloadStartTimeoutCts.Cancel();
        _downloadStartTimeoutCts.Dispose();
        _downloadStartTimeoutCts = null;
    }

    private async Task HandleActionButtonAsync()
    {
        _logger?.LogInformation(
            "Auto-update progress dialog: action button clicked. version={Version} download_finished={DownloadFinished} download_valid={DownloadValid} path={Path}",
            _item.ShortVersion ?? _item.Version?.ToString() ?? "unknown",
            _downloadFinished,
            _downloadedFileValid,
            _downloadedFilePath);

        if (!_downloadFinished || !_downloadedFileValid)
        {
            _logger?.LogInformation(
                "Auto-update progress dialog: cancel requested. version={Version}",
                _item.ShortVersion ?? _item.Version?.ToString() ?? "unknown");
            _sparkle.CancelFileDownload();
            _dialog.CloseDialog();
            return;
        }

        if (_customInstallHandler != null)
        {
            _dialog.ShowInstallingState();
            _dialog.SetDownloadAndInstallButtonEnabled(false);
            var wasHandled = await _customInstallHandler(_item, _downloadedFilePath);
            _logger?.LogInformation(
                "Auto-update progress dialog: custom install handler completed. version={Version} handled={Handled} path={Path}",
                _item.ShortVersion ?? _item.Version?.ToString() ?? "unknown",
                wasHandled,
                _downloadedFilePath);
            if (wasHandled)
            {
                var handledArgs = new DownloadInstallEventArgs(false)
                {
                    WasHandled = true
                };
                DownloadProcessCompleted?.Invoke(this, handledArgs);
                _dialog.CloseDialog();
                return;
            }

            _dialog.ShowHelperFailedState();
            _dialog.SetDownloadAndInstallButtonEnabled(true);
            return;
        }

        DownloadProcessCompleted?.Invoke(this, new DownloadInstallEventArgs(true));
        _dialog.CloseDialog();
    }
}
