using System;
using System.Threading.Tasks;
using TOTP.Core.Enums;
using TOTP.Core.Events;
using TOTP.Core.Models;

namespace TOTP.Core.Interfaces;

public interface ITotpManager
{

    bool TryComputeCode(string secret, out string? code, out string? error);

    /// <summary>
    /// Adds a new TOTP secret item by prompting the user for key and value.
    /// It writes the new item to the secrets file and returns the item if successful.
    /// </summary>
    /// <returns></returns>
    Task<(bool success, SecretItem? item)> AddNewSecretAsync();

    Task UpdateSecretAsync(SecretItem previous, SecretItem updated);

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