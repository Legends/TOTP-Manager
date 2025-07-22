using Github2FA.Models;
using System.Collections.Generic;

namespace Github2FA.Interfaces
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