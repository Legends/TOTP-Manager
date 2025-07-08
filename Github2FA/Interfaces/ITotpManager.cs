using Github2FA.Models;

namespace Github2FA.Interfaces
{
    public interface ITotpManager
    {
        (bool success, SecretItem? item) PromptAndAddTotp();
        void UpdateSecret(SecretItem previous, SecretItem updated);
        bool DeleteSecret(SecretItem item);
    }
}
