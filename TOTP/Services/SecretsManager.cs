using OtpNet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TOTP.Enums;
using TOTP.Helper;
using TOTP.Interfaces;
using TOTP.Models;
using TOTP.Resources;

public class SecretsManager : ISecretsManager, IDisposable
{
    private readonly IMessageService _messageService;
    private readonly JsonSerializerOptions? _options = null;
    private readonly string _secretsPath;
    private static readonly SemaphoreSlim _semaphore = new(1, 1);

    public SecretsManager(IMessageService messageService, string? storageFilePath)
    {
        _messageService = messageService;
        _secretsPath = storageFilePath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "TOTP-Manager", "secrets.dat");
    }




    public async Task<List<SecretItem>> GetAllSecretsAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            var (ok, list) = await LoadSecretsFromFileAsync();
            return ok ? list : [];
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<bool> AddNewItemAsync(SecretItem newItem)
    {
        await _semaphore.WaitAsync();
        try
        {
            var (ok, list) = await LoadSecretsFromFileAsync();
            if (!ok) return false;

            if (list.Any(x => x.Platform == newItem.Platform))
            {
                _messageService.ShowMessage(string.Format(UI.msg_Platform_Exists, newItem.Platform), CaptionType.Error, StringsConstants.ImgError);
                return false;
            }

            BackupSecretsFile();
            list.Add(newItem);
            return await WriteEncryptedFileAsync(list);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<bool> DeleteItemAsync(string platform)
    {
        await _semaphore.WaitAsync();
        try
        {
            var (ok, list) = await LoadSecretsFromFileAsync();
            if (!ok) return false;

            var existing = list.FirstOrDefault(x => x.Platform == platform);
            if (existing == null)
            {
                Debug.WriteLine(string.Format(UI.msg_PlatformNotFound_0, platform));
                return false;
            }

            BackupSecretsFile();
            list.Remove(existing);
            return await WriteEncryptedFileAsync(list);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<bool> UpdateItemAsync(string previousPlatform, SecretItem updated)
    {
        await _semaphore.WaitAsync();
        try
        {
            var (ok, list) = await LoadSecretsFromFileAsync();
            if (!ok) return false;

            var existing = list.FirstOrDefault(x => x.Platform == previousPlatform);
            if (existing == null)
            {
                _messageService.ShowMessage(UI.msg_Platform_Not_Found, CaptionType.Error, StringsConstants.ImgError);
                return false;
            }

            BackupSecretsFile();
            list.Remove(existing);
            list.Add(updated);
            return await WriteEncryptedFileAsync(list);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public bool BackupSecretsFile()
    {
        if (!File.Exists(_secretsPath)) return false;

        try
        {
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

            Debug.WriteLine($"Backup created: {firstBackup}");
            return true;
        }
        catch (Exception ex)
        {
            _messageService.ShowMessage(string.Format(UI.ex_BackupFailed, ex.Message), CaptionType.Error, StringsConstants.ImgError);
            return false;
        }
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
            _messageService.ShowMessage(string.Format(UI.msg_FailedReadingSecrets, ex.Message), CaptionType.Error, StringsConstants.ImgError);
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
            _messageService.ShowMessage($"Failed to save secrets: {ex.Message}", CaptionType.Error, StringsConstants.ImgError);
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