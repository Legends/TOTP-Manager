using NSec.Cryptography;
using System;
using System.Security.Cryptography;
using TOTP.Core.Security.Interfaces;

namespace TOTP.Core.Security.Services;

public sealed class KeyWrappingService : IKeyWrappingService
{
    // AES-GCM is the industry standard for authenticated encryption.
    private static readonly AeadAlgorithm _algorithm = AeadAlgorithm.Aes256Gcm;

    /// <summary>
    /// Generates a high-entropy 256-bit Master Key (DEK).
    /// </summary>
    public byte[] GenerateRawDek()
    {
        return RandomNumberGenerator.GetBytes(32);
    }

    /// <summary>
    /// Wraps the DEK using a Password-Derived Key (KEK).
    /// </summary>
    public (byte[] WrappedDek, byte[] Nonce) WrapDek(byte[] rawDek, byte[] kek)
    {
        if (rawDek == null || rawDek.Length == 0) throw new ArgumentNullException(nameof(rawDek));
        if (kek == null || kek.Length == 0) throw new ArgumentNullException(nameof(kek));

        // NSec uses Key objects for hardware/library optimization
        using var key = Key.Import(_algorithm, kek, KeyBlobFormat.RawSymmetricKey);

        // Nonce must be unique for every single encryption operation (GCM requirement)
        byte[] nonce = RandomNumberGenerator.GetBytes(_algorithm.NonceSize);

        // Encrypt the DEK. GCM provides 'Authenticated Encryption', 
        // meaning it will detect if the file has been tampered with.
        byte[] wrappedDek = _algorithm.Encrypt(key, nonce, null, rawDek);

        return (wrappedDek, nonce);
    }

    /// <summary>
    /// Unwraps the DEK. Returns the raw key if successful, or throws if tampered/wrong password.
    /// </summary>
    public byte[] UnwrapDek(byte[] wrappedDek, byte[] kek, byte[] nonce)
    {
        if (wrappedDek == null || kek == null || nonce == null)
            throw new ArgumentNullException("Encryption metadata is missing.");

        try
        {
            using var key = Key.Import(_algorithm, kek, KeyBlobFormat.RawSymmetricKey);

            // Decrypt the DEK. NSec returns null if the Authentication Tag (MAC) doesn't match.
            byte[]? rawDek = _algorithm.Decrypt(key, nonce, null, wrappedDek);

            if (rawDek == null)
            {
                throw new CryptographicException("Decryption failed. Authentication tag mismatch or invalid key.");
            }

            return rawDek;
        }
        catch (Exception ex)
        {
            // Log this carefully in the calling service; don't leak key info in exceptions.
            throw new CryptographicException("Critical failure during DEK unwrapping.", ex);
        }
    }
}