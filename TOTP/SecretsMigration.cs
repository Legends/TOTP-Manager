using Github2FA.Interfaces;
using Github2FA.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace TOTP.Manager
{
    public static class SecretsMigration
    {
        public static void MigrateFromUserSecrets(string userSecretsId, ISecretsManager targetManager)
        {
            string userSecretsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Microsoft", "UserSecrets", userSecretsId, "secrets.json");

            if (!File.Exists(userSecretsPath))
            {
                Console.WriteLine($"❌ Secrets file not found at: {userSecretsPath}");
                return;
            }

            string json = File.ReadAllText(userSecretsPath);
            var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            if (dict == null)
            {
                Console.WriteLine("❌ Failed to deserialize secrets.");
                return;
            }

            foreach (var kv in dict)
            {
                var item = new SecretItem(kv.Key, kv.Value);
                targetManager.AddNewItem(item);
            }

            Console.WriteLine("✅ Migration complete.");
        }
    }

}
