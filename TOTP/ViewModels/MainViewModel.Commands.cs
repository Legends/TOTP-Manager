using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using TOTP.Commands;

namespace TOTP.ViewModels;

public partial class MainViewModel
{
    #region ### COMMANDS EVENTHANDLER ###

    private void SetupCommandEventhandler()
    {
        CloseSettingsViewCommand = new RelayCommand(
            CloseSettingsView,
            () => IsSettingsViewOpen);

        OpenSettingsCommand = new RelayCommand(OpenSettingsView, CanOpenSettings);
        CheckForUpdatesCommand = new AsyncCommand(CheckForUpdatesAsync, CanCheckForUpdates, _logger);
        ShowAboutCommand = new RelayCommand(ShowAbout);

        CopyCodeCommand = new RelayCommand<OtpViewModel>(
            _ => CopyTotpCodeToClipboard(),
            _ => CanCopyCode());

        GenerateQrCommand = new RelayCommand<OtpViewModel>(
            _ => GenerateQrCodeImage(),
            _ => CanGenerateQr());

        ToggleQrPreviewCommand = new RelayCommand<System.Windows.Media.Imaging.BitmapSource?>(
            source => _qrPreviewService.Toggle(source),
            source => source != null);

        ExportSecretsCommand = new AsyncCommand(_accountTransferWorkflowService.ExportSecretsToFileAsync, CanExportSecrets);
        ScanQrAndAddCommand = new AsyncCommand(ScanQrAndAddAccountAsync, () => !_isGridInEditMode);

        OpenFlyoutEditModeCommand = new RelayCommand<OtpViewModel>(OpenFlyoutEditMode, CanOpenFlyoutEditMode);
        OpenFlyoutAddModeCommand = new RelayCommand(OpenFlyoutAddMode, () => !_isGridInEditMode);
        SaveEditFlyoutAsyncCommand = new AsyncCommand(AddOrUpdateAccountAsync, CanSaveEditFlyout);
        CancelFlyoutCommand = new RelayCommand(CancelFlyout, () => IsEditAddFlyoutOpen);

        RowSelectionChangedCommand = new AsyncCommand<OtpViewModel>(OnRowSelectionChangedAsync, CanProcessRowSelection);
        DeleteSecretCommand = new AsyncCommand<OtpViewModel>(DeleteAccountAsync, CanDeleteSecret, _logger);
        BeginEditCommand = new RelayCommand<OtpViewModel>(OnBeginEdit, CanBeginEdit);
        EndEditCommand = new AsyncCommand<OtpViewModel>(OnEndEditAsync, CanEndEdit);
        DoubleClickCommand = new RelayCommand<OtpViewModel>(OnDoubleClick, CanHandleDoubleClick);

        ToggleSearchBoxCommand = new RelayCommand(() =>
        {
            IsSearchVisible = !IsSearchVisible;
            IsSearchFocused = IsSearchVisible;

            if (!IsSearchVisible)
                SearchText = string.Empty;

        }, () => !IsGridEditing);

        ClearSearchCommand = new RelayCommand(ClearSearchTextbox, () => IsSearchVisible);
    }

    private bool CanOpenSettings() => !IsSettingsViewOpen && IsUnlocked && !_isOpeningSettings;

    private bool CanCopyCode() =>
        SelectedAccount != null &&
        !string.IsNullOrWhiteSpace(TotpCode);

    private bool CanGenerateQr() =>
        SelectedAccount != null &&
        !string.IsNullOrWhiteSpace(SelectedAccount.Secret);

    private bool CanExportSecrets() =>
        IsUnlocked &&
        !IsGridEditing &&
        AllOtps.Count > 0;

    private bool CanCheckForUpdates() =>
        IsUnlocked &&
        !IsBusy &&
        !IsGridEditing &&
        !_isOpeningSettings;

    private bool CanOpenFlyoutEditMode(OtpViewModel? item) =>
        item != null &&
        !IsGridEditing &&
        !IsEditAddFlyoutOpen;

    private bool CanSaveEditFlyout() =>
        IsEditAddFlyoutOpen &&
        CurrentSecretBeingEditedOrAdded != null;

    private bool CanProcessRowSelection(OtpViewModel? item) =>
        item != null &&
        !IsGridEditing;

    private bool CanDeleteSecret(OtpViewModel? item) =>
        item != null &&
        !IsGridEditing;

    private bool CanBeginEdit(OtpViewModel? item) =>
        item != null &&
        !IsEditAddFlyoutOpen;

    private bool CanEndEdit(OtpViewModel? item) =>
        item != null;

    private bool CanHandleDoubleClick(OtpViewModel? item) =>
        item != null &&
        !IsGridEditing;

    private void OpenSettingsView()
    {
        _ = OpenSettingsViewAsync();
    }

    private async Task OpenSettingsViewAsync()
    {
        if (IsSettingsViewOpen)
        {
            return;
        }

        var settingsVm = await EnsureSettingsViewModelLoadedAsync(showErrorOnFailure: true);
        if (settingsVm == null)
        {
            return;
        }

        IsSettingsViewOpen = true;
        settingsVm.RequestFocus();
    }

    private void CloseSettingsView()
    {
        IsSettingsViewOpen = false;
    }

    private async Task CheckForUpdatesAsync()
    {
        await _autoUpdateService.CheckForUpdatesInteractiveAsync();
    }

    private void ShowAbout()
    {
        var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
        var location = assembly.Location;
        var informationalVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        var assemblyVersion = assembly.GetName().Version?.ToString() ?? "unknown";
        var productVersion = string.Empty;

        if (!string.IsNullOrWhiteSpace(location))
        {
            var versionInfo = FileVersionInfo.GetVersionInfo(location);
            productVersion = versionInfo.ProductVersion ?? string.Empty;
        }

        var displayVersion = !string.IsNullOrWhiteSpace(productVersion)
            ? productVersion
            : !string.IsNullOrWhiteSpace(informationalVersion)
                ? informationalVersion
                : assemblyVersion;

        MessageBox.Show(
            $"TOTP Manager{Environment.NewLine}{Environment.NewLine}Installed version: {displayVersion}",
            "About TOTP Manager",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    #endregion COMMANDS SETUP

    private void SaveSettingsView()
    {
        IsSettingsViewOpen = false;
    }
}

