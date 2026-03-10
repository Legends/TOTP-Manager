using NetSparkleUpdater;
using NetSparkleUpdater.Events;
using NetSparkleUpdater.Interfaces;
using System;
using System.Windows;

namespace TOTP.AutoUpdate;

public partial class TOTPDownloadProgressWindow : Window, IDownloadProgress
{
    private readonly AppCastItem _item;
    private bool _downloadFinished;
    private bool _downloadedFileValid;

    public event DownloadInstallEventHandler? DownloadProcessCompleted;

    public TOTPDownloadProgressWindow(AppCastItem item)
    {
        _item = item ?? throw new ArgumentNullException(nameof(item));
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
        InvokeOnUi(() =>
        {
            DownloadProgressBar.IsIndeterminate = false;
            DownloadProgressBar.Value = Math.Clamp(args.ProgressPercentage, 0, 100);
            ProgressStateText.Text = "Downloading";
            ProgressText.Text = $"{args.ProgressPercentage}% downloaded ({FormatBytes(args.BytesReceived)} / {FormatBytes(args.TotalBytesToReceive)})";
        });
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

        InvokeOnUi(() =>
        {
            DownloadProgressBar.Value = 100;
            ActionButton.IsEnabled = true;

            if (isDownloadedFileValid)
            {
                TitleText.Text = "Update ready to install";
                ProgressStateText.Text = "Ready";
                ProgressText.Text = "The download completed and passed signature verification.";
                ActionButton.Content = "Install update";
                return;
            }

            TitleText.Text = "Download failed verification";
            ProgressStateText.Text = "Blocked";
            ProgressText.Text = "The downloaded file did not pass validation.";
            ActionButton.Content = "Close";
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

    private void ActionButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (!_downloadFinished || !_downloadedFileValid)
        {
            Close();
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
