using Microsoft.Extensions.Logging;
using OtpNet;
using TOTP.Core.Enums;
using TOTP.Core.Events;
using TOTP.Core.Interfaces;
using TOTP.Core.Models;
using TOTP.Core.Validation;
using TOTP.Interfaces;

namespace TOTP.Services;

public class TotpManager : ITotpManager
{
    //private readonly IPlatformSecretDialogService _platformSecretDialogService;
    private readonly ILogger<TotpManager> _logger;
    private readonly ISecretsManager _secretsManager;

    public TotpManager(
        ISecretsManager secretsManager,
        ILogger<TotpManager> logger)
    {
        _secretsManager = secretsManager;
        _logger = logger;
    }

    public event Action<Object?, OperationStatus, string?> OnMessageSend;
    public event Func<object?, AddNewPromptArgs>? OnAddNewPrompt;
    public event Func<object?, string, bool>? ConfirmDeleteRequested;


    public async Task<(bool success, SecretItem? item)> AddNewSecretAsync()
    {

        while (true)
        {
            // Prompt the user for a new secret key and value
            var promptResult = OnAddNewPrompt?.Invoke(this);

            if (!promptResult!.Success)
                return (false, null);

            var secretItem = new SecretItem(promptResult.Platform!, promptResult.Secret!);
            var result = await _secretsManager.AddNewItemAsync(secretItem);

            if (result.Status == OperationStatus.Success)
                return (true, secretItem);

            if (result.Status == OperationStatus.AlreadyExists)
            {
                OnMessageSend?.Invoke(this, OperationStatus.AlreadyExists, promptResult.Platform);
                continue;
            }
            if (result.Status == OperationStatus.StorageFailed)
            {
                OnMessageSend?.Invoke(this, OperationStatus.StorageFailed, promptResult.Platform);
            }

            if (result.Status == OperationStatus.LoadingFailed)
            {
                OnMessageSend?.Invoke(this, OperationStatus.LoadingFailed, null);
            }

            OnMessageSend?.Invoke(this, OperationStatus.CreateFailed, promptResult.Platform);

            return (false, null);
        }
    }

    public bool TryComputeCode(string secret, out string? code, out Exception? exc)
    {
        try
        {
            if (!SecretValidator.IsValidBase32Format(secret))
            {
                code = null;
                exc = new FormatException($"Secret is invalid Base32 format, supplied to {nameof(TryComputeCode)}");
                return false;
            }

            var encodedSecret = OtpNet.Base32Encoding.ToBytes(secret);
            var totp = new Totp(encodedSecret);
            code = totp.ComputeTotp();
            exc = null;
            return true;
        }
        catch (Exception ex)
        {
            code = null;

            exc = ex;
            _logger.LogError(ex.Message, ex);
            return false;
        }
    }

    public async Task<bool> UpdateSecretAsync(SecretItem previous, SecretItem updated, List<SecretItem> source)
    {
        ArgumentNullException.ThrowIfNull(previous);
        ArgumentNullException.ThrowIfNull(updated);

        if (previous.Equals(updated))
            return false;

        if (!string.Equals(previous.Platform, updated.Platform, StringComparison.OrdinalIgnoreCase) &&
            SecretValidator.CheckForPlatformDuplicates(updated.Platform, source) != ValidationError.None)
        {
            OnMessageSend?.Invoke(this, OperationStatus.AlreadyExists, updated.Platform);
            return false;
        }

        var result = await _secretsManager.UpdateItemAsync(previous.Platform, updated);

        var platform = result.Status == OperationStatus.LoadingFailed ? null : previous.Platform;

        var status = result.Status switch
        {
            OperationStatus.Success => OperationStatus.Success,
            OperationStatus.NotFound => OperationStatus.NotFound,
            OperationStatus.LoadingFailed => OperationStatus.LoadingFailed,
            OperationStatus.StorageFailed => OperationStatus.StorageFailed,
            _ => OperationStatus.UpdateFailed
        };

        OnMessageSend?.Invoke(this, status, platform);

        return result.Status == OperationStatus.Success;
    }


    /// <summary>
    ///     Deletes a secret item from the secrets.json file.
    /// </summary>
    /// <param name="item">SecretItem</param>
    /// <returns>true/false</returns>
    public async Task<bool> DeleteSecretAsync(SecretItem item)
    {
        var shouldDelete = ConfirmDeleteRequested?.Invoke(this, item.Platform) ?? false;

        if (shouldDelete)
        {
            var result = await _secretsManager.DeleteItemAsync(item.Platform);

            if (result.Status == OperationStatus.NotFound)
            {
                OnMessageSend?.Invoke(this, OperationStatus.NotFound, item.Platform);
            }
            else if (result.Status == OperationStatus.LoadingFailed)
            {
                OnMessageSend?.Invoke(this, OperationStatus.LoadingFailed, item.Platform);
            }
            else if (result.Status == OperationStatus.StorageFailed)
            {
                OnMessageSend?.Invoke(this, OperationStatus.StorageFailed, item.Platform);
            }

            return true;
        }

        return false;
    }


}