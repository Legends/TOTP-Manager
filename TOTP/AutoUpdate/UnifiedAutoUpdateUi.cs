using Microsoft.Extensions.Logging;
using NetSparkleUpdater;
using NetSparkleUpdater.Enums;
using NetSparkleUpdater.Events;
using NetSparkleUpdater.Interfaces;
using System;
using System.Collections.Generic;
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
        _dialog.CloseDialog();
    }
}

internal sealed class UnifiedDownloadProgress : IDownloadProgress
{
    private readonly AutoUpdateDialogWindow _dialog;
    private readonly AppCastItem _item;
    private readonly Func<AppCastItem, string?, Task<bool>>? _customInstallHandler;
    private readonly ILogger<AutoUpdateDialogWindow>? _logger;
    private string? _downloadedFilePath;
    private bool _downloadFinished;
    private bool _downloadedFileValid;

    public UnifiedDownloadProgress(
        AutoUpdateDialogWindow dialog,
        AppCastItem item,
        Func<AppCastItem, string?, Task<bool>>? customInstallHandler,
        ILogger<AutoUpdateDialogWindow>? logger)
    {
        _dialog = dialog;
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
        _logger?.LogInformation(
            "Auto-update progress dialog: shown. version={Version}",
            _item.ShortVersion ?? _item.Version?.ToString() ?? "unknown");
    }

    public void OnDownloadProgressChanged(object sender, ItemDownloadProgressEventArgs args)
    {
        _dialog.OnDownloadProgressChanged(args);
    }

    public void Close()
    {
        _dialog.CloseDialog();
    }

    public void FinishedDownloadingFile(bool isDownloadedFileValid)
    {
        _downloadFinished = true;
        _downloadedFileValid = isDownloadedFileValid;
        _dialog.FinishedDownloadingFile(isDownloadedFileValid);
    }

    public void SetDownloadedFilePath(string? downloadedFilePath)
    {
        _downloadedFilePath = downloadedFilePath;
        _dialog.SetDownloadedFilePath(downloadedFilePath);
    }

    public bool DisplayErrorMessage(string errorMessage)
    {
        return _dialog.DisplayErrorMessage(errorMessage);
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
