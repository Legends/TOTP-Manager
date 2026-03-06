using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using TOTP.Commands;
using TOTP.Core.Security.Interfaces;

namespace TOTP.ViewModels;

public abstract class PasswordPromptViewModelBase : INotifyPropertyChanged
{
    private readonly IPasswordValidationService _passwordValidationService;
    private string _title;
    private string _errorMessage = string.Empty;

    protected PasswordPromptViewModelBase(string title, IPasswordValidationService passwordValidationService)
    {
        _title = title;
        _passwordValidationService = passwordValidationService;
        ConfirmCommand = new AsyncCommand(ConfirmAsync, CanConfirm);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler? RequestClose;

    public ICommand ConfirmCommand { get; }

    public string Title
    {
        get => _title;
        set
        {
            if (_title == value)
            {
                return;
            }

            _title = value;
            RaiseConfirmCanExecuteChanged();
            OnPropertyChanged();
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            if (_errorMessage == value)
            {
                return;
            }

            _errorMessage = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasErrorMessage));
        }
    }

    public bool HasErrorMessage => !string.IsNullOrWhiteSpace(ErrorMessage);

    public virtual void ClearSensitiveData()
    {
        _errorMessage = string.Empty;
    }

    protected void ClearErrorMessage()
    {
        if (HasErrorMessage)
        {
            ErrorMessage = string.Empty;
        }
    }

    protected void RaiseConfirmCanExecuteChanged()
    {
        if (ConfirmCommand is AsyncCommand confirmCommand)
        {
            confirmCommand.RaiseCanExecuteChanged();
        }
    }

    protected void CloseDialog()
    {
        RequestClose?.Invoke(this, EventArgs.Empty);
    }

    protected bool ValidateRequired(string? password, string requiredMessage, out string validationError)
    {
        var validation = _passwordValidationService.ValidateRequired(password, requiredMessage);
        if (validation == null)
        {
            validationError = requiredMessage;
            return false;
        }

        validationError = validation.PasswordError ?? requiredMessage;
        return validation.IsValid;
    }

    protected bool ValidateNewWithConfirmation(
        string? password,
        string? confirmPassword,
        string requiredMessage,
        string minLengthMessageFormat,
        string confirmRequiredMessage,
        string mismatchMessage,
        out string validationError)
    {
        var validation = _passwordValidationService.ValidateNewWithConfirmation(
            password,
            confirmPassword,
            requiredMessage,
            minLengthMessageFormat,
            confirmRequiredMessage,
            mismatchMessage);

        if (validation == null)
        {
            validationError = requiredMessage;
            return false;
        }

        validationError = validation.PasswordError ?? validation.ConfirmPasswordError ?? requiredMessage;
        return validation.IsValid;
    }

    protected void SetError(string errorMessage)
    {
        ErrorMessage = errorMessage;
    }

    protected abstract Task<bool> ConfirmCoreAsync();

    private bool CanConfirm() => !string.IsNullOrWhiteSpace(Title);

    private async Task ConfirmAsync()
    {
        if (await ConfirmCoreAsync())
        {
            CloseDialog();
        }
    }

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null!)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
