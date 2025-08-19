using System.Collections.Generic;
using System.Threading.Tasks;
using TOTP.Core;
using TOTP.Core.Models;

namespace TOTP.Core.Interfaces;

public interface ISecretsManager
{

    //Task<List<SecretItem>> GetAllSecretsAsync();
    /// <summary>
    /// Retrieves all secret items from local storage file.
    /// </summary>
    /// <returns>Success | LoadingFailed</returns>
    Task<Result<List<SecretItem>>> GetAllSecretsAsync();

    /// <summary>
    /// Adds a new secret to the internal collection and writes it to the encrypted secrets file.
    /// </summary>
    /// <param name="item"></param>
    /// <returns>Success | AlreadyExist | LoadingFailed | StorageFailed</returns>
    Task<Result<bool>> AddNewItemAsync(SecretItem item);
    //Task<bool> AddNewItemAsync(SecretItem item);

    /// <summary>
    /// Updates an existing secret item in the internal collection and writes it to the encrypted secrets file.
    /// </summary>
    /// <param name="previousPlatform"></param>
    /// <param name="updated"></param>
    /// <returns>Success | NotFound | LoadingFailed | StorageFailed</returns>
    Task<Result<bool>> UpdateItemAsync(string previousPlatform, SecretItem updated);

    /// <summary>
    /// Deletes a secret item from the internal collection and writes the updated collection to the encrypted secrets file.
    /// </summary>
    /// <param name="platform"></param>
    /// <returns>Success | NotFound | LoadingFailed | StorageFailed</returns>
    Task<Result<bool>> DeleteItemAsync(string platform);

    /// <summary>
    /// Creates a backup of the current secrets .dat file.
    /// </summary>
    /// <returns></returns>
    bool BackupSecretsFile();
}