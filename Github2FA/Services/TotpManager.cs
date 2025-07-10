using Github2FA.Interfaces;
using Github2FA.Models;
using OtpNet;
using System;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace Github2FA.Services;

public class TotpManager : ITotpManager
{
    private readonly IDialogService _dialogService;
    private readonly IMessageService _messageService;
    private readonly ISecretsHelper _secretsHelper;
    private readonly IErrorHandler _errorHandler;

    public TotpManager(
        IDialogService dialogService,
        IMessageService messageService,
        ISecretsHelper secretsHelper,
        IErrorHandler errorHandler)
    {
        _dialogService = dialogService;
        _messageService = messageService;
        _secretsHelper = secretsHelper;
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

                if (!IsValidBase32Format(value))
                {
                    _messageService.ShowMessage("Secret must be a valid Base32 string.", "Error");
                    continue;
                }

                if (_secretsHelper.AddNewItemToSecretsFile(key, value))
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

            if (ex is FormatException || ex is ArgumentException)
            {
                error = "Invalid secret format. Please ensure it is a valid Base32 string.";
            }
            else
            {
                error = $"An unexpected error occurred while computing the TOTP code.{Environment.NewLine}{ex.Message}";
            }
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
                _secretsHelper.UpdateItemInSecretsFile(previous.Key, updated);
                _messageService.ShowMessage($"Updated secret: {previous.Key}");
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
                $"Are you sure you want to delete the secret: {item.Key}?",
                "Confirm Delete");

            if (shouldDelete)
            {
                _secretsHelper.DeleteItemFromSecretsFile(item.Key);
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

    private bool IsValidBase32Format(string value)
    {
        try
        {
            _ = Base32Encoding.ToBytes(value);
            return true;
        }
        catch
        {
            return false;
        }
    }

    // Base32 encoding: Only uppercase letters (A–Z) and digits 2–7
    // ❌ No lowercase letters, symbols, or whitespace
    private static readonly Regex Base32Regex = new Regex("^[A-Z2-7]+=*$", RegexOptions.None);

    public bool IsValidBase32Strict(string input)
    {
        if (string.IsNullOrEmpty(input))
            return false;

        return Base32Regex.IsMatch(input);
    }
}
