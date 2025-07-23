using OtpNet;
using System;
using TOTP.Interfaces;
using TOTP.Models;

namespace TOTP.Services;

public class TotpManager : ITotpManager
{
    private readonly IDialogService _dialogService;
    private readonly IMessageService _messageService;
    private readonly ISecretsManager _SecretsManager;
    private readonly IErrorHandler _errorHandler;

    public TotpManager(
        IDialogService dialogService,
        IMessageService messageService,
        ISecretsManager secretsManager,
        IErrorHandler errorHandler)
    {
        _dialogService = dialogService;
        _messageService = messageService;
        _SecretsManager = secretsManager;
        _errorHandler = errorHandler;
    }

    /// <summary>
    /// Adds a new TOTP secret to the secrets.json file by prompting the user for a key and value.
    /// </summary>
    /// <returns>bool for success and the secretItem/null</returns>
    public (bool success, SecretItem? item) PromptAndAddTotp()
    {
        try
        {
            string? lastKey = null;
            string? lastValue = null;

            while (true)
            {
                var (success, key, value) = _dialogService.ShowKeyValueDialog(lastKey, lastValue);

                if (!success)
                    return (false, null);

                lastKey = key;
                lastValue = value;

                if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
                {
                    _messageService.ShowMessage("Key and Value cannot be empty.", "Error");
                    continue;
                }

                if (!SecretsManager.IsValidBase32Format(value))
                {
                    _messageService.ShowMessage("Secret must be a valid Base32 string.", "Error");
                    continue;
                }

                if (_SecretsManager.AddNewItem(new SecretItem(key, value)))
                {
                    var item = new SecretItem(key, value);
                    return (true, item);
                }
                else
                {
                    _messageService.ShowMessage($"Failed to set secret: {key}", "Error");
                    return (false, null);
                }
            }
        }
        catch (Exception ex)
        {
            _errorHandler.Handle(ex, "Unexpected error adding secret.");
            return (false, null);
        }
    }

    public bool TryComputeCode(string secret, out string? code, out string? error)
    {
        try
        {
            var totp = new Totp(Base32Encoding.ToBytes(secret));
            code = totp.ComputeTotp();
            error = null;
            return true;
        }
        catch (Exception ex)
        {
            code = null;

            error = ex is FormatException || ex is ArgumentException
                ? "Invalid secret format. Please ensure it is a valid Base32 string."
                : $"An unexpected error occurred while computing the TOTP code.{Environment.NewLine}{ex.Message}";
            return false;
        }
    }

    public void UpdateSecret(SecretItem previous, SecretItem updated)
    {
        try
        {
            if (previous == null || updated == null)
                return;

            if (!previous.Equals(updated))
            {
                _SecretsManager.UpdateItem(previous.Platform, updated);
                _messageService.ShowMessage($"Updated secret: {previous.Platform}");
            }
        }
        catch (Exception ex)
        {
            _errorHandler.Handle(ex, "Error updating secret.");
        }
    }

    /// <summary>
    /// Deletes a secret item from the secrets.json file.
    /// </summary>
    /// <param name="item">SecretItem</param>
    /// <returns>true/false</returns>
    public bool DeleteSecret(SecretItem item)
    {
        try
        {
            if (item == null)
                return false;

            var shouldDelete = _messageService.ShowMessageDialog(
                $"Are you sure you want to delete the secret: {item.Platform}?",
                "Confirm Delete");

            if (shouldDelete)
            {
                _SecretsManager.DeleteItem(item.Platform);
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            _errorHandler.Handle(ex, "Error deleting secret.");
            return false;
        }
    }


}
