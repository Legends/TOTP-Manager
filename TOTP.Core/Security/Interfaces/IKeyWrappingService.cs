namespace TOTP.Core.Security.Interfaces;

public interface IKeyWrappingService
{
    /// <summary>
    /// Generates a random 256-bit Data Encryption Key (DEK).
    /// </summary>
    byte[] GenerateRawDek();

    /// <summary>
    /// Encrypts the DEK using a Key Encryption Key (KEK).
    /// </summary>
    (byte[] WrappedDek, byte[] Nonce) WrapDek(byte[] rawDek, byte[] kek);

    /// <summary>
    /// Decrypts the DEK using a Key Encryption Key (KEK).
    /// </summary>
    byte[] UnwrapDek(byte[] wrappedDek, byte[] kek, byte[] nonce);
}