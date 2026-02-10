using System.Threading.Tasks;
using TOTP.Security.Models;

namespace TOTP.Security.Interfaces;

public interface IAuthorizationService
{
    AuthorizationState State { get; }

    Task InitializeAsync();

    // First-run configuration
    Task<AuthorizationResult> ConfigureHelloAsync();
    Task<AuthorizationResult> ConfigurePasswordAsync(string password, string confirmPassword);

    // Unlock attempts (after configured)
    Task<AuthorizationResult> TryUnlockWithHelloAsync();
    Task<AuthorizationResult> TryUnlockWithPasswordAsync(string password);

    // Startup gate (always called on app start)
    Task<AuthorizationResult> TryUnlockOnStartupAsync();

    void Lock();
}