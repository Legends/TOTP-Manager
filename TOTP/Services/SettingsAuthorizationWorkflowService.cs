using System.Linq;
using System.Threading.Tasks;
using TOTP.Core.Security.Interfaces;
using TOTP.Core.Security.Models;
using TOTP.Resources;
using TOTP.Services.Interfaces;

namespace TOTP.Services;

public sealed class SettingsAuthorizationWorkflowService(
    IAuthorizationService authorizationService,
    ISettingsService settingsService,
    IPasswordValidationService passwordValidationService) : ISettingsAuthorizationWorkflowService
{
    private readonly IAppSettings _appSettings = settingsService.Current;

    public async Task<SettingsAuthorizationWorkflowResult> ApplyAuthorizationSettingsAsync(
        bool isHelloSelected,
        bool isHelloAvailable,
        string newPassword,
        string confirmPassword)
    {
        var currentGate = _appSettings.Authorization.Gate;

        if (isHelloSelected && currentGate != AuthorizationGateKind.Hello)
        {
            if (!isHelloAvailable)
            {
                return new SettingsAuthorizationWorkflowResult(false, "Windows Hello is not supported on this device.");
            }

            if (!_appSettings.Authorization.HasHelloSetup)
            {
                var setupResult = await authorizationService.ConfigureHelloAsync();
                if (setupResult != AuthorizationResult.Success)
                {
                    return new SettingsAuthorizationWorkflowResult(false, "Windows Hello setup failed.");
                }
            }
        }

        if (string.IsNullOrWhiteSpace(newPassword))
        {
            return new SettingsAuthorizationWorkflowResult(true, ClearPasswordInputs: true);
        }

        var validation = passwordValidationService.ValidateNewWithConfirmation(
            newPassword,
            confirmPassword,
            UI.ui_Password_Required,
            UI.ui_Password_MinLength_Format,
            UI.ui_Password_ConfirmRequired,
            UI.ui_Password_Mismatch);

        if (!validation.IsValid)
        {
            return new SettingsAuthorizationWorkflowResult(
                false,
                validation.PasswordError ?? validation.ConfirmPasswordError ?? UI.ui_Password_ValidationFailed,
                validation.PasswordError,
                validation.ConfirmPasswordError);
        }

        var result = await authorizationService.ChangePasswordAsync(string.Empty, newPassword);
        if (result != AuthorizationResult.Success)
        {
            return new SettingsAuthorizationWorkflowResult(false, UI.ui_Password_ValidationFailed);
        }

        return new SettingsAuthorizationWorkflowResult(true, ClearPasswordInputs: true);
    }

    public async Task<SettingsAuthorizationWorkflowResult> ApplyAuthorizationGateSelectionAsync(
        bool isHelloSelected,
        bool isHelloAvailable)
    {
        var selectedGate = isHelloSelected ? AuthorizationGateKind.Hello : AuthorizationGateKind.Password;
        if (_appSettings.Authorization.Gate == selectedGate)
        {
            return new SettingsAuthorizationWorkflowResult(true);
        }

        if (selectedGate == AuthorizationGateKind.Hello)
        {
            if (!isHelloAvailable)
            {
                return new SettingsAuthorizationWorkflowResult(false, "Windows Hello is not supported on this device.");
            }

            if (!_appSettings.Authorization.HasHelloSetup)
            {
                var configureResult = await authorizationService.ConfigureHelloAsync();
                if (configureResult != AuthorizationResult.Success)
                {
                    return new SettingsAuthorizationWorkflowResult(false, "Windows Hello setup failed.");
                }
            }
        }

        var previousGate = _appSettings.Authorization.Gate;
        _appSettings.Authorization.Gate = selectedGate;

        var saveResult = await settingsService.SaveAsync();
        if (saveResult.IsFailed)
        {
            _appSettings.Authorization.Gate = previousGate;
            return new SettingsAuthorizationWorkflowResult(false, string.Join("; ", saveResult.Errors.Select(e => e.Message)));
        }

        return new SettingsAuthorizationWorkflowResult(true);
    }

    public async Task<SettingsAuthorizationWorkflowResult> ChangePasswordAsync(string newPassword, string confirmPassword)
    {
        var validation = passwordValidationService.ValidateNewWithConfirmation(
            newPassword,
            confirmPassword,
            UI.ui_Password_Required,
            UI.ui_Password_MinLength_Format,
            UI.ui_Password_ConfirmRequired,
            UI.ui_Password_Mismatch);

        if (!validation.IsValid)
        {
            return new SettingsAuthorizationWorkflowResult(
                false,
                validation.PasswordError ?? validation.ConfirmPasswordError ?? UI.ui_Password_ValidationFailed,
                validation.PasswordError,
                validation.ConfirmPasswordError);
        }

        var result = await authorizationService.ChangePasswordAsync(string.Empty, newPassword);
        if (result != AuthorizationResult.Success)
        {
            return new SettingsAuthorizationWorkflowResult(false, UI.ui_Password_ValidationFailed);
        }

        return new SettingsAuthorizationWorkflowResult(true, ClearPasswordInputs: true);
    }
}
