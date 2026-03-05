using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using TOTP.Commands;
using TOTP.Core.Security.Interfaces;
using TOTP.Core.Security.Models;
using TOTP.Resources;

namespace TOTP.ViewModels;

public sealed class PasswordUnlockViewModel : INotifyPropertyChanged
{
    #region ### Props and Vars ###

    // === DP: IsSecretVisible ===


    private readonly IAuthorizationService _auth;
    private readonly IPasswordValidationService _passwordValidationService;

    public event PropertyChangedEventHandler? PropertyChanged;

    private bool _isSetup;
    public bool IsSetup
    {
        get => _isSetup;
        private set { _isSetup = value; OnPropertyChanged(); }
    }

    private string _confirmPassword = string.Empty;
    public string ConfirmPassword
    {
        get => _confirmPassword;
        set { _confirmPassword = value ?? string.Empty; OnPropertyChanged(); SavePasswordCommand.RaiseCanExecuteChanged(); }
    }

    private string _password = string.Empty;
    public string Password
    {
        get => _password;
        set
        {
            Message = string.Empty;
            _password = value ?? string.Empty;
            OnPropertyChanged();
            SavePasswordCommand.RaiseCanExecuteChanged();
            UnlockCommand.RaiseCanExecuteChanged();
        }
    }


    private string? _message;
    public string? Message
    {
        get => _message;
        set
        {
            if (Message == value)
                return;

            _message = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasMessage));
        }
    }

    public bool HasMessage => !string.IsNullOrWhiteSpace(Message);

    public AsyncCommand UnlockCommand { get; }

    public AsyncCommand SavePasswordCommand { get; }

    #endregion

    #region ### CONSTRUCTOR ###

    public PasswordUnlockViewModel(
        IAuthorizationService auth,
        IPasswordValidationService passwordValidationService,
        ILogger<PasswordUnlockViewModel> logger)
    {
        _auth = auth;
        _passwordValidationService = passwordValidationService;
        UnlockCommand = new AsyncCommand(
            execute: async () => await UnlockAsync(),
            canExecute: () => ValidatePassword(Password),
            logger: logger
        );

        IsSetup = false; // default: password unlock mode
        SavePasswordCommand = new AsyncCommand(SavePassword, CanSavePassword, logger);
        
        _auth.State.Changed += State_Changed;
    }
    #endregion

    private bool ValidatePassword(object s)
    {
        var res = !string.IsNullOrEmpty(Password);
        return res;
    }

    #region ### METHODS ###
    private void State_Changed(object? sender, EventArgs e)
    {
        if (_auth.State.IsUnlocked)
        {
            Password = string.Empty;
        }

        if (_auth.State.ConfiguredGate == AuthorizationGateKind.Password && _auth.State.IsConfigured)
        {
            IsSetup = false;
        }
    }

    private bool CanSavePassword()
    {
        if (IsSetup)
        {
            return _passwordValidationService.ValidateNewWithConfirmation(
                Password,
                ConfirmPassword,
                UI.ui_Password_Required,
                UI.ui_Password_MinLength_Format,
                UI.ui_Password_ConfirmRequired,
                UI.ui_Password_Mismatch).IsValid;
        }

        return _passwordValidationService.ValidateRequired(Password, UI.ui_Password_Required).IsValid;
    }

    private async Task SavePassword()
    {
        Message = string.Empty;

        // 1. Defensive validation
        if (!CanSavePassword())
        {
            Message = IsSetup
                ? BuildSetupValidationMessage()
                : UI.ui_Password_Required;
            return;
        }

        // 2. Execute Unlock/Setup logic
        var result = await UnlockAsync();

        // 3. Check if it actually succeeded
        if (result != AuthorizationResult.Success)
        {
            // Message is already set inside UnlockAsync, so we just exit
            return;
        }

        // 4. Success: Clear secrets and exit setup mode
        IsSetup = false;
        Password = string.Empty;
        ConfirmPassword = string.Empty;

    }


    public void EnterSetupMode()
    {
        IsSetup = true;
        Password = string.Empty;
        ConfirmPassword = string.Empty;
        Message = null;
    }

    /// <summary>
    /// This method does the following based on IsSetup true of false:
    /// TRUE: tries to configure a new password (with validation) and if successful, immediately tries to unlock with it
    /// FALSE: tries to unlock with the provided password
    /// </summary>
    /// <returns></returns>

    private async Task<AuthorizationResult> UnlockAsync()
    {
        Message = null;

        if (string.IsNullOrWhiteSpace(Password))
        {
            Message = UI.ui_Password_Required;
            return AuthorizationResult.InvalidCredentials; // Or a specific "Empty" result
        }

        if (IsSetup)
        {
            var setupValidation = _passwordValidationService.ValidateNewWithConfirmation(
                Password,
                ConfirmPassword,
                UI.ui_Password_Required,
                UI.ui_Password_MinLength_Format,
                UI.ui_Password_ConfirmRequired,
                UI.ui_Password_Mismatch);

            if (!setupValidation.IsValid)
            {
                Message = setupValidation.PasswordError ?? setupValidation.ConfirmPasswordError ?? UI.ui_Password_SetupFailed;
                return AuthorizationResult.InvalidCredentials;
            }

            var cfg = await _auth.ConfigurePasswordAsync(Password ?? "", ConfirmPassword ?? "");
            if (cfg != AuthorizationResult.Success)
            {
                Message = UI.ui_Password_SetupFailed;
            }

            return cfg;
        }

        // Standard unlock path for non-setup scenarios
        var unlock = await _auth.TryUnlockWithPasswordAsync(Password);
        if (unlock != AuthorizationResult.Success)
        {
            Message = UI.ui_Password_VerificationFailed;
        }

        return unlock;
    }

    private string BuildSetupValidationMessage()
    {
        var validation = _passwordValidationService.ValidateNewWithConfirmation(
            Password,
            ConfirmPassword,
            UI.ui_Password_Required,
            UI.ui_Password_MinLength_Format,
            UI.ui_Password_ConfirmRequired,
            UI.ui_Password_Mismatch);

        return validation.PasswordError ?? validation.ConfirmPasswordError ?? UI.ui_Password_SetupFailed;
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    #endregion
}
