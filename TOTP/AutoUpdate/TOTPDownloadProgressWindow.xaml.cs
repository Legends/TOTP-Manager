using Microsoft.Extensions.Logging;
using NetSparkleUpdater;
using NetSparkleUpdater.Events;
using NetSparkleUpdater.Interfaces;
using System;
using System.Threading.Tasks;
using System.Windows;
using TOTP.Resources;

namespace TOTP.AutoUpdate;

public partial class TOTPDownloadProgressWindow : AutoUpdateWindowBase, IDownloadProgress
{
    private readonly AppCastItem _item;
    private readonly Func<AppCastItem, string?, Task<bool>>? _customInstallHandler;
    private readonly ILogger<TOTPDownloadProgressWindow>? _logger;
    private readonly DateTimeOffset _downloadWindowCreatedAtUtc = DateTimeOffset.UtcNow;
    private static readonly TimeSpan MinimumReadyDelay = TimeSpan.FromMilliseconds(900);
    private bool _downloadFinished;
    private bool _downloadedFileValid;
    private bool _readyStateApplied;
    private bool _terminalErrorDisplayed;
    private int _lastProgressPercentage;
    private string? _downloadedFilePath;

    public event DownloadInstallEventHandler? DownloadProcessCompleted;

    public TOTPDownloadProgressWindow(
        AppCastItem item,
        Func<AppCastItem, string?, Task<bool>>? customInstallHandler = null,
        ILogger<TOTPDownloadProgressWindow>? logger = null)
    {
        _item = item ?? throw new ArgumentNullException(nameof(item));
        _customInstallHandler = customInstallHandler;
        _logger = logger;
        InitializeComponent();

        VersionText.Text = string.Format(
            UI.ui_Updater_Download_VersionInfo_Format,
            _item.ShortVersion ?? _item.Version?.ToString() ?? UI.ui_Updater_Common_Unknown,
            _item.DownloadLink);
        DownloadInfoText.Text = _item.UpdateSize > 0
            ? string.Format(UI.ui_Updater_Download_ExpectedSize_Format, FormatBytes(_item.UpdateSize))
            : string.Format(UI.ui_Updater_Download_ExpectedSize_Format, UI.ui_Updater_Common_Unknown);
        ActionButton.IsEnabled = false;

        _logger?.LogInformation(
            "Auto-update progress window: created. version={Version} download_url={DownloadUrl} expected_size={ExpectedSize}",
            _item.ShortVersion ?? _item.Version?.ToString() ?? "unknown",
            _item.DownloadLink,
            _item.UpdateSize);
    }

    public void SetDownloadAndInstallButtonEnabled(bool shouldBeEnabled)
    {
        if ((_readyStateApplied || _terminalErrorDisplayed) && !shouldBeEnabled)
        {
            _logger?.LogInformation(
                "Auto-update progress window: ignored external button disable after terminal state. version={Version} ready_state={ReadyStateApplied} terminal_error={TerminalErrorDisplayed}",
                _item.ShortVersion ?? _item.Version?.ToString() ?? "unknown",
                _readyStateApplied,
                _terminalErrorDisplayed);
            return;
        }

        _logger?.LogInformation(
            "Auto-update progress window: button enabled state changed. version={Version} enabled={Enabled}",
            _item.ShortVersion ?? _item.Version?.ToString() ?? "unknown",
            shouldBeEnabled);
        InvokeOnUi(() => ActionButton.IsEnabled = shouldBeEnabled);
    }

    public void SetDownloadedFilePath(string? downloadedFilePath)
    {
        _downloadedFilePath = downloadedFilePath;
        _logger?.LogInformation(
            "Auto-update progress window: downloaded file path captured. version={Version} path={Path}",
            _item.ShortVersion ?? _item.Version?.ToString() ?? "unknown",
            downloadedFilePath);
    }

    public void Show(bool isOnMainThread)
    {
        ShowOwnedWindow();

        _logger?.LogInformation(
            "Auto-update progress window: shown. version={Version}",
            _item.ShortVersion ?? _item.Version?.ToString() ?? "unknown");
    }

    public void OnDownloadProgressChanged(object sender, ItemDownloadProgressEventArgs args)
    {
        if (_terminalErrorDisplayed)
        {
            return;
        }

        _lastProgressPercentage = Math.Clamp(args.ProgressPercentage, 0, 100);
        _logger?.LogInformation(
            "Auto-update progress window: progress changed. version={Version} percentage={Percentage} bytes_received={BytesReceived} total_bytes={TotalBytes}",
            _item.ShortVersion ?? _item.Version?.ToString() ?? "unknown",
            _lastProgressPercentage,
            args.BytesReceived,
            args.TotalBytesToReceive);

        InvokeOnUi(() =>
        {
            DownloadProgressBar.IsIndeterminate = false;
            DownloadProgressBar.Value = _lastProgressPercentage;
            ProgressStateText.Text = UI.ui_Updater_Download_State_Downloading;
            ProgressText.Text = string.Format(
                UI.ui_Updater_Download_Progress_Format,
                args.ProgressPercentage,
                FormatBytes(args.BytesReceived),
                FormatBytes(args.TotalBytesToReceive));
        });

        if (_downloadFinished)
        {
            _ = TryApplyFinishedStateAsync();
        }
    }

    public new void Close()
    {
        CloseIfVisible();
    }

