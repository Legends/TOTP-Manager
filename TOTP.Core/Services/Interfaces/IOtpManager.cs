using FluentResults;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TOTP.Core.Enums;
using TOTP.Core.Events;
using TOTP.Core.Models;

namespace TOTP.Core.Services.Interfaces;

/// <summary>
/// Basically a higher level manager that uses ISecretsManager to perform TOTP related operations.
/// Operations:
/// Add new
/// Update
/// Delete
/// </summary>
public interface IOtpManager
{
    Task<Result> BackupOtpEntriesStorageFileAsync();
    Task<Result> AddNewAsync(OtpEntry newItem);
    Task<Result<ObservableCollection<OtpEntry>>> GetAllOtpEntriesSortedAsync();

    ///// <summary>
    ///// Adds a new secret item by prompting the user for key and value.
    ///// It writes the new item to the secrets file and returns the item if successful.
    ///// </summary>
    ///// <returns></returns>
    //Task<(bool isSuccess, SecretItem? item)> AddNewSecretAsync();

    /// <summary>
    /// Updates an existing secret item in the encrypted secrets file.
    /// And manages user message handling via OnMessageSend event.
    /// </summary>
    /// <param name="previous"></param>
    /// <param name="updated"></param>
    /// <returns></returns>
    Task<Result> UpdateAsync(OtpEntry previous, OtpEntry updated);

    /// <summary>
    /// Deletes a secret item from the encrypted secrets file.
    /// </summary>
    /// <param name="item">SecretItem</param>
    /// <returns>true/false</returns>
    Task<Result> DeleteAsync(OtpEntry item);

 
}