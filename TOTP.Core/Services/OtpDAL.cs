using FluentResults;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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

public class OtpDAL(ILogger<OtpDAL> logger, string? storageFilePath) : IOtpDAL, IDisposable
{
    private readonly JsonSerializerOptions? _options = null;
    private readonly string _secretsPath = storageFilePath ?? AlternativeStoragePath();
    private static readonly SemaphoreSlim Semaphore = new(1, 1);
    private readonly ILogger<OtpDAL> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    private static string AlternativeStoragePath()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "TOTP-Manager", "secrets.dat");
    }

    //Task<OperationResult<List<AccountItem>>> GetAllAccountsAsync
    public async Task<Result<List<OtpEntry>>> GetAllAsync()
    {
        await Semaphore.WaitAsync();
        try
        {
            var list = await LoadAccountsFromFileAsync();
            return Result.Ok(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, nameof(GetAllAsync));
            return new StatusError(OperationStatus.LoadingFailed);
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public async Task<Result> AddNewAsync(OtpEntry newItem)
    {
        await Semaphore.WaitAsync();
        try
        {
            var list = await LoadAccountsFromFileAsync();

            if (list.Any(x => x.Issuer == newItem.Issuer))
                return new StatusError(OperationStatus.AlreadyExists);

            list.Add(newItem);

            await WriteEncryptedFileAsync(list);
            return Result.Ok();

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, nameof(AddNewAsync));
            return new StatusError(OperationStatus.StorageFailed);
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public async Task<Result> UpdateAsync(OtpEntry updated)
    {
        await Semaphore.WaitAsync();
        try
        {
            var listStore = await LoadAccountsFromFileAsync();


            var existing = listStore.FirstOrDefault(x => x.ID == updated.ID);
            if (existing == null)
                return new StatusError(OperationStatus.NotFound, $"{updated.Issuer} not found");

            listStore.Remove(existing);
            listStore.Add(updated);

            await WriteEncryptedFileAsync(listStore);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, nameof(UpdateAsync));
            return new StatusError(OperationStatus.StorageFailed);
        }
        finally
        {
            Semaphore.Release();
        }
    }


    public async Task<Result> DeleteAccountAsync(OtpEntry account)
    {
        await Semaphore.WaitAsync();
        try
        {
            var secrets = await LoadAccountsFromFileAsync();

            var item = secrets.FirstOrDefault(x => x.ID == account.ID);

            if (item is null)
                return new StatusError(OperationStatus.NotFound);

            secrets.Remove(item);

            await WriteEncryptedFileAsync(secrets);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error deleting item for platform {Platform}", account);
            return new StatusError(OperationStatus.DeleteFailed);
        }
        finally
        {
            Semaphore.Release();
        }
    }

    private async Task<List<OtpEntry>> LoadAccountsFromFileAsync()
    {
        EnsureStorageFileExists();

        var encrypted = await File.ReadAllBytesAsync(_secretsPath);
        if (encrypted.Length == 0)
            return [];

        var decrypted = ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);
        var json = Encoding.UTF8.GetString(decrypted);

        var list = JsonSerializer.Deserialize<List<OtpEntry>>(json, GetOptions()) ?? [];
        return list;
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

    private async Task WriteEncryptedFileAsync(List<OtpEntry> list)
    {

        var json = JsonSerializer.Serialize(list, GetOptions());
        var data = Encoding.UTF8.GetBytes(json);
        var encrypted = ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser);
        await File.WriteAllBytesAsync(_secretsPath, encrypted);
    }

    public async Task<Result> BackupOtpEntriesStorageFileAsync() 
    {
       return await Task.Run(() =>
        {
            try
            {
                if (!File.Exists(_secretsPath))
                    return Result.Fail(new StatusError(OperationStatus.StorageFailed)
                        .WithMetadata("Context", nameof(BackupOtpEntriesStorageFileAsync))
                        .WithMetadata("Reason", $"Path: {_secretsPath} does not exist"));

                var dir = Path.GetDirectoryName(_secretsPath)!;
                var file = Path.GetFileName(_secretsPath);

                // Shifting existing backups
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

                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, nameof(BackupOtpEntriesStorageFileAsync));
                return Result.Fail(new StatusError(OperationStatus.StorageFailed, "BackupAccountsStorageFile"));
            }
        });
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