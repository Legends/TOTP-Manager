using TOTP.Core.Security.Models;

namespace TOTP.Core.Security.Interfaces;

public interface IAuthorizationService
{
    AuthorizationState State { get; }

    Task InitializeAsync();
    Task<bool> IsHelloAvailableAsync();
    Task<AuthorizationResult> TryUnlockOnStartupAsync();
    Task<AuthorizationResult> TryUnlockWithPasswordAsync(string password);
    Task<AuthorizationResult> TryUnlockWithHelloAsync();

    Task<AuthorizationResult> ConfigurePasswordAsync(string password, string confirmPassword);
    Task<AuthorizationResult> ConfigureHelloAsync();
    Task<AuthorizationResult> SetGateAsync(AuthorizationGateKind gate);
    Task<AuthorizationResult> ChangePasswordAsync(string currentPassword, string newPassword);

    void Logout();
    void Lock();
}