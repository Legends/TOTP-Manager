using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using TOTP.Commands;
using TOTP.Resources;

namespace TOTP.ViewModels;

public class PasswordPromptViewModel : INotifyPropertyChanged
{
    #region ### PROPERTIES & FIELDS ###

    private string _password = string.Empty;
    private string _title = string.Empty;
    private string _message = string.Empty;
    private string _errorMessage = string.Empty;
    private string _requiredErrorMessage = string.Empty;

    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler? RequestClose;

    public Func<string, Task<string?>>? ValidatePasswordAsync { get; set; }
    public ICommand ConfirmCommand { get; }

    public string Title
    {
        get => _title;
        set
        {
            if (_title != value)
            {
                _title = value;
                OnPropertyChanged();
            }
        }
    }

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
                if (!string.IsNullOrWhiteSpace(_errorMessage))
                {
                    ErrorMessage = string.Empty;
                }

                if (ConfirmCommand is AsyncCommand confirmCommand)
                {
                    confirmCommand.RaiseCanExecuteChanged();
                }

                OnPropertyChanged();
            }
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            if (_errorMessage != value)
            {
                _errorMessage = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasErrorMessage));
            }
        }
    }

    public bool HasErrorMessage => !string.IsNullOrWhiteSpace(_errorMessage);

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
    #endregion

    public PasswordPromptViewModel(string title, string message, string? errorMessage = null, string? requiredErrorMessage = null)
    {
        _title = title;
        _message = message;
        _errorMessage = errorMessage ?? string.Empty;
        _requiredErrorMessage = requiredErrorMessage ?? string.Empty;
        ConfirmCommand = new AsyncCommand(ConfirmAsync, CanConfirm);
    }

    private bool CanConfirm() => !string.IsNullOrWhiteSpace(Title);

    private async Task ConfirmAsync()
    {
        if (string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = string.IsNullOrWhiteSpace(RequiredErrorMessage)
                ? UI.ui_ImportPasswordRequired
                : RequiredErrorMessage;
            return;
        }

        if (ValidatePasswordAsync != null)
        {
            var validationError = await ValidatePasswordAsync(Password);
            if (!string.IsNullOrWhiteSpace(validationError))
            {
                ErrorMessage = validationError;
                return;
            }
        }

        RequestClose?.Invoke(this, EventArgs.Empty);
    }

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null!)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

