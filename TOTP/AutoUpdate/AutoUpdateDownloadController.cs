using NetSparkleUpdater;
using NetSparkleUpdater.Events;
using System;
using System.Threading.Tasks;
using TOTP.Resources;

namespace TOTP.AutoUpdate;

internal sealed class AutoUpdateDownloadController(
    AutoUpdateDialogState state,
    Action<Action> invokeOnUi,
    Action ensurePresented)
{
    private static readonly TimeSpan MinimumReadyDelay = TimeSpan.FromMilliseconds(900);
    private DateTimeOffset _startedAtUtc;
    private bool _finished;
    private bool _downloadedFileValid;
    private bool _readyStateApplied;
    private bool _terminalErrorDisplayed;
    private int _lastProgressPercentage;

    internal AppCastItem? CurrentItem { get; private set; }
    internal string? DownloadedFilePath { get; set; }

    internal void ShowProgress(AppCastItem item)
    {
        CurrentItem = item;
        DownloadedFilePath = null;
        _startedAtUtc = DateTimeOffset.UtcNow;
        _finished = false;
        _downloadedFileValid = false;
        _readyStateApplied = false;
        _terminalErrorDisplayed = false;
        _lastProgressPercentage = 0;
        state.ShowProgress(item);
    }

    internal void SetActionEnabled(bool enabled)
    {
        if ((_readyStateApplied || _terminalErrorDisplayed) && !enabled)
            return;

        invokeOnUi(() => state.SetProgressActionEnabled(enabled));
    }

    internal void ProgressChanged(ItemDownloadProgressEventArgs args)
    {
        if (_terminalErrorDisplayed)
            return;

        _lastProgressPercentage = Math.Clamp(args.ProgressPercentage, 0, 100);
        invokeOnUi(() => state.SetProgress(
            _lastProgressPercentage,
            string.Format(
                UI.ui_Updater_Download_Progress_Format,
                args.ProgressPercentage,
                FormatBytes(args.BytesReceived),
                FormatBytes(args.TotalBytesToReceive))));

        if (_finished)
            _ = TryApplyFinishedStateAsync();
    }

    internal void Finished(bool isDownloadedFileValid)
    {
        if (_terminalErrorDisplayed)
            return;

        _finished = true;
        _downloadedFileValid = isDownloadedFileValid;
        if (isDownloadedFileValid)
            _lastProgressPercentage = 100;

        _ = TryApplyFinishedStateAsync();
    }

    internal bool DisplayError(string errorMessage)
    {
        _terminalErrorDisplayed = true;
        _finished = false;
        _downloadedFileValid = false;
        _readyStateApplied = true;

        invokeOnUi(() =>
        {
            state.SetProgressError(errorMessage, Math.Clamp(_lastProgressPercentage, 0, 100));
            ensurePresented();
        });
        return true;
    }

    private async Task TryApplyFinishedStateAsync()
    {
        if (_readyStateApplied || !_finished)
            return;

        var remainingDelay = MinimumReadyDelay - (DateTimeOffset.UtcNow - _startedAtUtc);
        if (remainingDelay > TimeSpan.Zero)
            await Task.Delay(remainingDelay);

        invokeOnUi(() =>
        {
            if (_readyStateApplied)
                return;

            _readyStateApplied = true;
            if (_downloadedFileValid)
                state.SetProgressReady();
            else
                state.SetProgressBlocked();
        });
    }

    private static string FormatBytes(long bytes)
    {
        if (bytes <= 0)
            return "0 B";

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
