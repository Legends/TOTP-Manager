using Github2FA.Interfaces;
using Github2FA.Models;
using OtpNet;
using System;

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

    public void DeleteSecret(SecretItem item)
    {
        try
        {
            if (item == null)
                return;

            var shouldDelete = _messageService.ShowMessageDialog(
                $"Are you sure you want to delete the secret: {item.Key}?",
                "Confirm Delete");

            if (shouldDelete)
            {
                _secretsHelper.DeleteItemFromSecretsFile(item.Key);
            }
        }
        catch (Exception ex)
        {
            _errorHandler.Handle(ex, "Error deleting secret.");
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
}
