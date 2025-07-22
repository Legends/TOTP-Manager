using Github2FA.Interfaces;
using Github2FA.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Github2FA.Services;

public class SecretsManager : ISecretsManager
{
    private readonly string secretsPath;
    private readonly IMessageService _messageService;

    public SecretsManager(IMessageService messageService)
    {
        _messageService = messageService;

        string appDataDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "TOTP-Manager");

        Directory.CreateDirectory(appDataDir);
        secretsPath = Path.Combine(appDataDir, "secrets.dat");

        BackupSecretsFile();
    }

    public List<SecretItem> GetAllSecrets()
    {
        var (ok, list) = ReadSecretsFile();
        return ok ? list : new List<SecretItem>();
    }

    public bool AddNewItem(SecretItem newItem)
    {
        var (ok, list) = ReadSecretsFile();
        if (!ok) return false;

        // Don't allow duplicates by platform
        if (list.Any(x => x.Platform == newItem.Platform))
        {
            _messageService.ShowMessageDialog($"Platform '{newItem.Platform}' already exists.", "Error");
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
            _messageService.ShowMessageDialog("Platform not found.", "Error");
            return false;
        }

        list.Remove(existing);
        list.Add(updated);
        return WriteEncryptedFile(list);
    }

    private (bool, List<SecretItem>) ReadSecretsFile()
    {
        try
        {
            if (!File.Exists(secretsPath))
                return (true, new List<SecretItem>());

            byte[] encrypted = File.ReadAllBytes(secretsPath);
            byte[] decrypted = ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);
            string json = Encoding.UTF8.GetString(decrypted);

            var list = JsonSerializer.Deserialize<List<SecretItem>>(json) ?? new();
            return (true, list);
        }
        catch (Exception ex)
        {
            _messageService.ShowMessageDialog($"Failed to read secrets: {ex.Message}", "Error");
            return (false, default!);
        }
    }

    private bool WriteEncryptedFile(List<SecretItem> list)
    {
        try
        {
            string json = JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true });
            byte[] data = Encoding.UTF8.GetBytes(json);
            byte[] encrypted = ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser);
            File.WriteAllBytes(secretsPath, encrypted);
            return true;
        }
        catch (Exception ex)
        {
            _messageService.ShowMessageDialog($"Failed to save secrets: {ex.Message}", "Error");
            return false;
        }
    }

    public bool BackupSecretsFile()
    {
        try
        {
            if (!File.Exists(secretsPath)) return false;

            string dir = Path.GetDirectoryName(secretsPath)!;
            string file = Path.GetFileName(secretsPath);

            for (int i = 5; i >= 1; i--)
            {
                string oldBackup = Path.Combine(dir, $"{file}.bak{i}");
                string nextBackup = Path.Combine(dir, $"{file}.bak{i + 1}");

                if (File.Exists(oldBackup))
                {
                    if (i == 5) File.Delete(oldBackup);
                    else File.Move(oldBackup, nextBackup, true);
                }
            }

            string firstBackup = Path.Combine(dir, $"{file}.bak1");
            File.Copy(secretsPath, firstBackup, true);

            Debug.WriteLine($"Backup created: {firstBackup}");
            return true;
        }
        catch (Exception ex)
        {
            _messageService.ShowMessageDialog($"Backup failed: {ex.Message}", "Error");
            return false;
        }
    }
}
