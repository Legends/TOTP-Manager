using NetSparkleUpdater;
using NetSparkleUpdater.Enums;
using NetSparkleUpdater.Events;
using NetSparkleUpdater.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace TOTP.AutoUpdate;

public partial class TOTPUpdateAvailableWindow : Window, IUpdateAvailable
{
    private readonly List<AppCastItem> _updates;

    public event UserRespondedToUpdate? UserResponded;

    public UpdateAvailableResult Result { get; private set; }

    public AppCastItem CurrentItem { get; set; }

    public TOTPUpdateAvailableWindow(List<AppCastItem> updates, bool isUpdateAlreadyDownloaded)
    {
        _updates = updates ?? throw new ArgumentNullException(nameof(updates));
        CurrentItem = _updates.First();

        InitializeComponent();

        UpdatesList.ItemsSource = _updates;
        HeaderText.Text = isUpdateAlreadyDownloaded ? "Ready to install update" : "Update available";
        StatusChipText.Text = isUpdateAlreadyDownloaded ? "Downloaded" : "Verified";
        InstallButton.Content = isUpdateAlreadyDownloaded ? "Install update" : "Download update";
        ActionHintText.Text = isUpdateAlreadyDownloaded
            ? "The package is already present locally. Confirm to launch the installer."
            : "Confirm to download the selected package and then run the installer.";
        UpdatesList.SelectedItem = CurrentItem;
        ApplyCurrentItem(CurrentItem, isUpdateAlreadyDownloaded);
    }

    public void Show(bool isOnMainThread)
    {
        InvokeOnUi(() =>
        {
            ConfigureOwner();
            if (!IsVisible)
            {
                ShowDialog();
            }
        });
    }

    public void HideReleaseNotes()
    {
        ReleaseNotesTab.Visibility = Visibility.Collapsed;
    }

    public void HideRemindMeLaterButton()
    {
        LaterButton.Visibility = Visibility.Collapsed;
    }

    public void HideSkipButton()
    {
        SkipButton.Visibility = Visibility.Collapsed;
    }

    public void BringToFront()
    {
        InvokeOnUi(() =>
        {
            Activate();
            Topmost = true;
            Topmost = false;
            Focus();
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

    private void InstallButton_OnClick(object sender, RoutedEventArgs e)
    {
        SetResultAndClose(UpdateAvailableResult.InstallUpdate);
    }

    private void LaterButton_OnClick(object sender, RoutedEventArgs e)
    {
        SetResultAndClose(UpdateAvailableResult.RemindMeLater);
    }

    private void SkipButton_OnClick(object sender, RoutedEventArgs e)
    {
        SetResultAndClose(UpdateAvailableResult.SkipUpdate);
    }

    private void SetResultAndClose(UpdateAvailableResult result)
    {
        Result = result;
        UserResponded?.Invoke(this, new UpdateResponseEventArgs(result, CurrentItem));
        base.Close();
    }

    private void UpdatesList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (UpdatesList.SelectedItem is not AppCastItem item)
        {
            return;
        }

        CurrentItem = item;
        ApplyCurrentItem(item, Equals(InstallButton.Content, "Install update"));
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

    private static string BuildSummaryText(AppCastItem item, bool isUpdateAlreadyDownloaded)
    {
        var installedVersion = item.AppVersionInstalled?.ToString() ?? "unknown";
        var version = item.ShortVersion ?? item.Version?.ToString() ?? "unknown";
        if (isUpdateAlreadyDownloaded)
        {
            return $"Installed version {installedVersion} can now be replaced with {version}.";
        }

        return $"Version {version} is available for the current installation ({installedVersion}). The feed and signature are valid, and the updater is ready to download the selected package.";
    }

    private void ApplyCurrentItem(AppCastItem item, bool isUpdateAlreadyDownloaded)
    {
        InstalledVersionText.Text = $"Installed: {item.AppVersionInstalled?.ToString() ?? "unknown"}";
        SummaryText.Text = BuildSummaryText(item, isUpdateAlreadyDownloaded);
        CurrentVersionText.Text = $"Available: {item.ShortVersion ?? item.Version?.ToString() ?? "unknown"}";
        PublishDateText.Text = item.PublicationDate == default
            ? "Published: unknown"
            : $"Published: {item.PublicationDate.ToLocalTime():yyyy-MM-dd HH:mm}";
        PackageSizeText.Text = item.UpdateSize > 0
            ? $"Package size: {FormatBytes(item.UpdateSize)}"
            : "Package size: unknown";
        SourceText.Text = $"Source: {item.DownloadLink}";
        RenderReleaseNotes(item);
    }

    private void RenderReleaseNotes(AppCastItem item)
    {
        try
        {
            ReleaseNotesBrowser.NavigateToString(ReleaseNotesFormatter.ToHtmlDocument(item));
            ReleaseNotesBrowser.Visibility = Visibility.Visible;
            ReleaseNotesFallbackPanel.Visibility = Visibility.Collapsed;
        }
        catch
        {
            ReleaseNotesFallbackText.Text = ReleaseNotesFormatter.ToPlainText(item.Description);
            ReleaseNotesBrowser.Visibility = Visibility.Collapsed;
            ReleaseNotesFallbackPanel.Visibility = Visibility.Visible;
        }
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
