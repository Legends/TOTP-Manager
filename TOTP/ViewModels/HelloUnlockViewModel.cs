using System.Threading.Tasks;
using System.Windows.Input;
using TOTP.Commands;
using TOTP.Security;

namespace TOTP.ViewModels;

public sealed class HelloUnlockViewModel
{
    private readonly IAuthorizationService _auth;

    public ICommand UnlockCommand { get; }

    public HelloUnlockViewModel(IAuthorizationService auth)
    {
        _auth = auth;
        UnlockCommand = new AsyncCommand(UnlockAsync);
    }

    public Task<AuthorizationResult> UnlockAsync()
        => _auth.TryUnlockWithHelloAsync();
}
