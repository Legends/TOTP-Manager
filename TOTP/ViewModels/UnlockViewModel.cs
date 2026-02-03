using TOTP.Security;

namespace TOTP.ViewModels;

public sealed class UnlockViewModel
{
    public HelloUnlockViewModel Hello { get; }
    public PasswordUnlockViewModel Password { get; }

    public UnlockViewModel(IAuthorizationService auth)
    {
        Hello = new HelloUnlockViewModel(auth);
        Password = new PasswordUnlockViewModel(auth);
    }
}
