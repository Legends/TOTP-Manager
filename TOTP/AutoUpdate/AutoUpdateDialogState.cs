using NetSparkleUpdater;
using NetSparkleUpdater.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TOTP.Commands;
using TOTP.Resources;

namespace TOTP.AutoUpdate;

internal sealed class AutoUpdateDialogState : INotifyPropertyChanged
{
    private AutoUpdateDialogStep _currentStep;
    private IReadOnlyList<UpdateOffer> _updates = Array.Empty<UpdateOffer>();
    private UpdateOffer? _selectedUpdate;
    private string _availableHeaderText = UI.ui_Updater_Available_Header;
    private string _availableSummaryText = string.Empty;
    private string _availableStatusText = UI.ui_Updater_Available_State_Ready;
    private string _availableActionHintText = string.Empty;
    private string _availableTrustText = string.Empty;
    private string _availableRecommendationText = string.Empty;
    private string _installedVersionText = string.Empty;
    private string _currentVersionText = string.Empty;
    private string _publishDateText = string.Empty;
    private string _packageSizeText = string.Empty;
    private string _sourceText = string.Empty;
    private string _releaseNotesPlainText = string.Empty;
    private string _releaseNotesHtml = string.Empty;
    private bool _releaseNotesHtmlEnabled = true;
    private bool _releaseNotesVisible = true;
    private bool _laterVisible = true;
    private bool _skipVisible = true;
    private string _installButtonText = UI.ui_Updater_Available_Button_Download;
    private string _progressTitleText = UI.ui_Updater_Download_Header;
    private string _progressStateText = UI.ui_Updater_Download_State_Downloading;
    private string _progressVersionText = string.Empty;
    private string _progressDownloadInfoText = string.Empty;
    private string _progressDescriptionText = UI.ui_Updater_Download_Waiting;
    private string _progressErrorText = string.Empty;
    private bool _progressErrorVisible;
    private bool _progressActionEnabled;
    private bool _progressActionBusy;
    private string _progressActionText = UI.ui_btnClose;
    private bool _progressSecondaryActionVisible;
    private string _progressSecondaryActionText = UI.ui_btnClose;
    private bool _progressIndeterminate = true;
    private double _progressValue;
    private bool _downloadInstallRequested;

