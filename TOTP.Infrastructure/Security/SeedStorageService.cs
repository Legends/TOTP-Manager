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
        byte[] dek = securityContext.GetDekCopy();
        byte[] data = Encoding.UTF8.GetBytes(plainSeed);

        try
        {
            // 2. Setup NSec key and nonce
            using var key = Key.Import(_algorithm, dek, KeyBlobFormat.RawSymmetricKey);
            byte[] nonce = RandomNumberGenerator.GetBytes(_algorithm.NonceSize);

            // 3. Encrypt with GCM (Authenticated Encryption)
            byte[] encryptedData = _algorithm.Encrypt(key, nonce, null, data);

            return (encryptedData, nonce);
        }
        finally
        {
            Array.Clear(dek, 0, dek.Length);
            Array.Clear(data, 0, data.Length);
        }
    }

    /// <summary>
    /// Decrypts a TOTP seed back to plain text for code generation.
    /// </summary>
    public string DecryptSeed(byte[] encryptedSeed, byte[] nonce)
    {
        if (!securityContext.IsUnlocked)
            throw new InvalidOperationException("Vault is locked.");

        byte[] dek = securityContext.GetDekCopy();
        byte[]? decryptedData;
        try
        {
            using var key = Key.Import(_algorithm, dek, KeyBlobFormat.RawSymmetricKey);
            // Decrypt returns null if the authentication tag is invalid (tampering detection)
            decryptedData = _algorithm.Decrypt(key, nonce, null, encryptedSeed);
        }
        finally
        {
            Array.Clear(dek, 0, dek.Length);
        }

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
