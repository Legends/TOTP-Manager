using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
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
        ExportSecretsCommand = new AsyncCommand(ExportSecretsToFile);
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

    private async Task ExportOtps(bool toBeEncrypted)
    {
        try
        {
            System.Windows.MessageBox.Show(toBeEncrypted.ToString());
            var path = _fileDialogService.ShowSaveFileDialog(
                "JSON Files|*.json|Text Files|*.txt|CSV Files|*.csv",
                ".json",
                "otp-backup"
            );

            if (path == null)
                return;

            var result = await _accountsWorkflow.GetAllEntriesSortedAsync();
            if (result.IsFailed)
            {
                _messageService.ShowResultError(result);
                return;
            }

            var options = new JsonSerializerOptions { WriteIndented = true };
            await File.WriteAllTextAsync(path, JsonSerializer.Serialize(result.Value, options));

            var psi = new ProcessStartInfo { FileName = path, UseShellExecute = true };

            Process.Start(psi);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }
}
