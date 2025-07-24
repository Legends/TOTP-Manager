using System.Collections.Generic;
using TOTP.Models;

namespace TOTP.Interfaces;

public interface ISecretsManager
{

    List<SecretItem> GetAllSecrets();
    /// <summary>
    /// Adds a new secret to the internal collection and writes it to the encrypted secrets file.
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    bool AddNewItem(SecretItem item);
    /// <summary>
    /// Updates an existing secret item in the internal collection and writes it to the encrypted secrets file.
    /// </summary>
    /// <param name="previousPlatform"></param>
    /// <param name="updated"></param>
    /// <returns></returns>
    bool UpdateItem(string previousPlatform, SecretItem updated);
    /// <summary>
    /// Deletes a secret item from the internal collection and writes the updated collection to the encrypted secrets file.
    /// </summary>
    /// <param name="platform"></param>
    /// <returns></returns>
    bool DeleteItem(string platform);
    bool BackupSecretsFile();
}