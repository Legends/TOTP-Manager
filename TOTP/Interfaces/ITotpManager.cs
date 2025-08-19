using System;
using System.Threading.Tasks;
using TOTP.Core.Enums;
using TOTP.Events;
using TOTP.ViewModels;

namespace TOTP.Interfaces;

public interface ITotpManager
{

    bool TryComputeCode(string secret, out string? code, out string? error);

    /// <summary>
    /// Adds a new TOTP secret item by prompting the user for key and value.
    /// It writes the new item to the secrets file and returns the item if successful.
    /// </summary>
    /// <returns></returns>
    Task<(bool success, SecretItemViewModel? item)> AddNewSecretAsync();

    Task UpdateSecretAsync(SecretItemViewModel previous, SecretItemViewModel updated);

    /// <summary>
    ///     Deletes a secret item from the secrets.json file.
    /// </summary>
    /// <param name="item">SecretItem</param>
    /// <returns>true/false</returns>
    Task<bool> DeleteSecretAsync(SecretItemViewModel item);

    /// <summary>
    /// Called when a message needs to be displayed
    /// </summary>
    event Action<object, OperationStatus, string?> OnMessageSend;

    event Func<object?, AddNewPromptArgs> OnAddNewPrompt;

    event Func<object?, string, bool> ConfirmDeleteRequested;
}