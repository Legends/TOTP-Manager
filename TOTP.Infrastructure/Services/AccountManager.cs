using System.Collections.ObjectModel;
using FluentResults;
using TOTP.Core.Models;
using TOTP.Core.Services.Interfaces;

namespace TOTP.Infrastructure.Services;

/// <summary>
/// Provides methods for managing otp items, including adding, updating, retrieving, and deleting otps.
/// </summary>
/// <param name="otpDal">The data access layer used to persist and retrieve otp information.</param>
public class AccountManager(
    IAccountDAL otpDal) : IAccountManager
{
    public async Task<Result> AddNewAsync(Account newItem)
    {
        return await otpDal.AddNewAsync(newItem);
    }

    public async Task<Result> UpdateAsync(Account previous, Account updated)
    {
        ArgumentNullException.ThrowIfNull(updated);
        return await otpDal.UpdateAsync(updated);
    }

    public async Task<Result<ObservableCollection<Account>>> GetAllOtpEntriesSortedAsync()
    {
        var result = await otpDal.GetAllAsync();

        if (result.IsFailed)
            return result.ToResult();

        result.Value.Sort(new Comparison<Account>((a, b) => string.Compare(a.Issuer, b.Issuer, StringComparison.OrdinalIgnoreCase)));

        var allOtps = result.Value ?? [];
        return Result.Ok(new ObservableCollection<Account>((allOtps)));
    }

    public async Task<Result> DeleteAsync(Account item)
    {
        return await otpDal.DeleteAsync(item);
    }

    public async Task<Result> BackupOtpEntriesStorageFileAsync()
    {
        return await otpDal.BackupOtpEntriesStorageFileAsync();
    }

}
