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

/// <summary>
/// Provides methods for managing account items, including adding, updating, retrieving, and deleting accounts.
/// </summary>
/// <param name="secretsDal">The data access layer used to persist and retrieve account information.</param>
/// <param name="logger">The logger used to record operational and error information for the accounts manager.</param>
public class AccountsManager(
    IAccountsDAL secretsDal,
    ILogger<AccountsManager> logger) : IAccountsManager
{
    public async Task<Result> AddNewItemAsync(AccountItem newItem)
    {
        return await secretsDal.AddNewAccountAsync(newItem);
    }

    public async Task<Result> UpdateAccountAsync(AccountItem previous, AccountItem updated)
    {
        ArgumentNullException.ThrowIfNull(updated);
        return await secretsDal.UpdateAccountAsync(updated);
    }

    public async Task<Result<ObservableCollection<AccountItem>>> GetAllAccountsSortedAsync()
    {
        var result = await secretsDal.GetAllAccountsAsync();

        if (result.IsFailed)
            return result.ToResult();

        result.Value.Sort(new Comparison<AccountItem>((a, b) => string.Compare(a.Platform, b.Platform, StringComparison.OrdinalIgnoreCase)));

        var allAccounts = result.Value ?? [];
        return Result.Ok(new ObservableCollection<AccountItem>((allAccounts)));
    }

    public async Task<Result> DeleteAccountAsync(AccountItem item)
    {
        return await secretsDal.DeleteAccountAsync(item);
    }

    public async Task<Result> BackupAccountsStorageFileAsync()
    {
        return await secretsDal.BackupAccountsStorageFileAsync();
    }

}