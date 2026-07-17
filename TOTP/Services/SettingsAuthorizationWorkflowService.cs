using System.Linq;
using System.Threading.Tasks;
using TOTP.Core.Security.Interfaces;
using TOTP.Core.Security.Models;
using TOTP.Core.Services.Interfaces;
using TOTP.Resources;
using TOTP.Services.Interfaces;

namespace TOTP.Services;

public sealed class SettingsAuthorizationWorkflowService(
    IAuthorizationService authorizationService,
    IPasswordValidationService passwordValidationService,
    IPasswordPromptService? passwordPromptService = null) : ISettingsAuthorizationWorkflowService
{
    public async Task<SettingsAuthorizationWorkflowResult> ApplyAuthorizationSettingsAsync(
        bool isHelloSelected,
        bool isHelloAvailable,
        string newPassword,
        string confirmPassword)
    {
        var currentGate = authorizationService.State.ConfiguredGate;

        if (isHelloSelected && currentGate != AuthorizationGateKind.Hello)
        {
            if (!isHelloAvailable)
            {
                return new SettingsAuthorizationWorkflowResult(false, UI.ui_Settings_Auth_HelloUnsupported);
            }

            var helloResult = await EnsureHelloSelectedAsync();
            if (helloResult != null)
            {
                return helloResult;
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

        var currentPassword = PromptForCurrentPassword();
        if (currentPassword is null)
            return new SettingsAuthorizationWorkflowResult(false, UI.ui_Password_VerificationFailed);

        var result = await authorizationService.ChangePasswordAsync(currentPassword, newPassword);
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
        if (authorizationService.State.ConfiguredGate == selectedGate)
        {
            return new SettingsAuthorizationWorkflowResult(true);
        }

        if (selectedGate == AuthorizationGateKind.Hello)
        {
            if (!isHelloAvailable)
            {
                return new SettingsAuthorizationWorkflowResult(false, UI.ui_Settings_Auth_HelloUnsupported);
            }

            var helloResult = await EnsureHelloSelectedAsync();
            if (helloResult != null)
            {
                return helloResult;
            }

            return new SettingsAuthorizationWorkflowResult(true);
        }
        else
        {
            if (!authorizationService.State.IsConfigured)
            {
                return new SettingsAuthorizationWorkflowResult(
                    false,
                    UI.ui_Settings_Auth_PasswordSetupRequired,
                    RevertGateSelection: false);
            }

            var verificationResult = await VerifyExistingMasterPasswordAsync();
            if (verificationResult != null)
            {
                return verificationResult;
            }
        }

        var gateResult = await authorizationService.SetGateAsync(selectedGate);
        return gateResult == AuthorizationResult.Success
            ? new SettingsAuthorizationWorkflowResult(true)
            : new SettingsAuthorizationWorkflowResult(false, UI.ui_Password_ValidationFailed);
    }

    public Task<SettingsAuthorizationWorkflowResult> ChangePasswordAsync(string newPassword, string confirmPassword)
        => ChangePasswordAsync(newPassword, confirmPassword, activatePasswordGate: false);

    public async Task<SettingsAuthorizationWorkflowResult> ChangePasswordAsync(
        string newPassword,
        string confirmPassword,
        bool activatePasswordGate)
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

        var currentPassword = PromptForCurrentPassword();
        if (currentPassword is null)
            return new SettingsAuthorizationWorkflowResult(false, UI.ui_Password_VerificationFailed);

        var result = await authorizationService.ChangePasswordAsync(currentPassword, newPassword);
        if (result != AuthorizationResult.Success)
        {
            return new SettingsAuthorizationWorkflowResult(false, UI.ui_Password_ValidationFailed);
        }

        if (activatePasswordGate)
        {
            var gateResult = await authorizationService.SetGateAsync(AuthorizationGateKind.Password);
            if (gateResult != AuthorizationResult.Success)
                return new SettingsAuthorizationWorkflowResult(false, UI.ui_Password_ValidationFailed);
        }

        return new SettingsAuthorizationWorkflowResult(true, ClearPasswordInputs: true);
    }

    private async Task<SettingsAuthorizationWorkflowResult?> VerifyExistingMasterPasswordAsync()
    {
        if (passwordPromptService == null)
        {
            return new SettingsAuthorizationWorkflowResult(false, UI.ui_Password_VerificationFailed);
        }

        var password = passwordPromptService.Prompt(
            UI.ui_Settings_Auth_EnablePasswordUnlockTitle,
            UI.ui_Settings_Auth_EnablePasswordUnlockMessage,
            requiredErrorMessage: UI.ui_Password_Required,
            validatePasswordAsync: async candidate =>
            {
                var result = await authorizationService.TryUnlockWithPasswordAsync(candidate);
                return result == AuthorizationResult.Success ? null : UI.ui_Password_VerificationFailed;
            });

        if (string.IsNullOrWhiteSpace(password))
        {
            return new SettingsAuthorizationWorkflowResult(false, UI.ui_Password_VerificationFailed);
        }

        return null;
    }

    private async Task<SettingsAuthorizationWorkflowResult?> EnsureHelloSelectedAsync()
    {
        var gateResult = await authorizationService.SetGateAsync(AuthorizationGateKind.Hello);
        if (gateResult == AuthorizationResult.Success) return null;
        if (gateResult != AuthorizationResult.PasswordRequired)
            return new SettingsAuthorizationWorkflowResult(false, UI.ui_Settings_Auth_HelloSetupFailed);

        var recoveryPassword = PromptForCurrentPassword();
        if (recoveryPassword is null)
            return new SettingsAuthorizationWorkflowResult(false, UI.ui_Password_VerificationFailed);

        var configured = await authorizationService.ConfigureHelloAsync(recoveryPassword);
        return configured == AuthorizationResult.Success
            ? null
            : new SettingsAuthorizationWorkflowResult(false, UI.ui_Settings_Auth_HelloSetupFailed);
    }

    private string? PromptForCurrentPassword() =>
        passwordPromptService?.Prompt(
            UI.ui_AuthorizeChange_CurrentPasswordTitle,
            UI.ui_AuthorizeChange_CurrentPasswordMessage,
            requiredErrorMessage: UI.ui_Password_Required);
}
