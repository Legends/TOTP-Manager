using System;
using System.Security.Cryptography;
using NSec.Cryptography;
using TOTP.Security.Models;

namespace TOTP.Security.Models;

public static class PasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 32;

    // Wir nutzen dieselben Parameter wie im ExportService für Konsistenz
    private static readonly Argon2Parameters _parameters = new()
    {
        DegreeOfParallelism = 1,
        MemorySize = 128 * 1024, // 128 MB
        NumberOfPasses = 4
    };

    private static readonly Argon2id _algorithm = PasswordBasedKeyDerivationAlgorithm.Argon2id(in _parameters);

    public static PasswordRecord Hash(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password is empty.", nameof(password));

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var passwordBytes = System.Text.Encoding.UTF8.GetBytes(password);

        try
        {
            // hash pwd
            var hash = _algorithm.DeriveBytes(passwordBytes, salt, HashSize);
           
            return new PasswordRecord(
                salt,
                hash,
                (int)_parameters.NumberOfPasses,
                (int)_parameters.MemorySize);
        }
        finally
        {
            Array.Clear(passwordBytes, 0, passwordBytes.Length);
        }
    }
    public static bool Verify(string password, byte[] salt, byte[] expectedHash)
    {
        if (string.IsNullOrWhiteSpace(password)) return false;

        var passwordBytes = System.Text.Encoding.UTF8.GetBytes(password);
        try
        {
            var actualHash = _algorithm.DeriveBytes(passwordBytes, salt, expectedHash.Length);

            // use FixedTimeEquals to prevent timing attacks
            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }
        finally
        {
            Array.Clear(passwordBytes, 0, passwordBytes.Length);
        }
    }
}