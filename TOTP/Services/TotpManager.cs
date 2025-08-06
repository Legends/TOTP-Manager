using OtpNet;
using System;
using System.Threading.Tasks;
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
        IPlatformSecretDialogService platformSecretDialogService,
        IMessageService messageService,
        ISecretsManager secretsManager,
        IErrorHandler errorHandler)
    {
        _platformSecretDialogService = platformSecretDialogService;
        _messageService = messageService;
        _secretsManager = secretsManager;
        _errorHandler = errorHandler;
    }


    public async Task<(bool success, SecretItem? item)> AddNewSecretAsync()
    {
        try
        {

            while (true)
            {
                var (success, key, value) = _platformSecretDialogService.ShowForm();

                if (!success)
                    return (false, null);

                if (await _secretsManager.AddNewItemAsync(new SecretItem(key!, value!)))
                {
                    var item = new SecretItem(key!, value!);
                    return (true, item);
                }

                _messageService.ShowErrorMessage(string.Format(UI.msg_FailedAddingSecret, key));
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
            return false;
        }
    }

    public async Task UpdateSecretAsync(SecretItem previous, SecretItem updated)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(previous);
            ArgumentNullException.ThrowIfNull(updated);

            if (!previous.Equals(updated))
            {
                await _secretsManager.UpdateItemAsync(previous.Platform, updated);
                _messageService.ShowMessage($"{UI.msg_SecretUpdated}: {previous.Platform}");
            }
        }
        catch (Exception ex)
        {
            _errorHandler.Handle(ex, UI.ex_UpdatingSecret);
        }
    }

    /// <summary>
    ///     Deletes a secret item from the secrets.json file.
    /// </summary>
    /// <param name="item">SecretItem</param>
    /// <returns>true/false</returns>
    public async Task<bool> DeleteSecretAsync(SecretItem item)
    {
        try
        {
            var shouldDelete = _messageService.ShowWarningMessageDialog(
                string.Format(UI.msg_ConfirmDeleteSecret, item.Platform));

            if (shouldDelete)
            {
                await _secretsManager.DeleteItemAsync(item.Platform);
                return await Task.FromResult(true);
            }

            return await Task.FromResult(false);
        }
        catch (Exception ex)
        {
            _errorHandler.Handle(ex, UI.ex_DeletingSecret);
            return await Task.FromResult(false);
        }
    }

}