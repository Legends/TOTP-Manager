using System;
using System.Threading.Tasks;
using System.Windows;
using TOTP.Core.Security.Interfaces;
using TOTP.Core.Security.Models;
using TOTP.Services.Interfaces;
using TOTP.ViewModels;
using TOTP.Views;

namespace TOTP.Services;

public sealed class PasswordPromptService : IPasswordPromptService
{
    private readonly IAuthorizationService _authorizationService;

    public PasswordPromptService(IAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
    }

    public string? PromptForEncryptedExportPassword(string title)
    {
        var viewModel = new ExportPasswordPromptViewModel(title);

        var dialog = new ExportPasswordPromptWindow
        {
            DataContext = viewModel,
            Owner = Application.Current?.MainWindow,
            ValidateMasterPasswordAsync = async password =>
            {
                var result = await _authorizationService.TryUnlockWithPasswordAsync(password);
                return result == AuthorizationResult.Success;
            }
        };

        var result = dialog.ShowDialog();
        if (result != true || string.IsNullOrWhiteSpace(dialog.SelectedPassword))
        {
            return null;
        }

        return dialog.SelectedPassword;
    }

    public string? Prompt(
        string title,
        string message,
        string? errorMessage = null,
        string? requiredErrorMessage = null,
        Func<string, Task<string?>>? validatePasswordAsync = null)
    {
        var viewModel = new PasswordPromptViewModel(title, message, errorMessage, requiredErrorMessage);

        var dialog = new PasswordPromptWindow
        {
            DataContext = viewModel,
            Owner = Application.Current?.MainWindow,
            ValidatePasswordAsync = validatePasswordAsync
        };

        var result = dialog.ShowDialog();
        if (result != true || string.IsNullOrWhiteSpace(viewModel.Password))
        {
            return null;
        }

        return viewModel.Password;
    }
}
