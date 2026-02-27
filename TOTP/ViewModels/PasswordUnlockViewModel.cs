using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using TOTP.Commands;
using TOTP.Security.Interfaces;
using TOTP.Security.Models;

namespace TOTP.ViewModels;

public sealed class PasswordUnlockViewModel : INotifyPropertyChanged
{
    #region Props and Vars

    // === DP: IsSecretVisible ===


    private readonly IAuthorizationService _auth;

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

    public ICommand UnlockCommand { get; }

    public AsyncCommand SavePasswordCommand { get; }

    #endregion

    public PasswordUnlockViewModel(IAuthorizationService auth)
    {
        _auth = auth;
        UnlockCommand = new AsyncCommand(UnlockAsync);
        IsSetup = false; // default: unlock mode
        SavePasswordCommand = new AsyncCommand(SavePassword, CanSavePassword);

        _auth.State.Changed += State_Changed;
    }

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
        if (string.IsNullOrWhiteSpace(Password)) return false;
        if (Password.Length < 3) return false;           // example rule
        if (!string.Equals(Password, ConfirmPassword)) return false;
        return true;
    }

    private async Task SavePassword()
    {
        Message = string.Empty;

        // 1. Defensive validation
        if (!CanSavePassword())
        {
            Message = "Passwords must match and not be empty.";
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
        Message = "Password saved successfully!";
    }


    public void EnterSetupMode()
    {
        IsSetup = true;
        Password = null;
        ConfirmPassword = null;
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
            Message = "Password required!";
            return AuthorizationResult.InvalidCredentials; // Or a specific "Empty" result
        }

        if (IsSetup)
        {
            var cfg = await _auth.ConfigurePasswordAsync(Password ?? "", ConfirmPassword ?? "");
            if (cfg != AuthorizationResult.Success)
            {
                Message = "Password setup failed (min length 8, and both fields must match).";
            }

            return cfg;
        }

        // Standard unlock path for non-setup scenarios
        var unlock = await _auth.TryUnlockWithPasswordAsync(Password);
        if (unlock != AuthorizationResult.Success)
        {
            Message = "Password verification failed.";
        }

        return unlock;
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
