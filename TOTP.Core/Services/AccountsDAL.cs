using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TOTP.Core.Common;
using TOTP.Core.Enums;
using TOTP.Core.Models;
using TOTP.Core.Services.Interfaces;

namespace TOTP.Core.Services;

public class AccountsDAL : IAccountsDAL, IDisposable
{
    private readonly JsonSerializerOptions? _options = null;
    private readonly string _secretsPath;
    private static readonly SemaphoreSlim Semaphore = new(1, 1);
    private readonly ILogger<AccountsDAL> _logger;

    public AccountsDAL(ILogger<AccountsDAL> logger, string? storageFilePath)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _secretsPath = storageFilePath ?? AlternativeStoragePath();
    }

    private static string AlternativeStoragePath()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "TOTP-Manager", "secrets.dat");
    }

    //Task<OperationResult<List<AccountItem>>> GetAllAccountsAsync
    public async Task<OperationResult<List<AccountItem>>> GetAllAccountsAsync()
    {
        await Semaphore.WaitAsync();
        try
        {
            var (success, list) = await LoadAccountsFromFileAsync();
            return success ?
                OperationResult<List<AccountItem>>.Success(list)
                : OperationResult<List<AccountItem>>.Fail(OperationStatus.LoadingFailed);
        }
        finally
        {
            Semaphore.Release();
        }
    }


    public async Task<OperationResult<AccountItem>> GetSecretByPlatformAsync(string platform)
    {

        await Semaphore.WaitAsync();
        try
        {
            var (success, list) = await LoadAccountsFromFileAsync();
            var secret = list.Where(s => s.Platform.ToLowerInvariant() == platform.ToLowerInvariant()).FirstOrDefault();
            return success ?
                OperationResult<AccountItem>.Success(secret)
                : OperationResult<AccountItem>.Fail(OperationStatus.LoadingFailed);
        }
        finally
        {
            Semaphore.Release();
        }
    }


    public async Task<OperationResult<bool>> AddNewItemAsync(AccountItem newItem)
    {
        await Semaphore.WaitAsync();
        try
        {
            var (success, list) = await LoadAccountsFromFileAsync();
            if (!success) return OperationResult<bool>.Fail(OperationStatus.LoadingFailed);

            if (list.Any(x => x.Platform == newItem.Platform))
            {
                return OperationResult<bool>.Fail(OperationStatus.AlreadyExists);
            }

            list.Add(newItem);
            var result = await WriteEncryptedFileAsync(list);
            return result ? OperationResult<bool>.Success(true) : OperationResult<bool>.Fail(OperationStatus.StorageFailed);
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public async Task<OperationResult<bool>> UpdateItemAsync(AccountItem updated)
    {
        await Semaphore.WaitAsync();
        try
        {
            var (ok, listStore) = await LoadAccountsFromFileAsync();
            if (!ok) return new(OperationStatus.LoadingFailed, ok);

            var existing = listStore.FirstOrDefault(x => x.ID == updated.ID);
            if (existing == null)
            {
                return OperationResult<bool>.Fail(OperationStatus.NotFound);
            }

            listStore.Remove(existing);
            listStore.Add(updated);
            var result = await WriteEncryptedFileAsync(listStore);
            return result ? OperationResult<bool>.Success(true) : OperationResult<bool>.Fail(OperationStatus.StorageFailed);
        }
        finally
        {
            Semaphore.Release();
        }
    }


    public async Task<OperationResult<bool>> DeleteItemAsync(string platform)
    {
        await Semaphore.WaitAsync();
        try
        {
            var (success, secrets) = await LoadAccountsFromFileAsync();
            if (!success)
                return OperationResult<bool>.Fail(OperationStatus.LoadingFailed);

            var item = secrets.FirstOrDefault(x => x.Platform == platform);
            if (item is null)
            {
                return OperationResult<bool>.Fail(OperationStatus.NotFound);
            }

            secrets.Remove(item);

            var result = await WriteEncryptedFileAsync(secrets);
            return result ? OperationResult<bool>.Success(true) : OperationResult<bool>.Fail(OperationStatus.StorageFailed);
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public bool BackupAccountsFile()
    {
        if (!File.Exists(_secretsPath)) return false;

        var dir = Path.GetDirectoryName(_secretsPath)!;
        var file = Path.GetFileName(_secretsPath);

        for (var i = 5; i >= 1; i--)
        {
            var oldBackup = Path.Combine(dir, $"{file}.bak{i}");
            var nextBackup = Path.Combine(dir, $"{file}.bak{i + 1}");

            if (File.Exists(oldBackup))
            {
                if (i == 5) File.Delete(oldBackup);
                else File.Move(oldBackup, nextBackup, true);
            }
        }

        var firstBackup = Path.Combine(dir, $"{file}.bak1");
        File.Copy(_secretsPath, firstBackup, true);

        return true;

    }

    private async Task<(bool, List<AccountItem>)> LoadAccountsFromFileAsync()
    {
        try
        {
            EnsureStorageFileExists();

            var encrypted = await File.ReadAllBytesAsync(_secretsPath);
            if (encrypted.Length == 0)
                return (true, []);

            var decrypted = ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);
            var json = Encoding.UTF8.GetString(decrypted);

            var list = JsonSerializer.Deserialize<List<AccountItem>>(json, GetOptions()) ?? [];
            return (true, list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, nameof(LoadAccountsFromFileAsync));

            return (false, default!);
        }
    }

    /// <summary>
    /// If storage file does not exist it creates a new one
    /// </summary>
    private void EnsureStorageFileExists()
    {
        if (!File.Exists(_secretsPath))
        {
            // Ensure directory exists
            var directory = Path.GetDirectoryName(_secretsPath);

            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            // Create empty file (or write initial content)
            File.WriteAllText(_secretsPath, string.Empty);
        }
    }

    private async Task<bool> WriteEncryptedFileAsync(List<AccountItem> list)
    {
        try
        {
            var json = JsonSerializer.Serialize(list, GetOptions());
            var data = Encoding.UTF8.GetBytes(json);
            var encrypted = ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser);
            await File.WriteAllBytesAsync(_secretsPath, encrypted);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, nameof(WriteEncryptedFileAsync));
            return false;
        }
    }

    private JsonSerializerOptions GetOptions()
    {
        return _options ?? new JsonSerializerOptions { WriteIndented = true };
    }

    public void Dispose()
    {
        Semaphore.Dispose();
    }


}