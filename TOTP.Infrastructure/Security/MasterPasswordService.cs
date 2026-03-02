using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NSec.Cryptography;
using TOTP.Security.Interfaces;

namespace TOTP.Infrastructure.Security;

/// <summary>
/// Corrected MasterPasswordService for NSec unit constraints.
/// NSec uses Kibibytes (KiB) for MemorySize, not Bytes.
/// </summary>
public sealed class MasterPasswordService : IMasterPasswordService
{
    private readonly ILogger<MasterPasswordService> _logger;

    // 2026 Enterprise Security Standards
    private const int DefaultPasses = 3;

    // NSec EXPECTS KiB. 64 * 1024 KiB = 64 MB.
    // 64 MB is the libsodium "Moderate" limit.
    private const int DefaultMemorySizeKiB = 64 * 1024;

    // Using 1 for parallelism is the most stable setting for libsodium wrappers
    private const int DefaultParallelism = 1;

    private const int SaltSize = 16;
    private const int NonceSize = 12;

    public MasterPasswordService(ILogger<MasterPasswordService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<(byte[] WrappedDek, byte[] Salt, int Iterations, int MemorySize, byte[] Nonce)>
        WrapKeyAsync(byte[] rawDek, string password)
    {
        return await Task.Run(() =>
        {
            try
            {
                ValidateInputs(rawDek, password);

                _logger.LogDebug("Deriving KEK: Passes={Passes}, Mem={Mem}KiB, Parallelism={P}",
                    DefaultPasses, DefaultMemorySizeKiB, DefaultParallelism);

                byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
                byte[] nonce = RandomNumberGenerator.GetBytes(NonceSize);

                // 1. Setup Argon2id Parameters
                var argonParams = new Argon2Parameters
                {
                    NumberOfPasses = DefaultPasses,
                    MemorySize = DefaultMemorySizeKiB, // Verified: This must be KiB
                    DegreeOfParallelism = DefaultParallelism
                };

                // 2. Initialize Algorithm
                // This is where it was crashing due to the 64GB request
                var argon2 = PasswordBasedKeyDerivationAlgorithm.Argon2id(argonParams);
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

                // 3. Derive KEK specifically for Aes256Gcm
                using var kek = argon2.DeriveKey(passwordBytes, salt, AeadAlgorithm.Aes256Gcm);

                // 4. Wrap DEK
                var aes = AeadAlgorithm.Aes256Gcm;
                byte[] wrappedDek = aes.Encrypt(kek, nonce, null, rawDek);

                _logger.LogInformation("Key wrapping completed successfully.");

                return (wrappedDek, salt, DefaultPasses, DefaultMemorySizeKiB, nonce);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Argon2 Critical Failure. Parameters: Passes={P}, Mem={M}KiB, Par={Par}",
                    DefaultPasses, DefaultMemorySizeKiB, DefaultParallelism);
                throw;
            }
        });
    }

    public async Task<byte[]?> UnwrapKeyAsync(byte[] wrappedDek, string password, byte[] salt, int iterations, int memorySize, byte[] nonce)
    {
        return await Task.Run(() =>
        {
            try
            {
                // Safety check for memory range (NSec minimum is 8 KiB)
                if (memorySize < 8)
                {
                    _logger.LogError("Stored vault memory size ({Size} KiB) is corrupted or too low.", memorySize);
                    return null;
                }

                var argonParams = new Argon2Parameters
                {
                    NumberOfPasses = iterations,
                    MemorySize = memorySize,
                    DegreeOfParallelism = DefaultParallelism
                };

                var argon2 = PasswordBasedKeyDerivationAlgorithm.Argon2id(argonParams);
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

                using var kek = argon2.DeriveKey(passwordBytes, salt, AeadAlgorithm.Aes256Gcm);

                var aes = AeadAlgorithm.Aes256Gcm;
                return aes.Decrypt(kek, nonce, null, wrappedDek);
            }
            catch (CryptographicException)
            {
                _logger.LogWarning("Unwrap failed: Authentication tag mismatch (Invalid Password).");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected failure during UnwrapKeyAsync.");
                return null;
            }
        });
    }

    private static void ValidateInputs(byte[] rawDek, string password)
    {
        if (rawDek == null || rawDek.Length == 0)
            throw new ArgumentException("DEK cannot be empty.", nameof(rawDek));
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be empty.", nameof(password));
    }
}