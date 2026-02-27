using FluentResults;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TOTP.Core.Models;

namespace TOTP.Core.Services.Interfaces;

public interface IOtpDAL : IDisposable
{
    Task<Result<List<OtpEntry>>> GetAllAsync();
    Task<Result> AddNewAsync(OtpEntry newItem);
    Task<Result> UpdateAsync(OtpEntry updated);
    Task<Result> DeleteAccountAsync(OtpEntry account);
    Task<Result> BackupOtpEntriesStorageFileAsync();
    // Added for professional key rotation
    Task<Result> ReEncryptStorageAsync();
}