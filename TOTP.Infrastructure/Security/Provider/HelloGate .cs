using System.Security.Cryptography;
using Windows.Security.Credentials.UI;
using Microsoft.Extensions.Logging;
using TOTP.Core.Security.Interfaces;
using TOTP.Core.Security.Models;

namespace TOTP.Infrastructure.Security.Provider;
    
public sealed class HelloGate : IHelloGate
{
    private readonly ILogger<HelloGate> _logger;
    private const string ProviderName = "Microsoft Software Key Storage Provider";
    // Note: In production enterprise, use "Microsoft Platform Crypto Provider" for TPM hardware.

    public HelloGate(ILogger<HelloGate> logger) => _logger = logger;

    public async Task<bool> IsAvailableAsync(CancellationToken ct = default)
    {
        var availability = await UserConsentVerifier.CheckAvailabilityAsync();
        return availability == UserConsentVerifierAvailability.Available;
    }

    public async Task<AuthorizationResult> RequestVerificationAsync(CancellationToken ct = default)
    {
        var result = await UserConsentVerifier.RequestVerificationAsync("Unlock TOTP Manager Vault");
        return result == UserConsentVerificationResult.Verified ? AuthorizationResult.Success : AuthorizationResult.Failed;
    }

    public async Task<byte[]> ProtectKeyAsync(byte[] rawDek, string keyId)
    {
        return await Task.Run(() =>
        {
            CngKeyCreationParameters keyParams = new CngKeyCreationParameters
            {
                // This is the magic: The key is created IN the TPM and cannot be exported
                ExportPolicy = CngExportPolicies.None,
                Provider = CngProvider.MicrosoftPlatformCryptoProvider // This forces TPM usage
            };

            // 1. Create a 2048-bit RSA key in the TPM
            using var key = CngKey.Create(CngAlgorithm.Rsa, keyId, keyParams);

            // 2. Encrypt the DEK using the RSA Public Key
            using var rsa = new RSACng(key);
            return rsa.Encrypt(rawDek, RSAEncryptionPadding.OaepSHA256);
        });
    }

    public async Task<byte[]> UnprotectKeyAsync(byte[] wrappedDek, string keyId)
    {
        try
        {
            // This will trigger the Windows Hello popup automatically 
            // because the key is in the Platform Crypto Provider
            using var key = CngKey.Open(keyId, CngProvider.MicrosoftPlatformCryptoProvider);
            using var rsa = new RSACng(key);

            return rsa.Decrypt(wrappedDek, RSAEncryptionPadding.OaepSHA256);
        }
        catch (CryptographicException)
        {
            _logger.LogWarning("TPM/Hello unwrap failed. User might have cancelled or hardware changed.");
            return null;
        }
    }
}
