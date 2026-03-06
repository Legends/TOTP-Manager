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

        CopyCodeCommand = new RelayCommand<OtpViewModel>(
            _ => CopyTotpCodeToClipboard(),
            _ => CanCopyCode());

        GenerateQrCommand = new RelayCommand<OtpViewModel>(
            _ => GenerateQrCodeImage(),
            _ => CanGenerateQr());

        ToggleQrPreviewCommand = new RelayCommand<System.Windows.Media.Imaging.BitmapSource?>(
            source => _qrPreviewService.Toggle(source),
            source => source != null);

        ExportSecretsCommand = new AsyncCommand(_tokenTransferWorkflowService.ExportSecretsToFileAsync, CanExportSecrets);
        ScanQrAndAddCommand = new AsyncCommand(ScanQrAndAddTokenAsync, () => !_isGridInEditMode);

        OpenFlyoutEditModeCommand = new RelayCommand<OtpViewModel>(OpenFlyoutEditMode, CanOpenFlyoutEditMode);
        OpenFlyoutAddModeCommand = new RelayCommand(OpenFlyoutAddMode, () => !_isGridInEditMode);
        SaveEditFlyoutAsyncCommand = new AsyncCommand(AddOrUpdateOtpEntryAsync, CanSaveEditFlyout);
        CancelFlyoutCommand = new RelayCommand(CancelFlyout, () => IsEditAddFlyoutOpen);

        RowSelectionChangedCommand = new AsyncCommand<OtpViewModel>(OnRowSelectionChangedAsync, CanProcessRowSelection);
        DeleteSecretCommand = new AsyncCommand<OtpViewModel>(DeleteOtpEntryAsync, CanDeleteSecret, _logger);
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

    private bool CanOpenSettings() => !IsSettingsViewOpen && IsUnlocked;

    private bool CanCopyCode() =>
        SelectedToken != null &&
        !string.IsNullOrWhiteSpace(TotpCode);

    private bool CanGenerateQr() =>
        SelectedToken != null &&
        !string.IsNullOrWhiteSpace(SelectedToken.Secret);

    private bool CanExportSecrets() =>
        IsUnlocked &&
        !IsGridEditing &&
        AllOtps.Count > 0;

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
        IsSettingsViewOpen = true;
        SettingsVm.RequestFocus();
    }

    private void CloseSettingsView()
    {
        IsSettingsViewOpen = false;
    }

    #endregion COMMANDS SETUP

    private void SaveSettingsView()
    {
        IsSettingsViewOpen = false;
    }
}

