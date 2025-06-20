namespace Github2FA.Interfaces
{
    public interface ISecretsHelper
    {
        bool AddNewItemToSecretsFile(string key, string value);
        bool DeleteItemFromSecretsFile(string key);
    }
}