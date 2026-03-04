namespace TOTP.Services.Interfaces;

public interface IPasswordPromptService
{
    string? Prompt(string title, string message);
}
