using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using TOTP.Core.Common;
using TOTP.Core.Enums;
using TOTP.Core.Models;
using TOTP.Core.Validation;
using TOTP.Interfaces;


namespace TOTP.Core.Services;

public class SecretsManager : ISecretsManager, IDisposable
{
    private readonly JsonSerializerOptions? _options = null;
    private readonly string _secretsPath;
    private static readonly SemaphoreSlim Semaphore = new(1, 1);
    private readonly ILogger<SecretsManager> _logger;

    public SecretsManager(ILogger<SecretsManager> logger, string? storageFilePath)
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

    public async Task<Result<List<SecretItem>>> GetAllSecretsAsync()
    {
        await Semaphore.WaitAsync();
        try
        {
            var (success, list) = await LoadSecretsFromFileAsync();
            return success ?
                Result<List<SecretItem>>.Success(list)
                : Result<List<SecretItem>>.Fail(OperationStatus.LoadingFailed);
        }
        finally
        {
            Semaphore.Release();
        }
    }


    public async Task<Result<SecretItem>> GetSecretByPlatformAsync(string platform)
    {

        await Semaphore.WaitAsync();
        try
        {
            var (success, list) = await LoadSecretsFromFileAsync();
            var secret = list.Where(s => s.Platform.ToLowerInvariant() == platform.ToLowerInvariant()).FirstOrDefault();
            return success ?
                Result<SecretItem>.Success(secret)
                : Result<SecretItem>.Fail(OperationStatus.LoadingFailed);
        }
        finally
        {
            Semaphore.Release();
        }
    }


    public async Task<Result<bool>> AddNewItemAsync(SecretItem newItem)
    {
        await Semaphore.WaitAsync();
        try
        {
            var (success, list) = await LoadSecretsFromFileAsync();
            if (!success) return Result<bool>.Fail(OperationStatus.LoadingFailed);

            if (list.Any(x => x.Platform == newItem.Platform))
            {
                return Result<bool>.Fail(OperationStatus.AlreadyExists);
            }

            list.Add(newItem);
            var result = await WriteEncryptedFileAsync(list);
            return result ? Result<bool>.Success(true) : Result<bool>.Fail(OperationStatus.StorageFailed);
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public async Task<Result<bool>> UpdateItemAsync(SecretItem previous, SecretItem updated)
    {

        if (previous.ID != updated.ID)
            return Result<bool>.Fail(OperationStatus.ItemIdMismatch);

        await Semaphore.WaitAsync();
        try
        {
            var (ok, listStore) = await LoadSecretsFromFileAsync();
            if (!ok) return new(OperationStatus.LoadingFailed, ok);

            var existing = listStore.FirstOrDefault(x => x.Platform == previous.Platform);
            if (existing == null)
            {
                return Result<bool>.Fail(OperationStatus.NotFound);
            }

            listStore.Remove(existing);
            listStore.Add(updated);
            var result = await WriteEncryptedFileAsync(listStore);
            return result ? Result<bool>.Success(true) : Result<bool>.Fail(OperationStatus.StorageFailed);
        }
        finally
        {
            Semaphore.Release();
        }
    }


    public async Task<Result<bool>> DeleteItemAsync(string platform)
    {
        await Semaphore.WaitAsync();
        try
        {
            var (success, secrets) = await LoadSecretsFromFileAsync();
            if (!success)
                return Result<bool>.Fail(OperationStatus.LoadingFailed);

            var item = secrets.FirstOrDefault(x => x.Platform == platform);
            if (item is null)
            {
                return Result<bool>.Fail(OperationStatus.NotFound);
            }

            secrets.Remove(item);

            var result = await WriteEncryptedFileAsync(secrets);
            return result ? Result<bool>.Success(true) : Result<bool>.Fail(OperationStatus.StorageFailed);
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public bool BackupSecretsFile()
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

    private async Task<(bool, List<SecretItem>)> LoadSecretsFromFileAsync()
    {
        try
        {
            if (!File.Exists(_secretsPath))
                return (true, []);

            var encrypted = await File.ReadAllBytesAsync(_secretsPath);
            var decrypted = ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);
            var json = Encoding.UTF8.GetString(decrypted);

            var list = JsonSerializer.Deserialize<List<SecretItem>>(json, GetOptions()) ?? [];
            return (true, list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, nameof(LoadSecretsFromFileAsync));

            return (false, default!);
        }
    }

    private async Task<bool> WriteEncryptedFileAsync(List<SecretItem> list)
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

    public static (bool IsValid, ValidationError error) IsValidSecretItem(SecretItem item)
    {
        var result = SecretValidator.ValidatePlatform(item.Platform);
        if (result != ValidationError.None)
        {
            return (false, result);
        }


        result = SecretValidator.ValidateSecret(item.Secret);
        if (result != ValidationError.None)
        {
            return (false, result);
        }

        return (true, ValidationError.None);
    }


    public void Dispose()
    {
        Semaphore.Dispose();
    }


}