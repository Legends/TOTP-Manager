namespace TOTP.Security;

public interface IHelloService
{
    Task<bool> IsAvailableAsync();
    Task<AuthorizationResult> RequestVerificationAsync();
}
