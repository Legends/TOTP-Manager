using FluentResults;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TOTP.Core.Common;
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

    public async Task<Result> AddNewItemAsync(AccountItem newItem)
    {
        return await _secretsDal.AddNewAccountAsync(newItem);
    }

    public async Task<Result> UpdateAccountAsync(AccountItem previous, AccountItem updated)
    {
        //ArgumentNullException.ThrowIfNull(previous);
        ArgumentNullException.ThrowIfNull(updated);

        return await _secretsDal.UpdateAccountAsync(updated);

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
        return await _secretsDal.DeleteAccountAsync(item);
    }


}