    public void FinishedDownloadingFile(bool isDownloadedFileValid)
    {
        if (_terminalErrorDisplayed)
        {
            return;
        }

        _downloadFinished = true;
        _downloadedFileValid = isDownloadedFileValid;
        if (isDownloadedFileValid)
        {
            _lastProgressPercentage = 100;
        }

        _logger?.LogInformation(
            "Auto-update progress window: finished downloading file. version={Version} valid={IsValid} path={Path}",
            _item.ShortVersion ?? _item.Version?.ToString() ?? "unknown",
            isDownloadedFileValid,
            _downloadedFilePath);

        _ = TryApplyFinishedStateAsync();
    }

    private async Task TryApplyFinishedStateAsync()
    {
        if (_readyStateApplied || !_downloadFinished)
        {
            return;
        }

        var remainingDelay = MinimumReadyDelay - (DateTimeOffset.UtcNow - _downloadWindowCreatedAtUtc);
        if (remainingDelay > TimeSpan.Zero)
        {
            await Task.Delay(remainingDelay);
        }

        InvokeOnUi(() =>
        {
            if (_readyStateApplied)
            {
                return;
            }

            if (_downloadedFileValid)
            {
                DownloadProgressBar.IsIndeterminate = false;
                DownloadProgressBar.Value = 100;
                ActionButton.IsEnabled = true;
                TitleText.Text = UI.ui_Updater_Download_Ready_Header;
                ProgressStateText.Text = UI.ui_Updater_Available_State_Ready;
                ProgressText.Text = UI.ui_Updater_Download_Ready_Description;
                ActionButton.Content = UI.ui_Updater_Available_Button_Install;
                _readyStateApplied = true;
                _logger?.LogInformation(
                    "Auto-update progress window: ready state applied. version={Version} path={Path}",
                    _item.ShortVersion ?? _item.Version?.ToString() ?? "unknown",
                    _downloadedFilePath);
                return;
            }

            DownloadProgressBar.IsIndeterminate = false;
            DownloadProgressBar.Value = 100;
            ActionButton.IsEnabled = true;
            TitleText.Text = UI.ui_Updater_Download_Blocked_Header;
            ProgressStateText.Text = UI.ui_Updater_Download_State_Blocked;
            ProgressText.Text = UI.ui_Updater_Download_Blocked_Description;
            ActionButton.Content = UI.ui_btnClose;
            _readyStateApplied = true;
            _logger?.LogWarning(
                "Auto-update progress window: blocked state applied due to invalid download. version={Version} path={Path}",
                _item.ShortVersion ?? _item.Version?.ToString() ?? "unknown",
                _downloadedFilePath);
        });
    }

    public bool DisplayErrorMessage(string errorMessage)
    {
        _terminalErrorDisplayed = true;
        _downloadFinished = false;
        _downloadedFileValid = false;
        _readyStateApplied = true;

        InvokeOnUi(() =>
        {
            DownloadProgressBar.IsIndeterminate = false;
            DownloadProgressBar.Value = Math.Clamp(_lastProgressPercentage, 0, 100);
            TitleText.Text = UI.ui_Updater_Download_Error_Header;
            ProgressStateText.Text = UI.ui_Updater_Download_State_Error;
            ProgressText.Text = UI.ui_Updater_Download_Error_Description;
            ErrorText.Text = errorMessage;
            ErrorText.Visibility = Visibility.Visible;
            ActionButton.IsEnabled = true;
            ActionButton.Content = UI.ui_btnClose;
        });

        _logger?.LogWarning(
            "Auto-update progress window: terminal error displayed. version={Version} path={Path} error={Error}",
            _item.ShortVersion ?? _item.Version?.ToString() ?? "unknown",
            _downloadedFilePath,
            errorMessage);

        return true;
    }

    private async void ActionButton_OnClick(object sender, RoutedEventArgs e)
    {
        _logger?.LogInformation(
            "Auto-update progress window: action button clicked. version={Version} download_finished={DownloadFinished} download_valid={DownloadValid} path={Path}",
            _item.ShortVersion ?? _item.Version?.ToString() ?? "unknown",
            _downloadFinished,
            _downloadedFileValid,
            _downloadedFilePath);

        if (!_downloadFinished || !_downloadedFileValid)
        {
            Close();
            return;
        }

        if (_customInstallHandler != null)
        {
            ActionButton.IsEnabled = false;
            ProgressStateText.Text = UI.ui_Updater_Download_State_Installing;
            ProgressText.Text = UI.ui_Updater_Download_Installing_Description;

            var wasHandled = await _customInstallHandler(_item, _downloadedFilePath);
            _logger?.LogInformation(
                "Auto-update progress window: custom install handler completed. version={Version} handled={Handled} path={Path}",
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
                Close();
                return;
            }

            ActionButton.IsEnabled = true;
            ProgressStateText.Text = UI.ui_Updater_Available_State_Ready;
            ProgressText.Text = UI.ui_Updater_Download_HelperFailed_Description;
            return;
        }

        DownloadProcessCompleted?.Invoke(this, new DownloadInstallEventArgs(true));
        Close();
    }

    private static string FormatBytes(long bytes)
    {
        if (bytes <= 0)
        {
            return "0 B";
        }

        string[] sizes = ["B", "KB", "MB", "GB"];
        var value = (double)bytes;
        var order = 0;
        while (value >= 1024 && order < sizes.Length - 1)
        {
            order++;
            value /= 1024;
        }

        return $"{value:0.#} {sizes[order]}";
    }
}
