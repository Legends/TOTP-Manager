using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using TOTP.Commands;
using TOTP.Security;

namespace TOTP.ViewModels;

public sealed class PasswordUnlockViewModel : INotifyPropertyChanged
{
    #region Props and Vars

    private readonly IAuthorizationService _auth;

    public event PropertyChangedEventHandler? PropertyChanged;

    private bool _isSetup;
    public bool IsSetup
    {
        get => _isSetup;
        private set { _isSetup = value; OnPropertyChanged(); }
    }

    private string? _password;
    public string? Password
    {
        get => _password;
        set { _password = value; OnPropertyChanged(); }
    }

    private string? _confirmPassword;
    public string? ConfirmPassword
    {
        get => _confirmPassword;
        set { _confirmPassword = value; OnPropertyChanged(); }
    }

    private string? _message;
    public string? Message
    {
        get => _message;
        set { _message = value; OnPropertyChanged(); }
    }

    public ICommand UnlockCommand { get; }

    #endregion

    public PasswordUnlockViewModel(IAuthorizationService auth)
    {
        _auth = auth;
        UnlockCommand = new AsyncCommand(UnlockAsync);
        IsSetup = false; // default: unlock mode
    }

    public void EnterSetupMode()
    {
        IsSetup = true;
        Password = null;
        ConfirmPassword = null;
        Message = null;
    }

    private async Task UnlockAsync()
    {
        Message = null;

        if (IsSetup)
        {
            var cfg = await _auth.ConfigurePasswordAsync(Password ?? "", ConfirmPassword ?? "");
            if (cfg != AuthorizationResult.Success)
            { 
                Message = "Password setup failed (min length 8, and both fields must match).";
                return;
            }

            // after configuring: unlock immediately by verifying once
            var unlock = await _auth.TryUnlockWithPasswordAsync(Password ?? "");
            if (unlock != AuthorizationResult.Success)
                Message = "Password verification failed.";
            return;
        }

        var result = await _auth.TryUnlockWithPasswordAsync(Password ?? "");
        if (result == AuthorizationResult.InvalidCredentials)
            Message = "Wrong password.";
        else if (result != AuthorizationResult.Success)
            Message = "Unlock failed.";

        //Password = string.Empty; // reset pwd field !
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
