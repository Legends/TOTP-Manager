using System.Threading.Tasks;
using TOTP.Core.Security.Models;

namespace TOTP.Services.Interfaces;

public interface ISettingsAuthorizationWorkflowService
{
    Task<SettingsAuthorizationWorkflowResult> ApplyAuthorizationSettingsAsync(
        bool isHelloSelected,
        bool isHelloAvailable,
        string newPassword,
        string confirmPassword);

    Task<SettingsAuthorizationWorkflowResult> ApplyAuthorizationGateSelectionAsync(
        bool isHelloSelected,
        bool isHelloAvailable);

    Task<SettingsAuthorizationWorkflowResult> ChangePasswordAsync(string newPassword, string confirmPassword);
}

public sealed record SettingsAuthorizationWorkflowResult(
    bool IsSuccess,
    string? ErrorMessage = null,
    string? NewPasswordError = null,
    string? ConfirmPasswordError = null,
    bool ClearPasswordInputs = false);
