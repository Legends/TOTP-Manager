using System;
using System.Threading;
using System.Threading.Tasks;
using TOTP.Core.Security.Models;
using TOTP.Resources;

namespace TOTP.ViewModels;

public sealed partial class SettingsViewModel
{
    async Task SaveAndCloseAsync()
    {
        AuthError = null;

        if (!await ApplyAuthorizationSettingsAsync())
        {
            return;
        }

        await SaveGeneralSettingsAsync();
        _saveAction();
    }

    private async Task<bool> ApplyAuthorizationSettingsAsync()
    {
        var result = await _settingsAuthorizationWorkflowService.ApplyAuthorizationSettingsAsync(
            IsHelloSelected,
            IsHelloAvailable,
            NewPassword,
            ConfirmPassword);

        NewPasswordError = result.NewPasswordError;
        ConfirmPasswordError = result.ConfirmPasswordError;
        AuthError = result.ErrorMessage;

        if (result.ClearPasswordInputs)
        {
            NewPassword = string.Empty;
            ConfirmPassword = string.Empty;
        }

        return result.IsSuccess;
    }

    private void QueueAuthorizationGateSave(int delayMs = 120)
    {
        if (_isLoadingSettings || _suppressAuthAutoSave)
        {
            return;
        }

        _authGateDebounceCts?.Cancel();
        _authGateDebounceCts?.Dispose();
        _authGateDebounceCts = new CancellationTokenSource();
        _ = SaveAuthorizationGateDebouncedAsync(_authGateDebounceCts.Token, delayMs);
    }

    private async Task SaveAuthorizationGateDebouncedAsync(CancellationToken token, int delayMs)
    {
        try
        {
            await Task.Delay(delayMs, token);
            await ApplyAuthorizationGateSelectionAsync();
        }
        catch (TaskCanceledException)
        {
            // expected when toggling quickly
        }
        catch (Exception)
        {
            // Never let fire-and-forget debounce faults escape into UnobservedTaskException.
            AuthError = UI.ex_UnexpectedError;
        }
    }

    private async Task ApplyAuthorizationGateSelectionAsync()
    {
        AuthError = null;
        var result = await _settingsAuthorizationWorkflowService.ApplyAuthorizationGateSelectionAsync(
            IsHelloSelected,
            IsHelloAvailable);

        if (result is null)
        {
            AuthError = UI.ex_UnexpectedError;
            RevertAuthorizationGateSelectionFromCurrentSettings();
            return;
        }

        if (!result.IsSuccess)
        {
            AuthError = result.ErrorMessage;
            RevertAuthorizationGateSelectionFromCurrentSettings();
        }
    }

    private void RevertAuthorizationGateSelectionFromCurrentSettings()
    {
        var gate = _settingsSvc.Current?.Authorization?.Gate ?? AuthorizationGateKind.Password;
        _suppressAuthAutoSave = true;
        IsHelloSelected = gate == AuthorizationGateKind.Hello;
        IsPasswordSelected = gate == AuthorizationGateKind.Password;
        _suppressAuthAutoSave = false;
    }

    private async Task ChangePasswordAsync()
    {
        AuthError = null;
        var result = await _settingsAuthorizationWorkflowService.ChangePasswordAsync(
            NewPassword,
            ConfirmPassword);

        NewPasswordError = result.NewPasswordError;
        ConfirmPasswordError = result.ConfirmPasswordError;
        AuthError = result.ErrorMessage;

        if (!result.IsSuccess)
        {
            return;
        }

        if (result.ClearPasswordInputs)
        {
            NewPassword = string.Empty;
            ConfirmPassword = string.Empty;
        }

        _messageService.ShowSuccess(UI.ui_Password_ChangeSuccess, 2);
    }
}
