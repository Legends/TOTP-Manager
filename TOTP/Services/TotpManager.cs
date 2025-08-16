using OtpNet;
using System;
using System.Threading.Tasks;
using TOTP.Enums;
using TOTP.Events;
using TOTP.Interfaces;
using TOTP.Models;
using TOTP.Resources;

namespace TOTP.Services;

public class TotpManager : ITotpManager
{
    private readonly IPlatformSecretDialogService _platformSecretDialogService;
    private readonly IErrorHandler _errorHandler;
    private readonly IMessageService _messageService;
    private readonly ISecretsManager _secretsManager;

    public TotpManager(
        //IPlatformSecretDialogService platformSecretDialogService,
        IMessageService messageService,
        ISecretsManager secretsManager,
        IErrorHandler errorHandler)
    {
        //_platformSecretDialogService = platformSecretDialogService;
        //_messageService = messageService;
        _secretsManager = secretsManager;
        _errorHandler = errorHandler;
    }

    public event Action<Object?, OperationStatus, string?> OnMessageSend;
    public event Func<object?, AddNewPromptArgs>? OnAddNewPrompt;
    public event Func<object?, string, bool>? ConfirmDeleteRequested;


    public async Task<(bool success, SecretItem? item)> AddNewSecretAsync()
    {
        try
        {
            while (true)
            {
                // Prompt the user for a new secret key and value
                var promptResult = OnAddNewPrompt?.Invoke(this);

                if (!promptResult!.Success)
                    return (false, null);

                var secretItem = new SecretItem(promptResult.Key!, promptResult.Value!);
                var result = await _secretsManager.AddNewItemAsync(secretItem);

                if (result.status == OperationStatus.Success)
                    return (true, secretItem);

                if (result.status == OperationStatus.AlreadyExists)
                {
                    OnMessageSend?.Invoke(this, OperationStatus.AlreadyExists, promptResult.Key);
                    continue;
                }
                if (result.status == OperationStatus.StorageFailed)
                {
                    OnMessageSend?.Invoke(this, OperationStatus.StorageFailed, promptResult.Key);
                }

                if (result.status == OperationStatus.LoadingFailed)
                {
                    OnMessageSend?.Invoke(this, OperationStatus.LoadingFailed, null);
                }

                OnMessageSend?.Invoke(this, OperationStatus.CreateFailed, promptResult.Key);

                return (false, null);
            }
        }
        catch (Exception ex)
        {
            _errorHandler.Handle(ex, UI.ex_UnexpectedError);
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
                ? UI.ex_InvalidSecret
                : $"{UI.ex_UnexpectedError}.{Environment.NewLine}{ex.Message}";
            _errorHandler.Handle(ex, error);
            return false;
        }
    }

    public async Task UpdateSecretAsync(SecretItem previous, SecretItem updated)
    {
        //try
        //{
        ArgumentNullException.ThrowIfNull(previous);
        ArgumentNullException.ThrowIfNull(updated);

        if (!previous.Equals(updated))
        {
            var result = await _secretsManager.UpdateItemAsync(previous.Platform, updated);

            if (result.status == OperationStatus.NotFound)
            {
                OnMessageSend?.Invoke(this, OperationStatus.NotFound, previous.Platform);
            }
            else if (result.status == OperationStatus.LoadingFailed)
            {
                OnMessageSend?.Invoke(this, OperationStatus.LoadingFailed, null);
            }
            else if (result.status == OperationStatus.Success)
            {
                OnMessageSend?.Invoke(this, OperationStatus.Success, previous.Platform);
            }
            else if (result.status == OperationStatus.StorageFailed)
            {
                OnMessageSend?.Invoke(this, OperationStatus.StorageFailed, previous.Platform);
            }
            else
            {
                OnMessageSend?.Invoke(this, OperationStatus.UpdateFailed, previous.Platform);
            }

        }
        //}
        //catch (Exception ex)
        //{
        //    _errorHandler.Handle(ex, UI.ex_UpdatingSecret);
        //}
    }

    /// <summary>
    ///     Deletes a secret item from the secrets.json file.
    /// </summary>
    /// <param name="item">SecretItem</param>
    /// <returns>true/false</returns>
    public async Task<bool> DeleteSecretAsync(SecretItem item)
    {
        //try
        //{

        var shouldDelete = ConfirmDeleteRequested?.Invoke(this, item.Platform) ?? false;

        if (shouldDelete)
        {
            var result = await _secretsManager.DeleteItemAsync(item.Platform);

            if (result.status == OperationStatus.NotFound)
            {
                OnMessageSend?.Invoke(this, OperationStatus.NotFound, item.Platform);
            }
            else if (result.status == OperationStatus.LoadingFailed)
            {
                OnMessageSend?.Invoke(this, OperationStatus.LoadingFailed, item.Platform);
            }
            else if (result.status == OperationStatus.StorageFailed)
            {
                OnMessageSend?.Invoke(this, OperationStatus.StorageFailed, item.Platform);
            }

            return true;
        }

        return false;
    }
    //catch (Exception ex)
    //{
    //    _errorHandler.Handle(ex, UI.ex_DeletingSecret);
    //    return await Task.FromResult(false);
    //}


}