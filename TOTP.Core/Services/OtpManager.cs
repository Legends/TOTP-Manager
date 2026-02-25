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
/// <param name="otpDal">The data access layer used to persist and retrieve account information.</param>
/// <param name="logger">The logger used to record operational and error information for the accounts manager.</param>
public class OtpManager(
    IOtpDAL otpDal,
    ILogger<OtpManager> logger) : IOtpManager
{
    public async Task<Result> AddNewAsync(OtpEntry newItem)
    {
        return await otpDal.AddNewAsync(newItem);
    }

    public async Task<Result> UpdateAsync(OtpEntry previous, OtpEntry updated)
    {
        ArgumentNullException.ThrowIfNull(updated);
        return await otpDal.UpdateAsync(updated);
    }

    public async Task<Result<ObservableCollection<OtpEntry>>> GetAllOtpEntriesSortedAsync()
    {
        var result = await otpDal.GetAllAsync();

        if (result.IsFailed)
            return result.ToResult();

        result.Value.Sort(new Comparison<OtpEntry>((a, b) => string.Compare(a.Issuer, b.Issuer, StringComparison.OrdinalIgnoreCase)));

        var allAccounts = result.Value ?? [];
        return Result.Ok(new ObservableCollection<OtpEntry>((allAccounts)));
    }

    public async Task<Result> DeleteAsync(OtpEntry item)
    {
        return await otpDal.DeleteAccountAsync(item);
    }

    public async Task<Result> BackupOtpEntriesStorageFileAsync()
    {
        return await otpDal.BackupOtpEntriesStorageFileAsync();
    }

}