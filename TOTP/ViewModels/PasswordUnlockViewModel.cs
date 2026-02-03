using System.Windows.Input;
using TOTP.Commands;
using TOTP.Security;

namespace TOTP.ViewModels;

public sealed class PasswordUnlockViewModel
{
    private readonly IAuthorizationService _auth;

    public string Password { get; set; } = string.Empty;

    public ICommand UnlockCommand { get; }

    public PasswordUnlockViewModel(IAuthorizationService auth)
    {
        _auth = auth;
        UnlockCommand = new RelayCommand(Unlock);
    }

    public AuthorizationResult Unlock()
        => _auth.UnlockWithPassword(Password);
}
