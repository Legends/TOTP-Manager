using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using FluentResults;
using TOTP.Core.Enums;
using TOTP.Core.Events;
using TOTP.Core.Models;
using TOTP.Core.Services.Interfaces;

namespace TOTP.Core.Services;

public class AccountsManager : IAccountsManager
{

    private readonly ILogger<AccountsManager> _logger;
    private readonly IAccountsDAL _secretsDal;

    public AccountsManager(
        IAccountsDAL secretsDal,
        ILogger<AccountsManager> logger)
    {
        _secretsDal = secretsDal;
        _logger = logger;
    }

    //public event Action<object?, OperationStatus, string?> OnMessageSend;
    //public event Func<object?, AddNewPromptArgs>? OnAddNewPrompt;
    public event Func<object?, string, bool>? ConfirmDeleteRequested;

    public async Task<Result> UpdateAccountAsync(AccountItem previous, AccountItem updated)
    {
        //ArgumentNullException.ThrowIfNull(previous);
        ArgumentNullException.ThrowIfNull(updated);

        return await _secretsDal.UpdateItemAsync(updated);

        //var platform = result.Status == OperationStatus.LoadingFailed ? null : previous?.Platform;

        //if (result.Status != OperationStatus.Success)
        //    OnMessageSend?.Invoke(this, result.Status, platform ?? string.Empty);

        //return result.Status == OperationStatus.Success;
    }

    public async Task<Result<ObservableCollection<AccountItem>>> GetAllAccountsSortedAsync()
    {
        var result = await _secretsDal.GetAllAccountsAsync();

        if (result.IsFailed)
            return result.ToResult();
        //if (result.Status != OperationStatus.Success)
        //{
        //    OnMessageSend?.Invoke(this, result.Status, null);
        //}

        result.Value.Sort(new Comparison<AccountItem>((a, b) => string.Compare(a.Platform, b.Platform, StringComparison.OrdinalIgnoreCase)));

        var allAccounts = result.Value ?? [];
        return Result.Ok(new ObservableCollection<AccountItem>((allAccounts)));
    }


    /// <summary>
    ///     Deletes a secret item from the secrets.json file.
    /// </summary>
    /// <param name="item">SecretItem</param>
    /// <returns>true/false</returns>
    public async Task<Result> DeleteAccountAsync(AccountItem item)
    {
        //var shouldDelete = ConfirmDeleteRequested?.Invoke(this, item.Platform) ?? false;

        //if (shouldDelete)
        //{
        return await _secretsDal.DeleteItemAsync(item.Platform);

        //    if (result.Status == OperationStatus.NotFound)
        //    {
        //        OnMessageSend?.Invoke(this, OperationStatus.NotFound, item.Platform);
        //    }
        //    else if (result.Status == OperationStatus.LoadingFailed)
        //    {
        //        OnMessageSend?.Invoke(this, OperationStatus.LoadingFailed, item.Platform);
        //    }
        //    else if (result.Status == OperationStatus.StorageFailed)
        //    {
        //        OnMessageSend?.Invoke(this, OperationStatus.StorageFailed, item.Platform);
        //    }

        //    return true;
        //}

        //return false;
    }


}