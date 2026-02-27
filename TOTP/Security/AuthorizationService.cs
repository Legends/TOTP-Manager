using Microsoft.Extensions.Logging;
using System;
using System.Security;
using System.Threading.Tasks;
using TOTP.Security.Helpers;
using TOTP.Security.Interfaces;
using TOTP.Security.Models;

namespace TOTP.Security;

public sealed class AuthorizationService(
    IHelloGate helloGate,
    IGlobalProfileStore globalProfileStore,
    ILogger<AuthorizationService> logger,
    IKeyWrappingService keyWrappingService,
    ISecurityContext securityContext) : IAuthorizationService
{
    private ILogger<AuthorizationService> _logger = logger;
    private GlobalProfile _globalProfile = new();
    private AuthorizationProfile? _authorizationProfile;

    public AuthorizationState State { get; } = new();

    public async Task InitializeAsync()
    {
        _globalProfile = await globalProfileStore.LoadAsync().ConfigureAwait(false) ?? new GlobalProfile();
        _authorizationProfile = _globalProfile.Authorization;
        State.SetProfile(_authorizationProfile);

        // Always start locked.
        State.Lock();
    }

    public async Task<AuthorizationResult> TryUnlockOnStartupAsync()
    {
        if (_authorizationProfile?.IsConfigured != true)
            return AuthorizationResult.NotConfigured;

        if (_authorizationProfile.Gate == AuthorizationGateKind.WindowsHello)
            return await TryUnlockWithHelloAsync();

        return AuthorizationResult.RequiresUserInput;
    }

    public Task<bool> IsHelloAvailableAsync() => helloGate.IsAvailableAsync();

    public async Task<AuthorizationResult> ConfigureHelloAsync()
    {
        if (!await helloGate.IsAvailableAsync().ConfigureAwait(false))
            return AuthorizationResult.NotAvailable;

        _globalProfile = await globalProfileStore.LoadAsync().ConfigureAwait(false) ?? _globalProfile;
        _authorizationProfile = new AuthorizationProfile { Gate = AuthorizationGateKind.WindowsHello };
        _globalProfile.Authorization = _authorizationProfile;
        await globalProfileStore.SaveAsync(_globalProfile);

        State.SetProfile(_authorizationProfile);
        return AuthorizationResult.Success;
    }

    public async Task<AuthorizationResult> AuthorizeAsync(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return AuthorizationResult.InvalidCredentials;

        if (_authorizationProfile == null || !_authorizationProfile.IsConfigured)
            return AuthorizationResult.InvalidCredentials;

        try
        {
            var storedRecord = new PasswordRecord(
                _authorizationProfile.PasswordSalt!,
                _authorizationProfile.PasswordHash!,
                _authorizationProfile.ArgonIterations,
                _authorizationProfile.ArgonMemorySize
            );

            if (!PasswordHasher.Verify(password, storedRecord))
                return AuthorizationResult.InvalidCredentials;
            
            byte[] kek = PasswordHasher.HashWithParams(password, storedRecord);

            try
            {
                byte[] rawDek = keyWrappingService.UnwrapDek(
                    _authorizationProfile.WrappedDataEncryptionKey!,
                    kek,
                    _authorizationProfile.DekNonce!
                );

                securityContext.SetDek(rawDek);
                Array.Clear(rawDek, 0, rawDek.Length);
            }
            finally
            {
                // Always clear the Key Encryption Key from memory
                Array.Clear(kek, 0, kek.Length);
            }

            State.Unlock(); // Ensure the UI state is updated
            return AuthorizationResult.Success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Authorization failed due to an unexpected error.");
            return AuthorizationResult.InvalidCredentials;
        }
    }

    public void Logout()
    {
        securityContext.Lock();
        State.Lock();
    }

    public async Task<AuthorizationResult> ConfigurePasswordAsync(string password, string confirmPassword)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 1)
            return AuthorizationResult.InvalidCredentials;

        if (!string.Equals(password, confirmPassword, StringComparison.Ordinal))
            return AuthorizationResult.InvalidCredentials;

        try
        {
            // Create initial hash and salt
            var record = PasswordHasher.Hash(password);

            // Generate the master data key
            var rawDek = keyWrappingService.GenerateRawDek();

            // Wrap the DEK with the password-derived hash (record.Hash is our KEK here)
            var (wrappedDek, dekNonce) = keyWrappingService.WrapDek(rawDek, record.Hash);

            _authorizationProfile = new AuthorizationProfile
            {
                Gate = AuthorizationGateKind.Password,
                PasswordSalt = record.Salt,
                PasswordHash = record.Hash,
                ArgonIterations = record.Iterations,
                ArgonMemorySize = record.MemorySize,
                WrappedDataEncryptionKey = wrappedDek,
                DekNonce = dekNonce
            };

            _globalProfile = await globalProfileStore.LoadAsync().ConfigureAwait(false) ?? _globalProfile;
            _globalProfile.Authorization = _authorizationProfile;
            await globalProfileStore.SaveAsync(_globalProfile).ConfigureAwait(false);

            securityContext.SetDek(rawDek);
            Array.Clear(rawDek, 0, rawDek.Length);

            State.SetProfile(_authorizationProfile);
            State.Unlock();
            return AuthorizationResult.Success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to configure password.");
            return AuthorizationResult.InvalidCredentials;
        }
    }

    public async Task<AuthorizationResult> TryUnlockWithHelloAsync()
    {
        try
        {
            if (!await helloGate.IsAvailableAsync().ConfigureAwait(false))
                return AuthorizationResult.NotAvailable;

            var result = await helloGate.RequestVerificationAsync();
            if (result == AuthorizationResult.Success)
            {
                // Note: Windows Hello currently doesn't store a DEK. 
                // We will need to address how Hello unlocks the DEK in a later step.
                State.Unlock();
            }

            return result;
        }
        catch
        {
            return AuthorizationResult.Failed;
        }
    }

    public async Task<AuthorizationResult> TryUnlockWithPasswordAsync(string password)
    {
        // Simply delegate to AuthorizeAsync to avoid code duplication
        return await AuthorizeAsync(password);
    }

    public void Lock()
    {
        securityContext.Lock();
        State.Lock();
    }
}