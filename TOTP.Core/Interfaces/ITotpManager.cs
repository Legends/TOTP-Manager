using TOTP.Core.Enums;
using TOTP.Core.Events;
using TOTP.Core.Models;

namespace TOTP.Core.Interfaces;

public interface ITotpManager
{

    bool TryComputeCode(string secret, out string? code, out Exception? exc);

    /// <summary>
    /// Adds a new TOTP secret item by prompting the user for key and value.
    /// It writes the new item to the secrets file and returns the item if successful.
    /// </summary>
    /// <returns></returns>
    Task<(bool isSuccess, SecretItem? item)> AddNewSecretAsync();

    Task<bool> UpdateSecretAsync(SecretItem previous, SecretItem updated, List<SecretItem> source);

    /// <summary>
    ///     Deletes a secret item from the secrets.json file.
    /// </summary>
    /// <param name="item">SecretItem</param>
    /// <returns>true/false</returns>
    Task<bool> DeleteSecretAsync(SecretItem item);

    /// <summary>
    /// Called when a message needs to be displayed
    /// </summary>
    event Action<object, OperationStatus, string?> OnMessageSend;

    event Func<object?, AddNewPromptArgs> OnAddNewPrompt;

    event Func<object?, string, bool> ConfirmDeleteRequested;
}