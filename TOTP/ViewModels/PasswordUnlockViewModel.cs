using System.Windows.Input;
using TOTP.Commands;
using TOTP.Security;

namespace TOTP.ViewModels;

public sealed class PasswordUnlockViewModel
{
    private readonly IAuthorizationService _auth;

    public string Password { get; set; } = string.Empty;

    public ICommand UnlockCommand { get; }

    public AuthorizationResult? UnlockResult { get; private set; }

    public PasswordUnlockViewModel(IAuthorizationService auth)
    {
        _auth = auth;
        UnlockCommand = new RelayCommand(Unlock);
    }
 
    private void Unlock()
    {
        UnlockResult = _auth.UnlockWithPassword(Password);

        // TODO: raise PropertyChanged if you use INotifyPropertyChanged
        // OnPropertyChanged(nameof(UnlockResult));
    }
}
