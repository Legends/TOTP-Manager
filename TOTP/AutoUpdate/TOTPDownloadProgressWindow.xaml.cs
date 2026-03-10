using NetSparkleUpdater;
using NetSparkleUpdater.Events;
using NetSparkleUpdater.Interfaces;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace TOTP.AutoUpdate;

public partial class TOTPDownloadProgressWindow : Window, IDownloadProgress
{
    private readonly AppCastItem _item;
    private readonly Func<AppCastItem, string?, Task<bool>>? _customInstallHandler;
    private readonly DateTimeOffset _downloadWindowCreatedAtUtc = DateTimeOffset.UtcNow;
    private static readonly TimeSpan MinimumReadyDelay = TimeSpan.FromMilliseconds(900);
    private bool _downloadFinished;
    private bool _downloadedFileValid;
    private bool _readyStateApplied;
    private int _lastProgressPercentage;
    private string? _downloadedFilePath;

    public event DownloadInstallEventHandler? DownloadProcessCompleted;

    public TOTPDownloadProgressWindow(AppCastItem item, Func<AppCastItem, string?, Task<bool>>? customInstallHandler = null)
    {
        _item = item ?? throw new ArgumentNullException(nameof(item));
        _customInstallHandler = customInstallHandler;
        InitializeComponent();

        VersionText.Text = $"Version {_item.ShortVersion ?? _item.Version?.ToString() ?? "unknown"} from {_item.DownloadLink}";
        DownloadInfoText.Text = _item.UpdateSize > 0
            ? $"Expected package size: {FormatBytes(_item.UpdateSize)}"
            : "Expected package size: unknown";
        ActionButton.IsEnabled = false;
    }

    public void SetDownloadAndInstallButtonEnabled(bool shouldBeEnabled)
    {
        InvokeOnUi(() => ActionButton.IsEnabled = shouldBeEnabled);
    }

    public void SetDownloadedFilePath(string? downloadedFilePath)
    {
        _downloadedFilePath = downloadedFilePath;
    }

    public void Show(bool isOnMainThread)
    {
        InvokeOnUi(() =>
        {
            ConfigureOwner();
            if (!IsVisible)
            {
                base.Show();
            }

            Activate();
        });
    }

    public void OnDownloadProgressChanged(object sender, ItemDownloadProgressEventArgs args)
    {
        _lastProgressPercentage = Math.Clamp(args.ProgressPercentage, 0, 100);

        InvokeOnUi(() =>
        {
            DownloadProgressBar.IsIndeterminate = false;
            DownloadProgressBar.Value = _lastProgressPercentage;
            ProgressStateText.Text = "Downloading";
            ProgressText.Text = $"{args.ProgressPercentage}% downloaded ({FormatBytes(args.BytesReceived)} / {FormatBytes(args.TotalBytesToReceive)})";
        });

        if (_downloadFinished)
        {
            _ = TryApplyFinishedStateAsync();
        }
    }

    public new void Close()
    {
        InvokeOnUi(() =>
        {
            if (IsVisible)
            {
                base.Close();
            }
        });
    }

    public void FinishedDownloadingFile(bool isDownloadedFileValid)
    {
        _downloadFinished = true;
        _downloadedFileValid = isDownloadedFileValid;
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
                if (_lastProgressPercentage < 100)
                {
                    DownloadProgressBar.IsIndeterminate = true;
                    ProgressStateText.Text = "Finalizing";
                    ProgressText.Text = "Download completed. Finishing verification and preparing install...";
                    return;
                }

                DownloadProgressBar.IsIndeterminate = false;
                DownloadProgressBar.Value = 100;
                ActionButton.IsEnabled = true;
                TitleText.Text = "Update ready to install";
                ProgressStateText.Text = "Ready";
                ProgressText.Text = "The download completed and passed signature verification.";
                ActionButton.Content = "Install update";
                _readyStateApplied = true;
                return;
            }

            DownloadProgressBar.IsIndeterminate = false;
            DownloadProgressBar.Value = 100;
            ActionButton.IsEnabled = true;
            TitleText.Text = "Download failed verification";
            ProgressStateText.Text = "Blocked";
            ProgressText.Text = "The downloaded file did not pass validation.";
            ActionButton.Content = "Close";
            _readyStateApplied = true;
        });
    }

    public bool DisplayErrorMessage(string errorMessage)
    {
        InvokeOnUi(() =>
        {
            ProgressStateText.Text = "Error";
            ErrorText.Text = errorMessage;
            ErrorText.Visibility = Visibility.Visible;
            ActionButton.IsEnabled = true;
            ActionButton.Content = "Close";
        });

        return true;
    }

    private async void ActionButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (!_downloadFinished || !_downloadedFileValid)
        {
            Close();
            return;
        }

        if (_customInstallHandler != null)
        {
            ActionButton.IsEnabled = false;
            ProgressStateText.Text = "Installing";
            ProgressText.Text = "Preparing the update package and replacing the current app files...";

            var wasHandled = await _customInstallHandler(_item, _downloadedFilePath);
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
            ProgressStateText.Text = "Ready";
            ProgressText.Text = "The update helper could not start. You can try again or close this window.";
            return;
        }

        DownloadProcessCompleted?.Invoke(this, new DownloadInstallEventArgs(true));
        Close();
    }

    private void ConfigureOwner()
    {
        if (Owner == null && Application.Current?.MainWindow is Window mainWindow && !ReferenceEquals(mainWindow, this))
        {
            Owner = mainWindow;
        }
    }

    private void InvokeOnUi(Action action)
    {
        var dispatcher = Application.Current?.Dispatcher ?? Dispatcher;
        if (dispatcher.CheckAccess())
        {
            action();
            return;
        }

        dispatcher.Invoke(action);
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
