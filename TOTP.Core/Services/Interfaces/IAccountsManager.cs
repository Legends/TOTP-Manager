using System;
using System.Threading.Tasks;
using FluentResults;
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
public interface IAccountsManager
{

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
    Task<Result> UpdateAccountAsync(AccountItem previous, AccountItem updated);

    /// <summary>
    /// Deletes a secret item from the encrypted secrets file.
    /// </summary>
    /// <param name="item">SecretItem</param>
    /// <returns>true/false</returns>
    Task<Result> DeleteAccountAsync(AccountItem item);

    /// <summary>
    /// Called when a message needs to be displayed
    /// </summary>
    //event Action<object, OperationStatus, string?> OnMessageSend;

   

    //event Func<object?, string, bool> ConfirmDeleteRequested;
}