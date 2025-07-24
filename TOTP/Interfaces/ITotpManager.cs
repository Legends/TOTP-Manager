using TOTP.Models;

namespace TOTP.Interfaces;

public interface ITotpManager
{
    /// <summary>
    /// Adds a new TOTP secret item by prompting the user for key and value.
    /// It writes the new item to the secrets file and returns the item if successful.
    /// </summary>
    /// <returns></returns>
    (bool success, SecretItem? item) AddNewSecret();

    bool TryComputeCode(string secret, out string? code, out string? error);

    void UpdateSecret(SecretItem previous, SecretItem updated);

    /// <summary>
    ///     Deletes a secret item from the secrets.json file.
    /// </summary>
    /// <param name="item">SecretItem</param>
    /// <returns>true/false</returns>
    bool DeleteSecret(SecretItem item);
}