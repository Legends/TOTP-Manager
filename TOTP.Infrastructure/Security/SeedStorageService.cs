using NSec.Cryptography;
using System;
using System.Security.Cryptography;
using System.Text;
using TOTP.Core.Security.Interfaces;

namespace TOTP.Infrastructure.Security;

public sealed class SeedStorageService(
    ISecurityContext securityContext) : ISeedStorageService
{
    private static readonly AeadAlgorithm _algorithm = AeadAlgorithm.Aes256Gcm;

    /// <summary>
    /// Encrypts a TOTP seed (e.g., "JBSWY3DPEHPK3PXP") using the DEK from RAM.
    /// </summary>
    public (byte[] EncryptedSeed, byte[] Nonce) EncryptSeed(string plainSeed)
    {
        if (!securityContext.IsUnlocked)
            throw new InvalidOperationException("Vault is locked. Decryption key not available.");

        // 1. Get the DEK from SecurityContext
        byte[] dek = securityContext.GetDek();

        try
        {
            // 2. Setup NSec key and nonce
            using var key = Key.Import(_algorithm, dek, KeyBlobFormat.RawSymmetricKey);
            byte[] nonce = RandomNumberGenerator.GetBytes(_algorithm.NonceSize);
            byte[] data = Encoding.UTF8.GetBytes(plainSeed);

            // 3. Encrypt with GCM (Authenticated Encryption)
            byte[] encryptedData = _algorithm.Encrypt(key, nonce, null, data);

            return (encryptedData, nonce);
        }
        finally
        {
            // The SecurityContext returns a copy or access to the key; 
            // ensure we don't leave extra copies in this method's scope if unnecessary.
        }
    }

    /// <summary>
    /// Decrypts a TOTP seed back to plain text for code generation.
    /// </summary>
    public string DecryptSeed(byte[] encryptedSeed, byte[] nonce)
    {
        if (!securityContext.IsUnlocked)
            throw new InvalidOperationException("Vault is locked.");

        byte[] dek = securityContext.GetDek();

        using var key = Key.Import(_algorithm, dek, KeyBlobFormat.RawSymmetricKey);

        // Decrypt returns null if the authentication tag is invalid (tampering detection)
        byte[]? decryptedData = _algorithm.Decrypt(key, nonce, null, encryptedSeed);

        if (decryptedData == null)
            throw new CryptographicException("Failed to decrypt seed. Data may be corrupted or key is invalid.");

        try
        {
            return Encoding.UTF8.GetString(decryptedData);
        }
        finally
        {
            // Wipe the temporary decrypted byte array from memory
            Array.Clear(decryptedData, 0, decryptedData.Length);
        }
    }
}
