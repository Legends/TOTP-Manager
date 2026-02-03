namespace TOTP.Security;

public sealed class AuthorizationService : IAuthorizationService
{
    private readonly IHelloService _hello;
    private readonly IPasswordService _password;

    public AuthorizationState State { get; } = new();

    public AuthorizationService(
        IHelloService hello,
        IPasswordService password)
    {
        _hello = hello;
        _password = password;
    }

    public async Task<AuthorizationResult> TryUnlockWithHelloAsync()
    {
        if (!await _hello.IsAvailableAsync())
        {
            return AuthorizationResult.NotAvailable;
        }

        var result = await _hello.RequestVerificationAsync();

        if (result == AuthorizationResult.Success)
        {
            State.Unlock();
        }

        return result;
    }

    public AuthorizationResult UnlockWithPassword(string password)
    {
        if (!_password.IsConfigured)
        {
            return AuthorizationResult.NotAvailable;
        }

        if (!_password.Verify(password))
        {
            return AuthorizationResult.Failed;
        }

        State.Unlock();
        return AuthorizationResult.Success;
    }

    public void Lock()
    {
        State.Lock();
    }
}
