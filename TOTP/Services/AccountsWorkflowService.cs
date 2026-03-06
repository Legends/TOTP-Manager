using FluentResults;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using TOTP.Core.Common;
using TOTP.Core.Enums;
using TOTP.Core.Models;
using TOTP.Core.Services.Interfaces;
using TOTP.Infrastructure.Extensions;
using TOTP.Services.Interfaces;
using TOTP.ViewModels;
using TOTP.Validation;

namespace TOTP.Services;

public sealed class AccountsWorkflowService(
    IOtpManager otpManager,
    ILogger<AccountsWorkflowService> logger) : IAccountsWorkflowService
{
    public async Task<Result<ObservableCollection<OtpViewModel>>> LoadAllAsync()
    {
        try
        {
            var result = await otpManager.GetAllOtpEntriesSortedAsync();
            if (result.IsFailed)
                return Result.Fail(result.Errors);

            var mapped = new ObservableCollection<OtpViewModel>(result.Value.Select(item => item.ToViewModel()));
            return Result.Ok(mapped);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to load OTP view models.");
            return Result.Fail(new AppError(AppErrorCode.AccountsLoadFailed, "Failed to load OTP entries for the UI workflow.", ex));
        }
    }

    public async Task<Result<ObservableCollection<Account>>> GetAllEntriesSortedAsync()
    {
        try
        {
            return await otpManager.GetAllOtpEntriesSortedAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to load OTP entries.");
            return Result.Fail(new AppError(AppErrorCode.AccountsLoadFailed, "Failed to load OTP entries.", ex));
        }
    }

    public async Task<Result> AddAsync(OtpViewModel item)
    {
        try
        {
            return await otpManager.AddNewAsync(item.ToDomain());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to add OTP entry.");
            return Result.Fail(new AppError(AppErrorCode.AccountsCreateFailed, "Failed to create OTP entry in workflow.", ex));
        }
    }

    public async Task<Result> UpdateAsync(OtpViewModel? previous, OtpViewModel updated)
    {
        try
        {
            return await otpManager.UpdateAsync(previous?.ToDomain()!, updated.ToDomain());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update OTP entry.");
            return Result.Fail(new AppError(AppErrorCode.AccountsUpdateFailed, "Failed to update OTP entry in workflow.", ex));
        }
    }

    public async Task<Result> DeleteAsync(OtpViewModel item)
    {
        try
        {
            return await otpManager.DeleteAsync(item.ToDomain());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete OTP entry.");
            return Result.Fail(new AppError(AppErrorCode.AccountsDeleteFailed, "Failed to delete OTP entry in workflow.", ex));
        }
    }

    public IReadOnlyList<ValidationError> ValidateForCreate(OtpViewModel item, IEnumerable<OtpViewModel> source)
        => UiValidation.Use(item, source).ValidateAll().PlatformNameDuplicateExists().Errors;

    public IReadOnlyList<ValidationError> ValidateForUpdate(OtpViewModel item, IEnumerable<OtpViewModel> source)
        => UiValidation.Use(item, source).ValidateAll().PlatformNameDuplicateExists(excludeSelf: true).Errors;

    public ValidationError CheckDuplicateIssuer(OtpViewModel current, IEnumerable<OtpViewModel> source)
        => UiValidation.PlatformNameDuplicateExists(current.Issuer ?? string.Empty, source.Where(item => !item.Equals(current)).Select(it => it.ToDomain()).ToList());
}
