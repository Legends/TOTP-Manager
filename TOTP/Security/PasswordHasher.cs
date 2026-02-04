using System;
using System.Security.Cryptography;

namespace TOTP.Security;

public static class PasswordHasher
{
    private const int SaltSize = 16;        // 128-bit
    private const int HashSize = 32;        // 256-bit
    private const int Iterations = 200_000; // adjust if needed

    public static (byte[] salt, byte[] hash) Hash(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password is empty.", nameof(password));

        var salt = RandomNumberGenerator.GetBytes(SaltSize);

        using var pbkdf2 = new Rfc2898DeriveBytes(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256);

        var hash = pbkdf2.GetBytes(HashSize);
        return (salt, hash);
    }

    public static bool Verify(string password, byte[] salt, byte[] expectedHash)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256);

        var actual = pbkdf2.GetBytes(expectedHash.Length);
        return CryptographicOperations.FixedTimeEquals(actual, expectedHash);
    }
}