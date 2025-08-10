using Microsoft.Extensions.Logging;
using OtpNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TOTP.Core;
using TOTP.Enums;
using TOTP.Interfaces;
using TOTP.Models;
using TOTP.Resources;

public class SecretsManager : ISecretsManager, IDisposable
{
    private readonly JsonSerializerOptions? _options = null;
    private readonly string _secretsPath;
    private static readonly SemaphoreSlim _semaphore = new(1, 1);
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

    public async Task<OperationResult<List<SecretItem>>> GetAllSecretsAsync()
    {

        await _semaphore.WaitAsync();
        try
        {
            var (success, list) = await LoadSecretsFromFileAsync();
            return new OperationResult<List<SecretItem>>((success ? OperationStatus.Success : OperationStatus.Unknown), success ? list : []);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<OperationResult<bool>> AddNewItemAsync(SecretItem newItem)
    {
        await _semaphore.WaitAsync();
        try
        {
            var (success, list) = await LoadSecretsFromFileAsync();
            if (!success) return new OperationResult<bool>(OperationStatus.LoadingFailed, false);

            if (list.Any(x => x.Platform == newItem.Platform))
            {

                return new OperationResult<bool>(OperationStatus.AlreadyExists, false); ;
            }

            //BackupSecretsFile();
            list.Add(newItem);
            var result = await WriteEncryptedFileAsync(list);
            return new OperationResult<bool>(result ? OperationStatus.Success : OperationStatus.StorageFailed, result);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<OperationResult<bool>> DeleteItemAsync(string platform)
    {
        await _semaphore.WaitAsync();
        try
        {
            var (success, secrets) = await LoadSecretsFromFileAsync();
            if (!success)
                return new(OperationStatus.LoadingFailed, false);

            var item = secrets.FirstOrDefault(x => x.Platform == platform);
            if (item is null)
            {
                return new(OperationStatus.NotFound, false);
            }

            //BackupSecretsFile();
            secrets.Remove(item);

            var result = await WriteEncryptedFileAsync(secrets);
            return new OperationResult<bool>(result ? OperationStatus.Success : OperationStatus.StorageFailed, result);
        }
        finally
        {
            _semaphore.Release();
        }
    }


    public async Task<OperationResult<bool>> UpdateItemAsync(string previousPlatform, SecretItem updated)
    {
        await _semaphore.WaitAsync();
        try
        {
            var (ok, list) = await LoadSecretsFromFileAsync();
            if (!ok) return new(OperationStatus.LoadingFailed, ok);

            var existing = list.FirstOrDefault(x => x.Platform == previousPlatform);
            if (existing == null)
            {
                return new(OperationStatus.NotFound, false);
            }

            //BackupSecretsFile();
            list.Remove(existing);
            list.Add(updated);
            var result = await WriteEncryptedFileAsync(list);
            return new OperationResult<bool>(result ? OperationStatus.Success : OperationStatus.StorageFailed, result);
        }
        finally
        {
            _semaphore.Release();
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

        //Debug.WriteLine($"Backup created: {firstBackup}");
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
            //_messageService.ShowMessage(string.Format(UI.msg_FailedReadingSecrets, ex.Message), CaptionType.Error, StringsConstants.ImgError);
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
            //_messageService.ShowMessage($"Failed to save secrets: {ex.Message}", CaptionType.Error, StringsConstants.ImgError);
            return false;
        }
    }

    private JsonSerializerOptions GetOptions()
    {
        return _options ?? new JsonSerializerOptions { WriteIndented = true };
    }

    public static (bool IsValid, string? ErrorMessage) IsValidSecretItem(SecretItem item)
    {
        if (string.IsNullOrWhiteSpace(item.Platform) || string.IsNullOrWhiteSpace(item.Secret))
            return (false, UI.msg_PlatformSecretNotEmpty);

        if (!IsValidBase32Format(item.Secret))
            return (false, UI.msg_SecretInvalidFormat);

        return (true, null);
    }

    public static bool IsValidBase32Format(string value)
    {
        try
        {
            _ = Base32Encoding.ToBytes(value);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void Dispose()
    {
        _semaphore.Dispose();
    }


}