using FluentResults;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TOTP.Core.Enums;
using TOTP.Core.Models;
using TOTP.ViewModels;

namespace TOTP.Services.Interfaces;

public interface IAccountsWorkflowService
{
    Task<Result<ObservableCollection<OtpViewModel>>> LoadAllAsync();
    Task<Result<ObservableCollection<Account>>> GetAllEntriesSortedAsync();
    Task<Result> AddAsync(OtpViewModel item);
    Task<Result> UpdateAsync(OtpViewModel? previous, OtpViewModel updated);
    Task<Result> DeleteAsync(OtpViewModel item);
    IReadOnlyList<ValidationError> ValidateForCreate(OtpViewModel item, IEnumerable<OtpViewModel> source);
    IReadOnlyList<ValidationError> ValidateForUpdate(OtpViewModel item, IEnumerable<OtpViewModel> source);
    ValidationError CheckDuplicateIssuer(OtpViewModel current, IEnumerable<OtpViewModel> source);
}

