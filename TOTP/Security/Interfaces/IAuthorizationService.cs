using System.Threading.Tasks;
using TOTP.Security.Models;

namespace TOTP.Security.Interfaces;

public interface IAuthorizationService
{
    AuthorizationState State { get; }

    Task SetGateAsync(AuthorizationGateKind gate);

    // Lifecycle
    Task InitializeAsync();
    void Logout();
    void Lock(); // Required by MainViewSessionController

    // Unlocking
    Task<AuthorizationResult> TryUnlockOnStartupAsync();
    Task<AuthorizationResult> TryUnlockWithPasswordAsync(string password);
    Task<AuthorizationResult> TryUnlockWithHelloAsync();

    // Configuration
    Task<AuthorizationResult> ConfigurePasswordAsync(string password, string confirmPassword);
    Task<AuthorizationResult> ConfigureHelloAsync();
    Task<AuthorizationResult> ChangePasswordAsync(string newPassword, string confirmPassword);

    // Helpers
    Task<bool> IsHelloAvailableAsync();
}