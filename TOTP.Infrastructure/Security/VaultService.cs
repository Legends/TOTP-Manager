using NSec.Cryptography;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using TOTP.Core.Models;
using TOTP.Core.Security.Interfaces;

namespace TOTP.Core.Security.Services;

public sealed class VaultService : IVaultService
{
    private readonly ISecurityContext _securityContext;
    private static readonly AeadAlgorithm Algorithm = AeadAlgorithm.Aes256Gcm;
    private static readonly byte[] FileHeader = "TVLT"u8.ToArray();

    public VaultService(ISecurityContext securityContext)
    {
        _securityContext = securityContext ?? throw new ArgumentNullException(nameof(securityContext));
    }

    public byte[] EncryptVault(List<OtpEntry> entries)
    {
        if (!_securityContext.IsUnlocked) throw new InvalidOperationException("Security context locked.");

        var json = JsonSerializer.Serialize(entries, new JsonSerializerOptions { WriteIndented = true });
        byte[] data = Encoding.UTF8.GetBytes(json);
        byte[] nonce = RandomNumberGenerator.GetBytes(Algorithm.NonceSize);

        using var key = Key.Import(Algorithm, _securityContext.GetDek(), KeyBlobFormat.RawSymmetricKey);
        byte[] encrypted = Algorithm.Encrypt(key, nonce, null, data);

        // Combine: Header + Nonce + Ciphertext
        byte[] result = new byte[FileHeader.Length + nonce.Length + encrypted.Length];
        Buffer.BlockCopy(FileHeader, 0, result, 0, FileHeader.Length);
        Buffer.BlockCopy(nonce, 0, result, FileHeader.Length, nonce.Length);
        Buffer.BlockCopy(encrypted, 0, result, FileHeader.Length + nonce.Length, encrypted.Length);

        return result;
    }

    public List<OtpEntry> DecryptVault(byte[] encryptedBlob)
    {
        if (!_securityContext.IsUnlocked) throw new InvalidOperationException("Security context locked.");

        int minSize = FileHeader.Length + Algorithm.NonceSize;
        if (encryptedBlob.Length < minSize) throw new CryptographicException("Data is too small/corrupted.");

        if (!encryptedBlob.Take(FileHeader.Length).SequenceEqual(FileHeader))
            throw new CryptographicException("Invalid file header.");

        byte[] nonce = encryptedBlob.Skip(FileHeader.Length).Take(Algorithm.NonceSize).ToArray();
        byte[] ciphertext = encryptedBlob.Skip(FileHeader.Length + Algorithm.NonceSize).ToArray();

        using var key = Key.Import(Algorithm, _securityContext.GetDek(), KeyBlobFormat.RawSymmetricKey);
        byte[]? decrypted = Algorithm.Decrypt(key, nonce, null, ciphertext);

        if (decrypted == null) throw new CryptographicException("Decryption failed (Wrong key or tampered data).");

        try
        {
            var json = Encoding.UTF8.GetString(decrypted);
            return JsonSerializer.Deserialize<List<OtpEntry>>(json) ?? [];
        }
        finally { Array.Clear(decrypted, 0, decrypted.Length); }
    }
}