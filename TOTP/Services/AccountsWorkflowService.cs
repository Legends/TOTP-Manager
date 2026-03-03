using FluentResults;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using TOTP.Core.Enums;
using TOTP.Core.Models;
using TOTP.Core.Services.Interfaces;
using TOTP.Infrastructure.Extensions;
using TOTP.Services.Interfaces;
using TOTP.ViewModels;
using TOTP.Validation;

namespace TOTP.Services;

public sealed class AccountsWorkflowService(IOtpManager otpManager) : IAccountsWorkflowService
{
    public async Task<Result<ObservableCollection<OtpViewModel>>> LoadAllAsync()
    {
        var result = await otpManager.GetAllOtpEntriesSortedAsync();
        if (result.IsFailed)
            return Result.Fail(result.Errors);

        var mapped = new ObservableCollection<OtpViewModel>(result.Value.Select(item => item.ToViewModel()));
        return Result.Ok(mapped);
    }

    public async Task<Result<ObservableCollection<OtpEntry>>> GetAllEntriesSortedAsync()
        => await otpManager.GetAllOtpEntriesSortedAsync();

    public async Task<Result> AddAsync(OtpViewModel item)
        => await otpManager.AddNewAsync(item.ToDomain());

    public async Task<Result> UpdateAsync(OtpViewModel? previous, OtpViewModel updated)
        => await otpManager.UpdateAsync(previous?.ToDomain(), updated.ToDomain());

    public async Task<Result> DeleteAsync(OtpViewModel item)
        => await otpManager.DeleteAsync(item.ToDomain());

    public IReadOnlyList<ValidationError> ValidateForCreate(OtpViewModel item, IEnumerable<OtpViewModel> source)
        => UiValidation.Use(item, source).ValidateAll().PlatformNameDuplicateExists().Errors;

    public IReadOnlyList<ValidationError> ValidateForUpdate(OtpViewModel item, IEnumerable<OtpViewModel> source)
        => UiValidation.Use(item, source).ValidateAll().PlatformNameDuplicateExists(excludeSelf: true).Errors;

    public ValidationError CheckDuplicateIssuer(OtpViewModel current, IEnumerable<OtpViewModel> source)
        => UiValidation.PlatformNameDuplicateExists(current.Issuer, source.Where(item => !item.Equals(current)).Select(it => it.ToDomain()).ToList());
}
