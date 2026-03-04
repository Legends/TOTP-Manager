using System;
using System.Threading.Tasks;

namespace TOTP.Services.Interfaces;

public interface IPasswordPromptService
{
    string? Prompt(
        string title,
        string message,
        string? errorMessage = null,
        string? requiredErrorMessage = null,
        Func<string, Task<string?>>? validatePasswordAsync = null);

    string? PromptForEncryptedExportPassword(string title);
}
