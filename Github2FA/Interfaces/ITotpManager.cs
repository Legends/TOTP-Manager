 using Github2FA.Models;

namespace Github2FA.Interfaces
{
    public interface ITotpManager
    {
        /// <summary>
        /// Adds a new TOTP secret to the secrets.json file by prompting the user for a key and value.
        /// </summary>
        /// <returns>bool for success and the secretItem/null</returns>
        (bool success, SecretItem? item) PromptAndAddTotp();
        bool TryComputeCode(string secret, out string? code, out string? error);
      
        void UpdateSecret(SecretItem previous, SecretItem updated);

        /// <summary>
        /// Deletes a secret item from the secrets.json file.
        /// </summary>
        /// <param name="item">SecretItem</param>
        /// <returns>true/false</returns>
        bool DeleteSecret(SecretItem item);
    }
}
