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
    private readonly IPasswordValidationService _passwordValidationService;

    public PasswordPromptService(
        IAuthorizationService authorizationService,
        IPasswordValidationService passwordValidationService)
    {
        _authorizationService = authorizationService;
        _passwordValidationService = passwordValidationService;
    }

    public string? PromptForEncryptedExportPassword(string title)
    {
        var viewModel = new ExportPasswordPromptViewModel(title, _passwordValidationService)
        {
            ValidateMasterPasswordAsync = async password =>
            {
                var result = await _authorizationService.TryUnlockWithPasswordAsync(password);
                return result == AuthorizationResult.Success;
            }
        };

        var dialog = new ExportPasswordPromptWindow
        {
            DataContext = viewModel,
            Owner = Application.Current?.MainWindow
        };

        var result = dialog.ShowDialog();
        if (result != true || string.IsNullOrWhiteSpace(viewModel.SelectedPassword))
        {
            return null;
        }

        return viewModel.SelectedPassword;
    }

    public string? Prompt(
        string title,
        string message,
        string? errorMessage = null,
        string? requiredErrorMessage = null,
        Func<string, Task<string?>>? validatePasswordAsync = null)
    {
        var viewModel = new PasswordPromptViewModel(title, message, errorMessage, requiredErrorMessage);
        viewModel.ValidatePasswordAsync = validatePasswordAsync;

        var dialog = new PasswordPromptWindow
        {
            DataContext = viewModel,
            Owner = Application.Current?.MainWindow
        };

        var result = dialog.ShowDialog();
        if (result != true || string.IsNullOrWhiteSpace(viewModel.Password))
        {
            return null;
        }

        return viewModel.Password;
    }
}
