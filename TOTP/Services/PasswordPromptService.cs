using System;
using System.Threading.Tasks;
using TOTP.Core.Security.Interfaces;
using TOTP.Core.Security.Models;
using TOTP.Core.Services.Interfaces;
using TOTP.Services.Interfaces;
using TOTP.ViewModels;

namespace TOTP.Services;

public sealed class PasswordPromptService : IPasswordPromptService
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IPasswordValidationService _passwordValidationService;
    private readonly IPasswordPromptDialogFactory _dialogFactory;

    public PasswordPromptService(
        IAuthorizationService authorizationService,
        IPasswordValidationService passwordValidationService,
        IPasswordPromptDialogFactory dialogFactory)
    {
        _authorizationService = authorizationService;
        _passwordValidationService = passwordValidationService;
        _dialogFactory = dialogFactory;
    }

    public string? PromptForEncryptedExportPassword(string title)
    {
        string? confirmedPassword = null;
        var viewModel = new ExportPasswordPromptViewModel(title, _passwordValidationService)
        {
            ValidateMasterPasswordAsync = async password =>
            {
                var result = await _authorizationService.TryUnlockWithPasswordAsync(password);
                return result == AuthorizationResult.Success;
            }
        };
        viewModel.PasswordConfirmed += OnPasswordConfirmed;

        var dialog = _dialogFactory.CreateExportPasswordPromptDialog();
        dialog.DataContext = viewModel;
        try
        {
            var result = dialog.ShowDialog();
            if (result != true || string.IsNullOrWhiteSpace(confirmedPassword))
            {
                return null;
            }

            return confirmedPassword;
        }
        finally
        {
            viewModel.PasswordConfirmed -= OnPasswordConfirmed;
            viewModel.ClearSensitiveData();
            dialog.DataContext = null;
        }

        void OnPasswordConfirmed(object? _, string password)
        {
            confirmedPassword = password;
        }
    }

    public string? Prompt(
        string title,
        string message,
        string? errorMessage = null,
        string? requiredErrorMessage = null,
        Func<string, Task<string?>>? validatePasswordAsync = null)
    {
        string? confirmedPassword = null;
        var viewModel = new PasswordPromptViewModel(
            title,
            message,
            _passwordValidationService,
            errorMessage,
            requiredErrorMessage);
        viewModel.ValidatePasswordAsync = validatePasswordAsync;
        viewModel.PasswordConfirmed += OnPasswordConfirmed;

        var dialog = _dialogFactory.CreatePasswordPromptDialog();
        dialog.DataContext = viewModel;
        try
        {
            var result = dialog.ShowDialog();
            if (result != true || string.IsNullOrWhiteSpace(confirmedPassword))
            {
                return null;
            }

            return confirmedPassword;
        }
        finally
        {
            viewModel.PasswordConfirmed -= OnPasswordConfirmed;
            viewModel.ClearSensitiveData();
            dialog.DataContext = null;
        }

        void OnPasswordConfirmed(object? _, string password)
        {
            confirmedPassword = password;
        }
    }

}
