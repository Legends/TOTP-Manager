using OtpNet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using TOTP.Enums;
using TOTP.Interfaces;
using TOTP.Models;

namespace TOTP.Services;

public class SecretsManager : ISecretsManager
{
    private readonly IMessageService _messageService;
    private readonly JsonSerializerOptions? _options = null;
    private readonly string _secretsPath;

    public SecretsManager(IMessageService messageService, string? storageFilePath)
    {
        _messageService = messageService;
        _secretsPath = storageFilePath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "TOTP-Manager", "secrets.dat");

        BackupSecretsFile();
    }

    public List<SecretItem> GetAllSecrets()
    {
        var (ok, list) = ReadSecretsFile();
        return ok ? list : [];
    }

    public bool AddNewItem(SecretItem newItem)
    {
        var (ok, list) = ReadSecretsFile();
        if (!ok) return false;

        // Don't allow duplicates by platform
        if (list.Any(x => x.Platform == newItem.Platform))
        {
            _messageService.ShowMessage($"Platform '{newItem.Platform}' already exists.", CaptionType.Error, "pack://application:,,,/TOTP;component/Assets/Icons/Wrong.png");
            return false;
        }

        list.Add(newItem);
        return WriteEncryptedFile(list);
    }

    public bool DeleteItem(string platform)
    {
        var (ok, list) = ReadSecretsFile();
        if (!ok) return false;

        var existing = list.FirstOrDefault(x => x.Platform == platform);
        if (existing == null)
        {
            Debug.WriteLine($"Platform '{platform}' not found.");
            return false;
        }

        list.Remove(existing);
        return WriteEncryptedFile(list);
    }

    public bool UpdateItem(string previousPlatform, SecretItem updated)
    {
        var (ok, list) = ReadSecretsFile();
        if (!ok) return false;

        var existing = list.FirstOrDefault(x => x.Platform == previousPlatform);
        if (existing == null)
        {
            _messageService.ShowMessage("Platform not found.", CaptionType.Error, "pack://application:,,,/TOTP;component/Assets/Icons/Wrong.png");
            return false;
        }

        list.Remove(existing);
        list.Add(updated);
        return WriteEncryptedFile(list);
    }

    public bool BackupSecretsFile()
    {
        try
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

            Debug.WriteLine($"Backup created: {firstBackup}");
            return true;
        }
        catch (Exception ex)
        {
            _messageService.ShowMessage($"Backup failed: {ex.Message}", CaptionType.Error, "pack://application:,,,/TOTP;component/Assets/Icons/Wrong.png");
            return false;
        }
    }

    /// <summary>
    /// Returns a tuple indicating success and the list of secrets read from the encrypted secrets.dat file
    /// </summary>
    /// <returns></returns>
    private (bool, List<SecretItem>) ReadSecretsFile()
    {
        try
        {
            if (!File.Exists(_secretsPath))
                return (true, []);

            var encrypted = File.ReadAllBytes(_secretsPath);
            var decrypted = ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);
            var json = Encoding.UTF8.GetString(decrypted);

            var list = JsonSerializer.Deserialize<List<SecretItem>>(json) ?? [];
            return (true, list);
        }
        catch (Exception ex)
        {
            _messageService.ShowMessage($"Failed to read secrets: {ex.Message}", CaptionType.Error, "pack://application:,,,/TOTP;component/Assets/Icons/Wrong.png");
            return (false, default!);
        }
    }

    private bool WriteEncryptedFile(List<SecretItem> list)
    {
        try
        {
            var json = JsonSerializer.Serialize(list, GetOptions());
            var data = Encoding.UTF8.GetBytes(json);
            var encrypted = ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser);
            File.WriteAllBytes(_secretsPath, encrypted);
            return true;
        }
        catch (Exception ex)
        {
            _messageService.ShowMessage($"Failed to save secrets: {ex.Message}", CaptionType.Error, "pack://application:,,,/TOTP;component/Assets/Icons/Wrong.png");
            return false;
        }
    }

    private JsonSerializerOptions GetOptions()
    {
        return _options ?? new JsonSerializerOptions { WriteIndented = true };
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
}