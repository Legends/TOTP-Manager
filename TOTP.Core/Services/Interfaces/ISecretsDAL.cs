using TOTP.Core.Common;
using TOTP.Core.Models;

namespace TOTP.Core.Services.Interfaces;


/// <summary>
/// - Responsible for CRUD operations on secret items stored in an encrypted local file.
/// - Secret validation via SecretValidator.cs
/// - Secrets file backup
/// 
/// </summary>
public interface ISecretsDAL
{

    /// <summary>
    /// Loads secret by platform name (case insensitive) from storage.
    /// </summary>
    /// <param name="platform"></param>
    /// <returns>Result&lt;SecretItem&gt;</returns>
    Task<OperationResult<SecretItem>> GetSecretByPlatformAsync(string platform);

    //Task<List<SecretItem>> GetAllSecretsAsync();
    /// <summary>
    /// Retrieves all secret items from local storage file.
    /// </summary>
    /// <returns>Success | LoadingFailed</returns>
    Task<OperationResult<List<SecretItem>>> GetAllSecretsAsync();

    /// <summary>
    /// Adds a new secret to the internal collection and writes it to the encrypted secrets file.
    /// </summary>
    /// <param name="item"></param>
    /// <returns>Success | AlreadyExist | LoadingFailed | StorageFailed</returns>
    Task<OperationResult<bool>> AddNewItemAsync(SecretItem item);
    //Task<bool> AddNewItemAsync(SecretItem item);

    /// <summary>
    /// Updates an existing secret item in the encrypted secrets file.
    /// </summary>
    /// <param name="updated"></param>
    /// <returns>Success | NotFound | LoadingFailed | StorageFailed</returns>
    Task<OperationResult<bool>> UpdateItemAsync(SecretItem updated);


    /// <summary>
    /// Deletes a secret item from the internal collection and writes the updated collection to the encrypted secrets file.
    /// </summary>
    /// <param name="platform"></param>
    /// <returns>Success | NotFound | LoadingFailed | StorageFailed</returns>
    Task<OperationResult<bool>> DeleteItemAsync(string platform);

    /// <summary>
    /// Creates a backup of the current secrets .dat file.
    /// </summary>
    /// <returns></returns>
    bool BackupSecretsFile();
}