using NetSparkleUpdater;
using NetSparkleUpdater.Enums;
using NetSparkleUpdater.Events;
using NetSparkleUpdater.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TOTP.Resources;

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
        HeaderText.Text = isUpdateAlreadyDownloaded ? UI.ui_Updater_Available_Header_Ready : UI.ui_Updater_Available_Header;
        StatusChipText.Text = isUpdateAlreadyDownloaded ? UI.ui_Updater_Available_State_Downloaded : UI.ui_Updater_Available_State_Verified;
        InstallButton.Content = isUpdateAlreadyDownloaded ? UI.ui_Updater_Available_Button_Install : UI.ui_Updater_Available_Button_Download;
        ActionHintText.Text = isUpdateAlreadyDownloaded
            ? UI.ui_Updater_Available_ActionHint_Install
            : UI.ui_Updater_Available_ActionHint_Download;
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
        ApplyCurrentItem(item, Equals(InstallButton.Content, UI.ui_Updater_Available_Button_Install));
    }

    private void ConfigureOwner()
    {
        if (Owner == null && Application.Current?.MainWindow is Window mainWindow && !ReferenceEquals(mainWindow, this))
        {
            if (mainWindow.IsVisible)
            {
                Owner = mainWindow;
            }
            else
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
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
        var installedVersion = item.AppVersionInstalled?.ToString() ?? UI.ui_Updater_Common_Unknown;
        var version = item.ShortVersion ?? item.Version?.ToString() ?? UI.ui_Updater_Common_Unknown;
        if (isUpdateAlreadyDownloaded)
        {
            return string.Format(UI.ui_Updater_Available_Summary_Install_Format, installedVersion, version);
        }

        return string.Format(UI.ui_Updater_Available_Summary_Download_Format, version, installedVersion);
    }

    private void ApplyCurrentItem(AppCastItem item, bool isUpdateAlreadyDownloaded)
    {
        InstalledVersionText.Text = string.Format(
            UI.ui_Updater_Available_InstalledVersion_Format,
            item.AppVersionInstalled?.ToString() ?? UI.ui_Updater_Common_Unknown);
        SummaryText.Text = BuildSummaryText(item, isUpdateAlreadyDownloaded);
        CurrentVersionText.Text = string.Format(
            UI.ui_Updater_Available_CurrentVersion_Format,
            item.ShortVersion ?? item.Version?.ToString() ?? UI.ui_Updater_Common_Unknown);
        PublishDateText.Text = item.PublicationDate == default
            ? string.Format(UI.ui_Updater_Available_Published_Format, UI.ui_Updater_Common_Unknown)
            : string.Format(UI.ui_Updater_Available_Published_Format, item.PublicationDate.ToLocalTime().ToString("yyyy-MM-dd HH:mm"));
        PackageSizeText.Text = item.UpdateSize > 0
            ? string.Format(UI.ui_Updater_Available_PackageSize_Format, FormatBytes(item.UpdateSize))
            : string.Format(UI.ui_Updater_Available_PackageSize_Format, UI.ui_Updater_Common_Unknown);
        SourceText.Text = string.Format(UI.ui_Updater_Available_Source_Format, item.DownloadLink);
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
