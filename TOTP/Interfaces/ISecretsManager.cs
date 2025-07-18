using Github2FA.Models;

namespace Github2FA.Interfaces
{
    public interface ISecretsManager
    {
        bool AddNewItemToSecretsFile(string key, string value);
        bool DeleteItemFromSecretsFile(string key);
        bool UpdateItemInSecretsFile(string prevKey, SecretItem updated);
        bool BackupSecretsFile();
    }
}