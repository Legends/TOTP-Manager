using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using TOTP.Commands;
using TOTP.Services.Interfaces;

namespace TOTP.ViewModels;

public sealed partial class SettingsViewModel
{
    private void ApplyGeneralSnapshot(SettingsGeneralSnapshot snapshot)
    {
        SelectedLogLevel = snapshot.SelectedLogLevel;
        LockOnSessionLock = snapshot.LockOnSessionLock;
        LockOnMinimize = snapshot.LockOnMinimize;
        LockOnIdleTimeout = snapshot.LockOnIdleTimeout;
        IdleTimeoutMinutes = snapshot.IdleTimeoutMinutes;
        ClearClipboardEnabled = snapshot.ClearClipboardEnabled;
        ClearClipboardSeconds = snapshot.ClearClipboardSeconds;
        QrPreviewScaleFactor = snapshot.QrPreviewScaleFactor;
        OpenExportFileAfterExport = snapshot.OpenExportFileAfterExportBeforeEncrypt;
        _exportOptions.SetOpenExportFileAfterExportBeforeEncrypt(snapshot.OpenExportFileAfterExportBeforeEncrypt);
        ExportEncrypt = snapshot.ExportEncrypt;
        OpenExportFileAfterExport = ExportEncrypt
            ? false
            : _exportOptions.OpenExportFileAfterExportBeforeEncrypt;
        UpdateAvailableExportFormats();
        HideSecretsByDefault = snapshot.HideSecretsByDefault;
    }

    private async Task SaveGeneralSettingsAsync()
    {
        var snapshot = new SettingsGeneralSnapshot(
            SelectedLogLevel,
            LockOnSessionLock,
            LockOnMinimize,
            LockOnIdleTimeout,
            IdleTimeoutMinutes,
            ClearClipboardEnabled,
            ClearClipboardSeconds,
            QrPreviewScaleFactor,
            ExportEncrypt,
            _exportOptions.OpenExportFileAfterExportBeforeEncrypt,
            HideSecretsByDefault);

        var saveResult = await _settingsPersistenceService.SaveGeneralSettingsAsync(snapshot);
        if (!saveResult.IsSuccess)
        {
            AuthError = saveResult.ErrorMessage;
            return;
        }

        if (saveResult.LogSwitchStateChanged)
        {
            OnPropertyChanged(nameof(IsCliOverrideActive));
            OnPropertyChanged(nameof(ClrOverrideText));
        }
    }

    private void QueueGeneralSettingsSave(int delayMs = 200)
    {
        if (_isLoadingSettings)
        {
            return;
        }

        _saveDebounceCts?.Cancel();
        _saveDebounceCts?.Dispose();
        _saveDebounceCts = new CancellationTokenSource();
        _ = SaveGeneralSettingsDebouncedAsync(_saveDebounceCts.Token, delayMs);
    }

    private async Task SaveGeneralSettingsDebouncedAsync(CancellationToken token, int delayMs)
    {
        try
        {
            await Task.Delay(delayMs, token);
            await SaveGeneralSettingsAsync();
        }
        catch (TaskCanceledException)
        {
            // expected when user changes multiple settings quickly
        }
    }

    private async Task ResetToDefaultsAsync()
    {
        _isLoadingSettings = true;
        try
        {
            ApplyGeneralSnapshot(_settingsPersistenceService.CreateDefaultGeneralSettings());
        }
        finally
        {
            _isLoadingSettings = false;
        }

        await SaveGeneralSettingsAsync();
        OnPropertyChanged(nameof(CanResetToDefaults));
        RaiseCommandStates();
    }

    private void UpdateAvailableExportFormats()
    {
        var formats = _exportOptions.AvailableFormats;

        AvailableExportFormats.Clear();
        foreach (var format in formats)
        {
            AvailableExportFormats.Add(format);
        }

        if (!AvailableExportFormats.Contains(SelectedExportFormat))
        {
            SelectedExportFormat = AvailableExportFormats.First();
            return;
        }

        // Keep ComboBox selection in sync after ItemsSource rebuild, even when the value did not change.
        OnPropertyChanged(nameof(SelectedExportFormat));
    }

    private void RaiseCommandStates()
    {
        if (ChangePasswordCommand is AsyncCommand changePasswordCommand)
        {
            changePasswordCommand.RaiseCanExecuteChanged();
        }

        if (ResetToDefaultsCommand is AsyncCommand resetToDefaultsCommand)
        {
            resetToDefaultsCommand.RaiseCanExecuteChanged();
        }
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        if (name == nameof(CanResetToDefaults) || name == nameof(NewPassword) || name == nameof(ConfirmPassword) || name == nameof(IsPasswordSelected))
        {
            RaiseCommandStates();
        }
    }
}
