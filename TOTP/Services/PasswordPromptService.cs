using System;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.Logging;
using TOTP.Core.Security.Interfaces;
using TOTP.Core.Security.Models;
using TOTP.Services.Interfaces;
using TOTP.ViewModels;

namespace TOTP.Services;

public sealed class PasswordPromptService : IPasswordPromptService
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IPasswordValidationService _passwordValidationService;
    private readonly IPasswordPromptDialogFactory _dialogFactory;
    private readonly ILogger<PasswordPromptService> _logger;

    public PasswordPromptService(
        IAuthorizationService authorizationService,
        IPasswordValidationService passwordValidationService,
        IPasswordPromptDialogFactory dialogFactory,
        ILogger<PasswordPromptService> logger)
    {
        _authorizationService = authorizationService;
        _passwordValidationService = passwordValidationService;
        _dialogFactory = dialogFactory;
        _logger = logger;
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

        var dialog = _dialogFactory.CreateExportPasswordPromptDialog();
        dialog.DataContext = viewModel;
        dialog.Owner = GetMainWindowSafe();

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

        var dialog = _dialogFactory.CreatePasswordPromptDialog();
        dialog.DataContext = viewModel;
        dialog.Owner = GetMainWindowSafe();

        var result = dialog.ShowDialog();
        if (result != true || string.IsNullOrWhiteSpace(viewModel.Password))
        {
            return null;
        }

        return viewModel.Password;
    }

    private Window? GetMainWindowSafe()
    {
        try
        {
            return Application.Current?.MainWindow;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to access Application.Current.MainWindow in password prompt service.");
            return null;
        }
    }
}
