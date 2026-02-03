using System.Threading.Tasks;

namespace TOTP.Security;

public interface IAuthorizationService
{
    AuthorizationState State { get; }

    Task<AuthorizationResult> TryUnlockWithHelloAsync();
    AuthorizationResult UnlockWithPassword(string password);

    void Lock();
}
