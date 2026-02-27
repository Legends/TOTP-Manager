using NSec.Cryptography;
using System.Security.Cryptography;
using TOTP.Security.Interfaces;

namespace TOTP.Security;

public class KeyWrappingService: IKeyWrappingService
{
    private static readonly AeadAlgorithm _algorithm = AeadAlgorithm.Aes256Gcm;

    public byte[] GenerateRawDek()
    {
        return RandomNumberGenerator.GetBytes(32);
    }

    public (byte[] WrappedDek, byte[] Nonce) WrapDek(byte[] rawDek, byte[] kek)
    {
        using var key = Key.Import(_algorithm, kek, KeyBlobFormat.RawSymmetricKey);
        var nonce = RandomNumberGenerator.GetBytes(_algorithm.NonceSize);

        // Den DEK mit dem KEK verschlüsseln
        var wrappedDek = _algorithm.Encrypt(key, nonce, null, rawDek);

        return (wrappedDek, nonce);
    }

    public byte[] UnwrapDek(byte[] wrappedDek, byte[] kek, byte[] nonce)
    {
        using var key = Key.Import(_algorithm, kek, KeyBlobFormat.RawSymmetricKey);

        // Den DEK mit dem KEK entschlüsseln
        var rawDek = _algorithm.Decrypt(key, nonce, null, wrappedDek);

        return rawDek ?? throw new CryptographicException("Hauptschlüssel-Entschlüsselung fehlgeschlagen.");
    }
}