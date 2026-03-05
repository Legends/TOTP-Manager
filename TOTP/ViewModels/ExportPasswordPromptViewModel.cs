using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using TOTP.Commands;
using TOTP.Core.Security.Interfaces;
using TOTP.Resources;

namespace TOTP.ViewModels;

public sealed class ExportPasswordPromptViewModel : INotifyPropertyChanged
{
    private readonly IPasswordValidationService _passwordValidationService;
    private string _title;
    private bool _useMasterPassword = true;
    private string _masterPassword = string.Empty;
    private string _customPassword = string.Empty;
    private string _confirmCustomPassword = string.Empty;
    private string _errorMessage = string.Empty;

    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler? RequestClose;

    public Func<string, Task<bool>>? ValidateMasterPasswordAsync { get; set; }
    public ICommand ConfirmCommand { get; }

    public ExportPasswordPromptViewModel(string title, IPasswordValidationService passwordValidationService)
    {
        _title = title;
        _passwordValidationService = passwordValidationService;
        ConfirmCommand = new AsyncCommand(ConfirmAsync);
    }

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
            OnPropertyChanged();
        }
    }

    public bool UseMasterPassword
    {
        get => _useMasterPassword;
        set
        {
            if (_useMasterPassword == value)
            {
                return;
            }

            _useMasterPassword = value;
            ErrorMessage = string.Empty;
            OnPropertyChanged();
            OnPropertyChanged(nameof(UseCustomPassword));
        }
    }

    public bool UseCustomPassword
    {
        get => !UseMasterPassword;
        set => UseMasterPassword = !value;
    }

    public string MasterPassword
    {
        get => _masterPassword;
        set
        {
            if (_masterPassword == value)
            {
                return;
            }

            _masterPassword = value;
            if (!string.IsNullOrWhiteSpace(_errorMessage))
            {
                ErrorMessage = string.Empty;
            }
            OnPropertyChanged();
        }
    }

    public string CustomPassword
    {
        get => _customPassword;
        set
        {
            if (_customPassword == value)
            {
                return;
            }

            _customPassword = value;
            if (!string.IsNullOrWhiteSpace(_errorMessage))
            {
                ErrorMessage = string.Empty;
            }
            OnPropertyChanged();
        }
    }

    public string ConfirmCustomPassword
    {
        get => _confirmCustomPassword;
        set
        {
            if (_confirmCustomPassword == value)
            {
                return;
            }

            _confirmCustomPassword = value;
            if (!string.IsNullOrWhiteSpace(_errorMessage))
            {
                ErrorMessage = string.Empty;
            }
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
        }
    }

    public string SelectedPassword { get; private set; } = string.Empty;

    private async Task ConfirmAsync()
    {
        if (UseMasterPassword)
        {
            var requiredValidation = _passwordValidationService.ValidateRequired(MasterPassword, UI.ui_ExportPasswordRequired);
            if (!requiredValidation.IsValid)
            {
                ErrorMessage = requiredValidation.PasswordError ?? UI.ui_ExportPasswordRequired;
                return;
            }

            if (ValidateMasterPasswordAsync != null)
            {
                var isValid = await ValidateMasterPasswordAsync(MasterPassword);
                if (!isValid)
                {
                    ErrorMessage = UI.ui_ExportPwd_WrongMasterPassword;
                    return;
                }
            }

            SelectedPassword = MasterPassword;
            RequestClose?.Invoke(this, EventArgs.Empty);
            return;
        }

        var validation = _passwordValidationService.ValidateNewWithConfirmation(
            CustomPassword,
            ConfirmCustomPassword,
            UI.ui_Password_Required,
            UI.ui_Password_MinLength_Format,
            UI.ui_ExportPasswordRequired,
            UI.ui_ExportPwd_CustomPasswordMismatch);

        if (!validation.IsValid)
        {
            ErrorMessage = validation.PasswordError ?? validation.ConfirmPasswordError ?? UI.ui_ExportPasswordRequired;
            return;
        }

        SelectedPassword = CustomPassword;
        RequestClose?.Invoke(this, EventArgs.Empty);
    }

    private void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
