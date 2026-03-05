using System;
using Microsoft.Extensions.Logging;
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

        OpenSettingsCommand = new RelayCommand(OpenSettingsView);

        CopyCodeCommand = new RelayCommand<OtpViewModel>(model => CopyTotpCodeToClipboard());
        GenerateQrCommand = new RelayCommand<OtpViewModel>(model => GenerateQrCodeImage());
        ToggleQrPreviewCommand = new RelayCommand<System.Windows.Media.Imaging.BitmapSource?>(source => _qrPreviewService.Toggle(source));
        ExportSecretsCommand = new AsyncCommand(_accountTransferWorkflowService.ExportSecretsToFileAsync);
        ScanQrAndAddCommand = new AsyncCommand(ScanQrAndAddAccountAsync, () => !_isGridInEditMode);

        OpenFlyoutEditModeCommand = new RelayCommand<OtpViewModel>(OpenFlyoutEditMode);
        OpenFlyoutAddModeCommand = new RelayCommand(OpenFlyoutAddMode, () => !_isGridInEditMode);
        SaveEditFlyoutAsyncCommand = new AsyncCommand(AddOrUpdateOtpEntryAsync);
        CancelFlyoutCommand = new RelayCommand(CancelFlyout);

        RowSelectionChangedCommand = new AsyncCommand<OtpViewModel>(OnRowSelectionChangedAsync);
        DeleteSecretCommand = new AsyncCommand<OtpViewModel>(DeleteOtpEntryAsync, null, _logger);
        BeginEditCommand = new RelayCommand<OtpViewModel>(OnBeginEdit);
        EndEditCommand = new AsyncCommand<OtpViewModel>(OnEndEditAsync);
        DoubleClickCommand = new RelayCommand<OtpViewModel>(OnDoubleClick);

        ToggleSearchBoxCommand = new RelayCommand(() =>
        {
            IsSearchVisible = !IsSearchVisible;
            IsSearchFocused = IsSearchVisible;

            if (!IsSearchVisible)
                SearchText = string.Empty;

        }, () => !IsGridEditing);

        ClearSearchCommand = new RelayCommand(ClearSearchTextbox, () => IsSearchVisible);

    }

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
