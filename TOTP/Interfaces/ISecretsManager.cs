using System.Collections.Generic;
using TOTP.Models;

namespace TOTP.Interfaces
{
    public interface ISecretsManager
    {
        List<SecretItem> GetAllSecrets();
        bool AddNewItem(SecretItem item);
        bool UpdateItem(string previousPlatform, SecretItem updated);
        bool DeleteItem(string platform);
        bool BackupSecretsFile();
    }

}