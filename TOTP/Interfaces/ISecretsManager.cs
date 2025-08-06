using System.Collections.Generic;
using System.Threading.Tasks;
using TOTP.Models;

namespace TOTP.Interfaces;

public interface ISecretsManager
{

    Task<List<SecretItem>> GetAllSecretsAsync();

    /// <summary>
    /// Adds a new secret to the internal collection and writes it to the encrypted secrets file.
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    Task<bool> AddNewItemAsync(SecretItem item);

    /// <summary>
    /// Updates an existing secret item in the internal collection and writes it to the encrypted secrets file.
    /// </summary>
    /// <param name="previousPlatform"></param>
    /// <param name="updated"></param>
    /// <returns></returns>
    Task<bool> UpdateItemAsync(string previousPlatform, SecretItem updated);

    /// <summary>
    /// Deletes a secret item from the internal collection and writes the updated collection to the encrypted secrets file.
    /// </summary>
    /// <param name="platform"></param>
    /// <returns></returns>
    Task<bool> DeleteItemAsync(string platform);

    /// <summary>
    /// Creates a backup of the current secrets .dat file.
    /// </summary>
    /// <returns></returns>
    Task<bool> BackupSecretsFileAsync();
}