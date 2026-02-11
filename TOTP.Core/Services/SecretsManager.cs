using Microsoft.Extensions.Logging;
using TOTP.Core.Enums;
using TOTP.Core.Events;
using TOTP.Core.Models;
using TOTP.Core.Services.Interfaces;

namespace TOTP.Core.Services;

public class SecretsManager : ISecretsManager
{

    private readonly ILogger<SecretsManager> _logger;
    private readonly ISecretsDAL _secretsDal;

    public SecretsManager(
        ISecretsDAL secretsDal,
        ILogger<SecretsManager> logger)
    {
        _secretsDal = secretsDal;
        _logger = logger;
    }

    public event Action<object?, OperationStatus, string?> OnMessageSend;
    //public event Func<object?, AddNewPromptArgs>? OnAddNewPrompt;
    public event Func<object?, string, bool>? ConfirmDeleteRequested;


    //public async Task<(bool isSuccess, SecretItem? item)> AddNewSecretAsync()
    //{

    //    while (true)
    //    {
    //        // Prompt the user for a new secret key and value
    //        // The OnAddNewPrompt event is triggered here and handled by the MainViewModel to show a dialog and gathers user input
    //        var promptResult = OnAddNewPrompt?.Invoke(this);

    //        if (!promptResult!.Success)
    //            return (false, null); // user cancelled

    //        var secretItem = new SecretItem(Guid.NewGuid(), promptResult.Platform!, promptResult.Secret!, promptResult.Account);
    //        var result = await _secretsDal.AddNewItemAsync(secretItem);

    //        if (result.Status == OperationStatus.Success)
    //            return (true, secretItem);

    //        if (result.Status == OperationStatus.AlreadyExists) // platform name already exists
    //        {
    //            OnMessageSend?.Invoke(this, OperationStatus.AlreadyExists, promptResult.Platform);
    //            continue;
    //        }

    //        OnMessageSend?.Invoke(this, result.Status, promptResult.Platform);
    //        OnMessageSend?.Invoke(this, OperationStatus.CreateFailed, promptResult.Platform);

    //        return (false, null);
    //    }
    //}

    public async Task<bool> UpdateSecretAsync(SecretItem previous, SecretItem updated)
    {
        //ArgumentNullException.ThrowIfNull(previous);
        ArgumentNullException.ThrowIfNull(updated);

        var result = await _secretsDal.UpdateItemAsync(updated);

        var platform = result.Status == OperationStatus.LoadingFailed ? null : previous?.Platform;

        if (result.Status != OperationStatus.Success)
            OnMessageSend?.Invoke(this, result.Status, platform ?? string.Empty);

        return result.Status == OperationStatus.Success;
    }

    //public async Task<bool> UpdateSecretAsync(SecretItem previous, SecretItem updated)
    //{
    //    ArgumentNullException.ThrowIfNull(previous);
    //    ArgumentNullException.ThrowIfNull(updated);

    //    var result = await _secretsDal.UpdateItemAsync(previous, updated);

    //    var platform = result.Status == OperationStatus.LoadingFailed ? null : previous.Platform;

    //    if (result.Status != OperationStatus.Success)
    //        OnMessageSend?.Invoke(this, result.Status, platform);

    //    return result.Status == OperationStatus.Success;
    //}


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
            var result = await _secretsDal.DeleteItemAsync(item.Platform);

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

    public bool TryComputeTotpCode(string secret, out string? code, out int remainingSeconds, out Exception? exc)
    {
        throw new NotImplementedException();
    }
}