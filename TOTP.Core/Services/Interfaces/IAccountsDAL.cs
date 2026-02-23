using System.Collections.Generic;
using System.Threading.Tasks;
using FluentResults;
using TOTP.Core.Common;
using TOTP.Core.Models;

namespace TOTP.Core.Services.Interfaces;


/// <summary>
/// - Responsible for CRUD operations on secret items stored in an encrypted local file.
/// - Secret validation via SecretValidator.cs
/// - Secrets file backup
/// 
/// </summary>
public interface IAccountsDAL
{

    ///// <summary>
    ///// Loads secret by platform name (case insensitive) from storage.
    ///// </summary>
    ///// <param name="platform"></param>
    ///// <returns>Result&lt;SecretItem&gt;</returns>
    //Task<Result<AccountItem>> GetSecretByPlatformAsync(string platform);

    //Task<List<SecretItem>> GetAllSecretsAsync();
    /// <summary>
    /// Retrieves all secret items from local storage file.
    /// </summary>
    /// <returns>Success | LoadingFailed</returns>
    Task<Result<List<AccountItem>>> GetAllAccountsAsync();

    /// <summary>
    /// Adds a new secret to the internal collection and writes it to the encrypted secrets file.
    /// </summary>
    /// <param name="item"></param>
    /// <returns>Success | AlreadyExist | LoadingFailed | StorageFailed</returns>
    Task<Result> AddNewAccountAsync(AccountItem item);
    //Task<bool> AddNewItemAsync(SecretItem item);

    /// <summary>
    /// Updates an existing secret item in the encrypted secrets file.
    /// </summary>
    /// <param name="updated"></param>
    /// <returns>Success | NotFound | LoadingFailed | StorageFailed</returns>
    Task<Result> UpdateAccountAsync(AccountItem updated);


    /// <summary>
    /// Deletes a secret item from the internal collection and writes the updated collection to the encrypted secrets file.
    /// </summary>
    /// <param name="account"></param>
    /// <returns>Success | NotFound | LoadingFailed | StorageFailed</returns>
    Task<Result> DeleteAccountAsync(AccountItem account);

    /// <summary>
    /// Creates a backup of the current secrets .dat file.
    /// </summary>
    /// <returns></returns>
    Task<Result> BackupAccountsStorageFileAsync();
}