using System;
using System.Security.Cryptography;
using System.Text;
using NSec.Cryptography;
using TOTP.Core.Security.Models;

namespace TOTP.Security.Helpers;

public static class PasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 32;

    // Default settings for newly created passwords
    private static readonly Argon2Parameters _defaultParameters = new()
    {
        DegreeOfParallelism = 1,
        MemorySize = 128 * 1024, // 128 MB
        NumberOfPasses = 4
    };

    /// <summary>
    /// Hashes a password using default parameters and a new random salt.
    /// </summary>
    public static PasswordRecord Hash(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be empty.", nameof(password));

        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);

        // Initialize algorithm with default parameters
        var algorithm = PasswordBasedKeyDerivationAlgorithm.Argon2id(in _defaultParameters);

        // Use the string overload of DeriveBytes
        byte[] hash = algorithm.DeriveBytes(password, salt, HashSize);

        return new PasswordRecord(
            salt,
            hash,
            (int)_defaultParameters.NumberOfPasses,
            (int)_defaultParameters.MemorySize);
    }

    /// <summary>
    /// Verifies a password against a stored record by initializing Argon2id 
    /// with the record's specific parameters.
    /// </summary>
    public static bool Verify(string password, PasswordRecord storedRecord)
    {
        if (string.IsNullOrWhiteSpace(password) || storedRecord.Hash == null || storedRecord.Salt == null)
            return false;

        var parameters = new Argon2Parameters
        {
            DegreeOfParallelism = 1,
            MemorySize = storedRecord.MemorySize,
            NumberOfPasses = storedRecord.Iterations
        };

        // Create the algorithm instance specific to this record's cost factors
        var algorithm = PasswordBasedKeyDerivationAlgorithm.Argon2id(in parameters);

        byte[] actualHash = algorithm.DeriveBytes(password, storedRecord.Salt, storedRecord.Hash.Length);

        // Constant-time comparison to prevent timing attacks
        return CryptographicOperations.FixedTimeEquals(actualHash, storedRecord.Hash);
    }

    /// <summary>
    /// Derives a 256-bit Key (KEK) using stored parameters. 
    /// Used to unwrap the Data Encryption Key during login.
    /// </summary>
    public static byte[] HashWithParams(string password, PasswordRecord storedRecord)
    {
        var parameters = new Argon2Parameters
        {
            DegreeOfParallelism = 1,
            MemorySize = storedRecord.MemorySize,
            NumberOfPasses = storedRecord.Iterations
        };

        var algorithm = PasswordBasedKeyDerivationAlgorithm.Argon2id(in parameters);

        // Derive 32 bytes (256 bits) for the AES-GCM KEK
        return algorithm.DeriveBytes(password, storedRecord.Salt, 32);
    }
}