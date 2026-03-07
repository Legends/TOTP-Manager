using System;
using System.Threading.Tasks;
using TOTP.Core.Security.Interfaces;
using TOTP.Resources;

namespace TOTP.ViewModels;

public sealed class ExportPasswordPromptViewModel : PasswordPromptViewModelBase
{
    private bool _useMasterPassword = true;
    private string _masterPassword = string.Empty;
    private string _customPassword = string.Empty;
    private string _confirmCustomPassword = string.Empty;

    public Func<string, Task<bool>>? ValidateMasterPasswordAsync { get; set; }
    public event EventHandler<string>? PasswordConfirmed;

    public ExportPasswordPromptViewModel(string title, IPasswordValidationService passwordValidationService)
        : base(title, passwordValidationService)
    {
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
            ClearErrorMessage();
            RaiseConfirmCanExecuteChanged();
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
            ClearErrorMessage();
            RaiseConfirmCanExecuteChanged();
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
            ClearErrorMessage();
            RaiseConfirmCanExecuteChanged();
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
            ClearErrorMessage();
            RaiseConfirmCanExecuteChanged();
            OnPropertyChanged();
        }
    }

    public string SelectedPassword { get; private set; } = string.Empty;

    protected override async Task<bool> ConfirmCoreAsync()
    {
        if (UseMasterPassword)
        {
            var enteredMasterPassword = MasterPassword;
            MasterPassword = string.Empty;

            if (!ValidateRequired(enteredMasterPassword, UI.ui_ExportPasswordRequired, out var requiredError))
            {
                SetError(requiredError);
                return false;
            }

            if (ValidateMasterPasswordAsync != null)
            {
                var isValid = await ValidateMasterPasswordAsync(enteredMasterPassword);
                if (!isValid)
                {
                    SetError(UI.ui_ExportPwd_WrongMasterPassword);
                    return false;
                }
            }

            SelectedPassword = enteredMasterPassword;
            PasswordConfirmed?.Invoke(this, SelectedPassword);
            return true;
        }

        var enteredCustomPassword = CustomPassword;
        var enteredCustomPasswordConfirmation = ConfirmCustomPassword;
        CustomPassword = string.Empty;
        ConfirmCustomPassword = string.Empty;

        var isValidCustomPassword = ValidateNewWithConfirmation(
            enteredCustomPassword,
            enteredCustomPasswordConfirmation,
            UI.ui_Password_Required,
            UI.ui_Password_MinLength_Format,
            UI.ui_ExportPasswordRequired,
            UI.ui_ExportPwd_CustomPasswordMismatch,
            out var customValidationError);

        if (!isValidCustomPassword)
        {
            SetError(customValidationError);
            return false;
        }

        SelectedPassword = enteredCustomPassword;
        PasswordConfirmed?.Invoke(this, SelectedPassword);
        return true;
    }

    public override void ClearSensitiveData()
    {
        base.ClearSensitiveData();
        _masterPassword = string.Empty;
        _customPassword = string.Empty;
        _confirmCustomPassword = string.Empty;
        SelectedPassword = string.Empty;
        ValidateMasterPasswordAsync = null;
    }
}