    public AutoUpdateDialogState()
    {
        InstallCommand = new RelayCommand(() => TryRaiseUpdateResponse(UpdateAvailableResult.InstallUpdate));
        RemindLaterCommand = new RelayCommand(() => TryRaiseUpdateResponse(UpdateAvailableResult.RemindMeLater));
        SkipCommand = new RelayCommand(() => TryRaiseUpdateResponse(UpdateAvailableResult.SkipUpdate));
        ProgressActionCommand = new RelayCommand(() => ProgressActionRequested?.Invoke());
        ProgressSecondaryActionCommand = new RelayCommand(() => ProgressSecondaryActionRequested?.Invoke());
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public event Action<UpdateAvailableResult, AppCastItem>? UpdateResponseRequested;

    public event Action? ProgressActionRequested;

    public event Action? ProgressSecondaryActionRequested;

    public ICommand InstallCommand { get; }

    public ICommand RemindLaterCommand { get; }

    public ICommand SkipCommand { get; }

    public ICommand ProgressActionCommand { get; }

    public ICommand ProgressSecondaryActionCommand { get; }

    public AutoUpdateDialogStep CurrentStep
    {
        get => _currentStep;
        private set
        {
            if (SetProperty(ref _currentStep, value))
            {
                OnPropertyChanged(nameof(IsCheckingVisible));
                OnPropertyChanged(nameof(IsAvailableVisible));
                OnPropertyChanged(nameof(IsProgressVisible));
            }
        }
    }

    public bool IsCheckingVisible => CurrentStep == AutoUpdateDialogStep.Checking;
    public bool IsAvailableVisible => CurrentStep == AutoUpdateDialogStep.Available;
    public bool IsProgressVisible => CurrentStep == AutoUpdateDialogStep.Progress;

    public IReadOnlyList<UpdateOffer> Updates
    {
        get => _updates;
        private set => SetProperty(ref _updates, value);
    }

    public UpdateOffer? SelectedUpdate
    {
        get => _selectedUpdate;
        set
        {
            if (SetProperty(ref _selectedUpdate, value) && value != null)
            {
                UpdateAvailableTexts(value, IsDownloadReady);
            }
        }
    }

    public bool IsDownloadReady { get; private set; }

    public string AvailableHeaderText { get => _availableHeaderText; private set => SetProperty(ref _availableHeaderText, value); }
    public string AvailableSummaryText { get => _availableSummaryText; private set => SetProperty(ref _availableSummaryText, value); }
    public string AvailableStatusText { get => _availableStatusText; private set => SetProperty(ref _availableStatusText, value); }
    public string AvailableActionHintText { get => _availableActionHintText; private set => SetProperty(ref _availableActionHintText, value); }
    public string AvailableTrustText { get => _availableTrustText; private set => SetProperty(ref _availableTrustText, value); }
    public string AvailableRecommendationText { get => _availableRecommendationText; private set => SetProperty(ref _availableRecommendationText, value); }
    public string InstalledVersionText { get => _installedVersionText; private set => SetProperty(ref _installedVersionText, value); }
    public string CurrentVersionText { get => _currentVersionText; private set => SetProperty(ref _currentVersionText, value); }
    public string PublishDateText { get => _publishDateText; private set => SetProperty(ref _publishDateText, value); }
    public string PackageSizeText { get => _packageSizeText; private set => SetProperty(ref _packageSizeText, value); }
    public string SourceText { get => _sourceText; private set => SetProperty(ref _sourceText, value); }
    public string ReleaseNotesPlainText { get => _releaseNotesPlainText; private set => SetProperty(ref _releaseNotesPlainText, value); }
    public string ReleaseNotesHtml { get => _releaseNotesHtml; private set => SetProperty(ref _releaseNotesHtml, value); }
    public bool ReleaseNotesHtmlEnabled { get => _releaseNotesHtmlEnabled; private set => SetProperty(ref _releaseNotesHtmlEnabled, value); }
    public bool ReleaseNotesVisible { get => _releaseNotesVisible; private set => SetProperty(ref _releaseNotesVisible, value); }
    public bool LaterVisible { get => _laterVisible; private set => SetProperty(ref _laterVisible, value); }
    public bool SkipVisible { get => _skipVisible; private set => SetProperty(ref _skipVisible, value); }
    public string InstallButtonText { get => _installButtonText; private set => SetProperty(ref _installButtonText, value); }

    public string ProgressTitleText { get => _progressTitleText; private set => SetProperty(ref _progressTitleText, value); }
    public string ProgressStateText { get => _progressStateText; private set => SetProperty(ref _progressStateText, value); }
    public string ProgressVersionText { get => _progressVersionText; private set => SetProperty(ref _progressVersionText, value); }
    public string ProgressDownloadInfoText { get => _progressDownloadInfoText; private set => SetProperty(ref _progressDownloadInfoText, value); }
    public string ProgressDescriptionText { get => _progressDescriptionText; private set => SetProperty(ref _progressDescriptionText, value); }
    public string ProgressErrorText { get => _progressErrorText; private set => SetProperty(ref _progressErrorText, value); }
    public bool ProgressErrorVisible { get => _progressErrorVisible; private set => SetProperty(ref _progressErrorVisible, value); }
    public bool ProgressActionEnabled { get => _progressActionEnabled; private set => SetProperty(ref _progressActionEnabled, value); }
    public bool ProgressActionBusy { get => _progressActionBusy; private set => SetProperty(ref _progressActionBusy, value); }
    public string ProgressActionText { get => _progressActionText; private set => SetProperty(ref _progressActionText, value); }
    public bool ProgressSecondaryActionVisible { get => _progressSecondaryActionVisible; private set => SetProperty(ref _progressSecondaryActionVisible, value); }
    public string ProgressSecondaryActionText { get => _progressSecondaryActionText; private set => SetProperty(ref _progressSecondaryActionText, value); }
    public bool ProgressIndeterminate { get => _progressIndeterminate; private set => SetProperty(ref _progressIndeterminate, value); }
    public double ProgressValue { get => _progressValue; private set => SetProperty(ref _progressValue, value); }

    public void ShowChecking()
    {
        CurrentStep = AutoUpdateDialogStep.Checking;
    }

    public void ShowAvailable(
        List<AppCastItem> updates,
        bool isUpdateAlreadyDownloaded,
        bool hideReleaseNotes,
        bool hideRemindMeLaterButton,
        bool hideSkipButton)
    {
        Updates = UpdateOffer.CreateMany(updates);
        IsDownloadReady = isUpdateAlreadyDownloaded;
        AvailableHeaderText = isUpdateAlreadyDownloaded ? UI.ui_Updater_Available_Header_Ready : UI.ui_Updater_Available_Header;
        AvailableStatusText = isUpdateAlreadyDownloaded ? UI.ui_Updater_Available_State_Downloaded : UI.ui_Updater_Available_State_Verified;
        InstallButtonText = isUpdateAlreadyDownloaded ? UI.ui_Updater_Available_Button_Install : UI.ui_Updater_Available_Button_Download;
        AvailableActionHintText = isUpdateAlreadyDownloaded
            ? UI.ui_Updater_Available_ActionHint_Install
            : UI.ui_Updater_Available_ActionHint_Download;
        ReleaseNotesVisible = !hideReleaseNotes;
        LaterVisible = !hideRemindMeLaterButton;
        SkipVisible = !hideSkipButton;
        CurrentStep = AutoUpdateDialogStep.Available;
        SelectedUpdate = Updates.Count > 0 ? Updates[0] : null;
    }

    public void ShowProgress(AppCastItem item)
    {
        CurrentStep = AutoUpdateDialogStep.Progress;
        _downloadInstallRequested = false;
        ProgressTitleText = UI.ui_Updater_Download_Header;
        ProgressStateText = UI.ui_Updater_Download_State_Downloading;
        ProgressVersionText = string.Format(
            UI.ui_Updater_Download_VersionInfo_Format,
            item.ShortVersion ?? item.Version?.ToString() ?? UI.ui_Updater_Common_Unknown,
            item.DownloadLink);
        ProgressDownloadInfoText = item.UpdateSize > 0
            ? string.Format(UI.ui_Updater_Download_ExpectedSize_Format, FormatBytes(item.UpdateSize))
            : string.Format(UI.ui_Updater_Download_ExpectedSize_Format, UI.ui_Updater_Common_Unknown);
        ProgressDescriptionText = UI.ui_Updater_Download_Waiting;
        ProgressErrorText = string.Empty;
        ProgressErrorVisible = false;
        ProgressActionBusy = false;
        ProgressActionText = UI.ui_btnCancel;
        ProgressSecondaryActionVisible = false;
        ProgressSecondaryActionText = UI.ui_btnClose;
        ProgressActionEnabled = true;
        ProgressIndeterminate = true;
        ProgressValue = 0;
    }

    public void SetProgress(double value, string description)
    {
        ProgressIndeterminate = false;
        ProgressValue = value;
        ProgressStateText = UI.ui_Updater_Download_State_Downloading;
        ProgressDescriptionText = description;
    }

    public void SetProgressReady()
    {
        ProgressIndeterminate = false;
        ProgressValue = 100;
        ProgressActionEnabled = true;
        ProgressActionBusy = false;
        ProgressTitleText = UI.ui_Updater_Download_Ready_Header;
        ProgressStateText = UI.ui_Updater_Available_State_Ready;
        ProgressDescriptionText = UI.ui_Updater_Download_Ready_Description;
        ProgressActionText = UI.ui_Updater_Available_Button_Install;
        ProgressSecondaryActionVisible = true;
        ProgressSecondaryActionText = UI.ui_Updater_Download_Ready_Button_Later;
        ProgressErrorVisible = false;
        _downloadInstallRequested = true;
    }

    public void SetProgressBlocked()
    {
        ProgressIndeterminate = false;
        ProgressValue = 100;
        ProgressActionEnabled = true;
        ProgressActionBusy = false;
        ProgressTitleText = UI.ui_Updater_Download_Blocked_Header;
        ProgressStateText = UI.ui_Updater_Download_State_Blocked;
        ProgressDescriptionText = UI.ui_Updater_Download_Blocked_Description;
        ProgressActionText = UI.ui_btnClose;
        ProgressSecondaryActionVisible = false;
        ProgressErrorVisible = false;
        _downloadInstallRequested = false;
    }

    public void SetProgressError(string errorMessage, double progressValue)
    {
        ProgressIndeterminate = false;
        ProgressValue = progressValue;
        ProgressTitleText = UI.ui_Updater_Download_Error_Header;
        ProgressStateText = UI.ui_Updater_Download_State_Error;
        ProgressDescriptionText = UI.ui_Updater_Download_Error_Description;
        ProgressErrorText = errorMessage;
        ProgressErrorVisible = true;
        ProgressActionEnabled = true;
        ProgressActionBusy = false;
        ProgressActionText = UI.ui_btnClose;
        ProgressSecondaryActionVisible = false;
        _downloadInstallRequested = false;
    }

    public void SetProgressActionEnabled(bool enabled)
    {
        if (_downloadInstallRequested && !enabled)
        {
            return;
        }

        ProgressActionEnabled = enabled;
    }

    public void ShowInstallingState()
    {
        ProgressActionEnabled = false;
        ProgressActionBusy = true;
        ProgressStateText = UI.ui_Updater_Download_State_Installing;
        ProgressDescriptionText = UI.ui_Updater_Download_Installing_Description;
        ProgressSecondaryActionVisible = false;
    }

    public void ShowHelperFailedState()
    {
        ProgressActionEnabled = true;
        ProgressActionBusy = false;
        ProgressStateText = UI.ui_Updater_Available_State_Ready;
        ProgressDescriptionText = UI.ui_Updater_Download_HelperFailed_Description;
        ProgressSecondaryActionVisible = false;
    }

    public void HandleProgressActionRequested()
    {
        ProgressActionRequested?.Invoke();
    }

    private void TryRaiseUpdateResponse(UpdateAvailableResult result)
    {
        var selectedItem = SelectedUpdate?.Item;
        if (selectedItem == null)
        {
            return;
        }

        UpdateResponseRequested?.Invoke(result, selectedItem);
    }

    private void UpdateAvailableTexts(UpdateOffer offer, bool isUpdateAlreadyDownloaded)
    {
        var item = offer.Item;
        var installedVersion = item.AppVersionInstalled?.ToString() ?? UI.ui_Updater_Common_Unknown;
        var version = offer.ShortVersion;
        var downloadHost = offer.SourceHost;
        AvailableSummaryText = isUpdateAlreadyDownloaded
            ? string.Format(UI.ui_Updater_Available_Summary_Install_Format, installedVersion, version)
            : string.Format(UI.ui_Updater_Available_Summary_Download_Format, version, installedVersion);
        InstalledVersionText = string.Format(UI.ui_Updater_Available_InstalledVersion_Format, installedVersion);
        CurrentVersionText = string.Format(UI.ui_Updater_Available_CurrentVersion_Format, version);
        AvailableTrustText = string.Format(UI.ui_Updater_Available_Trust_Format, downloadHost);
        AvailableRecommendationText = offer.IsRecommended
            ? UI.ui_Updater_Available_Recommendation_Primary
            : UI.ui_Updater_Available_Recommendation_Alternate;
        PublishDateText = item.PublicationDate == default
            ? string.Format(UI.ui_Updater_Available_Published_Format, UI.ui_Updater_Common_Unknown)
            : string.Format(UI.ui_Updater_Available_Published_Format, item.PublicationDate.ToLocalTime().ToString("yyyy-MM-dd HH:mm"));
        PackageSizeText = item.UpdateSize > 0
            ? string.Format(UI.ui_Updater_Available_PackageSize_Format, FormatBytes(item.UpdateSize))
            : string.Format(UI.ui_Updater_Available_PackageSize_Format, UI.ui_Updater_Common_Unknown);
        SourceText = string.Format(UI.ui_Updater_Available_Source_Format, item.DownloadLink);

        try
        {
            ReleaseNotesHtml = ReleaseNotesFormatter.ToHtmlDocument(item);
            ReleaseNotesPlainText = string.Empty;
            ReleaseNotesHtmlEnabled = true;
        }
        catch
        {
            ReleaseNotesPlainText = ReleaseNotesFormatter.ToPlainText(item.Description);
            ReleaseNotesHtmlEnabled = false;
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

    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
