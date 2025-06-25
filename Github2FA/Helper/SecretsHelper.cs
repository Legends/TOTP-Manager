using Github2FA.Interfaces;
using Github2FA.Models;
using Github2FA.Services;
using Microsoft.Extensions.Configuration;
using Syncfusion.ProjIO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Xml.Linq;

namespace Github2FA.Helper
{
    public class SecretsHelper : ISecretsHelper
    {
        string csprojPath;
        string userSecretsId;
        string secretsPath;
        IMessageService _messageService;

        public SecretsHelper(IMessageService svcMsg)
        {
            _messageService = svcMsg;
            csprojPath = @"E:\Repos\Github2FA\Github2FA\Github2FA.csproj";
            var csproj = XDocument.Load(csprojPath);
            userSecretsId = csproj.Descendants("UserSecretsId").FirstOrDefault()?.Value;
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

            if (prevKey == updated.Key)// just update the value
            {
                dict[prevKey] = updated.Value;
            }
            else // key has changed, we need to remove the old key and add the new one
            {
                if (dict.ContainsKey(prevKey))
                {
                    dict.Remove(prevKey);
                }
                dict[updated.Key] = updated.Value; // add or update the new key
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
            return (flowControl: true, dict: dict);
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
    }

    //public class SecretsHelper
    //{
    //    public static bool UpdateSecretsJsonFile(string key, string value )
    //    {
    //        string projectPath = AppDomain.CurrentDomain.BaseDirectory; // Get the current directory of the application
    //        Console.WriteLine($"projectPath: {projectPath}");
    //        // Step 1: Use the CLI to set the secret
    //        var process = new Process
    //        {
    //            StartInfo = new ProcessStartInfo
    //            {
    //                FileName = "dotnet",
    //                Arguments = $"user-secrets set \"{key}\" \"{value}\" --project \"{projectPath}\"",
    //                RedirectStandardOutput = true,
    //                RedirectStandardError = true,
    //                UseShellExecute = false,
    //                CreateNoWindow = true
    //            }
    //        };

    //        process.Start();
    //        process.WaitForExit();

    //        if (process.ExitCode != 0)
    //        {
    //            Debug.WriteLine("Failed to write secret: " + process.StandardError.ReadToEnd());
    //            return false;
    //        }

    //        // Step 2: Verify the secret by reloading it from the secrets store
    //        var config = new ConfigurationBuilder()
    //            .AddUserSecrets(Path.Combine(projectPath, $"{Path.GetFileName(projectPath)}.csproj"))
    //            .Build();

    //        string? retrieved = config[key]; // Allow nullable string
    //        return retrieved == value;
    //    }
    //}

}
