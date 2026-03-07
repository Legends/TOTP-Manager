using System;
using System.Threading.Tasks;
using TOTP.Core.Security.Interfaces;
using TOTP.Resources;

namespace TOTP.ViewModels;

public sealed class PasswordPromptViewModel : PasswordPromptViewModelBase
{
    private string _password = string.Empty;
    private string _message = string.Empty;
    private string _requiredErrorMessage = string.Empty;

    public Func<string, Task<string?>>? ValidatePasswordAsync { get; set; }
    public event EventHandler<string>? PasswordConfirmed;

    public string Message
    {
        get => _message;
        set
        {
            if (_message != value)
            {
                _message = value;
                OnPropertyChanged();
            }
        }
    }

    public string Password
    {
        get => _password;
        set
        {
            if (_password != value)
            {
                _password = value;
                ClearErrorMessage();
                RaiseConfirmCanExecuteChanged();
                OnPropertyChanged();
            }
        }
    }

    public string RequiredErrorMessage
    {
        get => _requiredErrorMessage;
        set
        {
            if (_requiredErrorMessage != value)
            {
                _requiredErrorMessage = value;
                OnPropertyChanged();
            }
        }
    }

    public PasswordPromptViewModel(
        string title,
        string message,
        IPasswordValidationService passwordValidationService,
        string? errorMessage = null,
        string? requiredErrorMessage = null)
        : base(title, passwordValidationService)
    {
        _message = message;
        SetError(errorMessage ?? string.Empty);
        _requiredErrorMessage = requiredErrorMessage ?? string.Empty;
    }

    protected override async Task<bool> ConfirmCoreAsync()
    {
        var candidatePassword = Password;
        Password = string.Empty;

        if (!ValidateRequired(candidatePassword, GetRequiredErrorMessage(), out var requiredError))
        {
            SetError(requiredError);
            return false;
        }

        if (ValidatePasswordAsync != null)
        {
            var validationError = await ValidatePasswordAsync(candidatePassword);
            if (!string.IsNullOrWhiteSpace(validationError))
            {
                SetError(validationError);
                return false;
            }
        }

        PasswordConfirmed?.Invoke(this, candidatePassword);
        return true;
    }

    public override void ClearSensitiveData()
    {
        base.ClearSensitiveData();
        _password = string.Empty;
        ValidatePasswordAsync = null;
    }

    private string GetRequiredErrorMessage()
    {
        return string.IsNullOrWhiteSpace(RequiredErrorMessage)
            ? UI.ui_ImportPasswordRequired
            : RequiredErrorMessage;
    }
}

