using Github2FA.Interfaces;
using Github2FA.Models;
using Microsoft.Extensions.Configuration;
using Syncfusion.ProjIO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Xml.Linq;

namespace Github2FA.Services;

public class SecretsManager : ISecretsManager
{
    string csprojPath;
    string userSecretsId;
    string secretsPath;
    IMessageService _messageService;

    public SecretsManager(IMessageService svcMsg)
    {
        _messageService = svcMsg;
        Init();
        BackupSecretsFile();
    }

    private void Init()
    {
        csprojPath = @"E:\Repos\Github2FA\TOTP\TOTP.Manager.csproj";
        var csprojXDocument = XDocument.Load(csprojPath);
        userSecretsId = csprojXDocument.Descendants("UserSecretsId").FirstOrDefault()?.Value ?? string.Empty;
    }

    public bool DeleteItemFromSecretsFile(string key)
    {
        if (!getSecretsPath())
            return false;

        var json = File.ReadAllText(secretsPath);
        var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new();

        if (dict.Remove(key))
        {
            var success = UpdateSecretsFile(dict);
            return success;
        }
        Debug.WriteLine($"Key '{key}' not found in secrets.");
        return false;

    }

    public bool AddNewItemToSecretsFile(string key, string value)
    {

        if (!getSecretsPath())
            return false;

        var json = File.Exists(secretsPath)
            ? File.ReadAllText(secretsPath)
            : "{}";

        var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new();

        // Set or update a secret
        dict[key] = value;

        var success = UpdateSecretsFile(dict);
        return success;

    }

    public bool UpdateItemInSecretsFile(string prevKey, SecretItem updated)
    {
        (bool hasRead, Dictionary<string, string> dict) = ReadSecretsFile();

        if (!hasRead)
        {
            return false;
        }

        if (prevKey == updated.Platform)// just update the value
        {
            dict[prevKey] = updated.Secret;
        }
        else // key has changed, we need to remove the old key and add the new one
        {
            if (dict.ContainsKey(prevKey))
            {
                dict.Remove(prevKey);
            }
            dict[updated.Platform] = updated.Secret; // add or update the new key
        }

        var success = UpdateSecretsFile(dict);
        return success;
    }

    private (bool flowControl, Dictionary<string, string> dict) ReadSecretsFile()
    {
        Dictionary<string, string>? dict = default;
        try
        {
            if (!getSecretsPath())
                return (flowControl: false, dict: default);

            var json = File.Exists(secretsPath)
                ? File.ReadAllText(secretsPath)
                : "{}";

            dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new();

        }
        catch (Exception ex)
        {
            _messageService.ShowMessageDialog($"Error reading secrets file: {ex.Message}", "Error");
            return (flowControl: false, dict: default);
        }
        return (flowControl: true, dict);
    }

    private bool UpdateSecretsFile(Dictionary<string, string> dict)
    {
        try
        {
            string newJson = JsonSerializer.Serialize(dict, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(secretsPath, newJson);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error updating secrets: {ex.Message}");
            _messageService.ShowMessageDialog($"Error updating secrets file: {ex.Message}", "Error");
            return false;
        }

        return true;
    }
    public bool BackupSecretsFile()
    {
        try
        {
            if (!getSecretsPath())
            {
                _messageService.ShowMessageDialog("Secrets file not found. Backup failed.", "Error");
                return false;
            }

            string dir = Path.GetDirectoryName(secretsPath) ?? "";
            string file = Path.GetFileName(secretsPath);

            // Rotate old backups
            for (int i = 5; i >= 1; i--)
            {
                string oldBackup = Path.Combine(dir, $"{file}.bak{i}");
                string nextBackup = Path.Combine(dir, $"{file}.bak{i + 1}");

                if (File.Exists(oldBackup))
                {
                    // Delete the oldest
                    if (i == 5 && File.Exists(oldBackup))
                        File.Delete(oldBackup);

                    // Move (rename) to next
                    if (i < 5)
                        File.Move(oldBackup, nextBackup, true);
                }
            }

            // Create newest backup
            string firstBackup = Path.Combine(dir, $"{file}.bak1");
            File.Copy(secretsPath, firstBackup, true);

            Debug.WriteLine($"Backup created at {firstBackup}");
            return true;

        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error creating backup: {ex.Message}");
            _messageService.ShowMessageDialog($"Error creating backup: {ex.Message}", "Error");
            return false;
        }
    }
    bool getSecretsPath()
    {
        if (string.IsNullOrWhiteSpace(userSecretsId))
        {
            Debug.WriteLine("No UserSecretsId found in .csproj.");
            return false;
        }

        secretsPath = Path.Combine(
           Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
           "Microsoft", "UserSecrets", userSecretsId, "secrets.json"
       );

        if (!File.Exists(secretsPath))
        {
            Debug.WriteLine($"Secrets file not found at {secretsPath}.");
            return false;
        }
        return true;
    }
}
