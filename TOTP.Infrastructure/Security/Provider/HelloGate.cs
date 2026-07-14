using System.Security.Cryptography;
using Windows.Security.Credentials.UI;
using Microsoft.Extensions.Logging;
using TOTP.Core.Security.Interfaces;
using TOTP.Core.Security.Models;

namespace TOTP.Infrastructure.Security.Provider;

public sealed class HelloGate : IHelloGate
{
    private readonly ILogger<HelloGate> _logger;
    private readonly IHelloPromptWindowHandleProvider _windowHandleProvider;
    private readonly IHelloVerificationRequester _verificationRequester;
    private const string ProviderName = "Microsoft Software Key Storage Provider";
    // Note: In production enterprise, use "Microsoft Platform Crypto Provider" for TPM hardware.

    public HelloGate(
        ILogger<HelloGate> logger,
        IHelloPromptWindowHandleProvider windowHandleProvider,
        IHelloVerificationRequester verificationRequester)
    {
        _logger = logger;
        _windowHandleProvider = windowHandleProvider;
        _verificationRequester = verificationRequester;
    }

    public async Task<bool> IsAvailableAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var availability = await UserConsentVerifier.CheckAvailabilityAsync().AsTask(ct);
        return availability == UserConsentVerifierAvailability.Available;
    }

    public async Task<AuthorizationResult> RequestVerificationAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        const string message = "Unlock TOTP Manager Vault";
        var windowHandle = _windowHandleProvider.GetActiveWindowHandle();
        var result = await _verificationRequester.RequestAsync(windowHandle, message, ct);
        return result switch
        {
            UserConsentVerificationResult.Verified => AuthorizationResult.Success,
            UserConsentVerificationResult.Canceled => AuthorizationResult.Cancelled,
            UserConsentVerificationResult.RetriesExhausted => AuthorizationResult.TooManyAttempts,
            UserConsentVerificationResult.DisabledByPolicy => AuthorizationResult.DisabledByPolicy,
            UserConsentVerificationResult.DeviceBusy or
            UserConsentVerificationResult.DeviceNotPresent or
            UserConsentVerificationResult.NotConfiguredForUser => AuthorizationResult.NotAvailable,
            _ => AuthorizationResult.Failed
        };
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

    public async Task<byte[]?> UnprotectKeyAsync(byte[] wrappedDek, string keyId, CancellationToken ct = default)
    {
        try
        {
            ct.ThrowIfCancellationRequested();
            return await Task.Run(() =>
            {
                ct.ThrowIfCancellationRequested();
                // This can trigger the Windows Hello popup because the key is in the Platform Crypto Provider.
                using var key = CngKey.Open(keyId, CngProvider.MicrosoftPlatformCryptoProvider);
                using var rsa = new RSACng(key);

                return rsa.Decrypt(wrappedDek, RSAEncryptionPadding.OaepSHA256);
            }, ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (CryptographicException ex)
        {
            _logger.LogWarning(ex, "TPM/Hello unwrap failed. User might have cancelled or hardware changed.");
            return null;
        }
    }
}
