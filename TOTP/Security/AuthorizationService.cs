using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using TOTP.Core.Services.Interfaces;
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
    private GlobalProfile _globalProfile = new();
    private AuthorizationProfile? _authorizationProfile;
    public AuthorizationState State { get; } = new();

    public async Task InitializeAsync()
    {
        _globalProfile = await globalProfileStore.LoadAsync() ?? new GlobalProfile();
        _authorizationProfile = _globalProfile.Authorization;
        State.SetProfile(_authorizationProfile);
        State.Lock();
    }

    public void Lock() => Logout();

    public void Logout()
    {
        securityContext.Lock();
        State.Lock();
    }

    // Add this to IAuthorizationService and AuthorizationService
    public async Task SetGateAsync(AuthorizationGateKind gate)
    {
        if (_authorizationProfile == null) return;
        _authorizationProfile.Gate = gate;
        await SaveAndSyncProfileAsync();
    }

    public async Task<AuthorizationResult> TryUnlockOnStartupAsync()
    {
        if (_authorizationProfile?.IsConfigured != true) return AuthorizationResult.NotConfigured;

        // If Hello is the preferred gate, try it first.
        if (_authorizationProfile.Gate == AuthorizationGateKind.WindowsHello)
            return await TryUnlockWithHelloAsync();

        return AuthorizationResult.RequiresUserInput;
    }

    public async Task<AuthorizationResult> TryUnlockWithPasswordAsync(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || _authorizationProfile == null)
            return AuthorizationResult.InvalidCredentials;

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
                _authorizationProfile.DekNonce!);

            securityContext.SetDek(rawDek);
            Array.Clear(rawDek, 0, rawDek.Length);
            State.Unlock();
            return AuthorizationResult.Success;
        }
        finally { Array.Clear(kek, 0, kek.Length); }
    }

    public async Task<AuthorizationResult> TryUnlockWithHelloAsync()
    {
        if (_authorizationProfile?.HelloWrappedDek == null) return AuthorizationResult.NotConfigured;

        var authResult = await helloGate.RequestVerificationAsync();
        if (authResult != AuthorizationResult.Success) return authResult;

        byte[] rawDek = helloGate.UnprotectKey(_authorizationProfile.HelloWrappedDek);
        if (rawDek == null) return AuthorizationResult.Failed;

        securityContext.SetDek(rawDek);
        Array.Clear(rawDek, 0, rawDek.Length);
        State.Unlock();
        return AuthorizationResult.Success;
    }

    public async Task<AuthorizationResult> ConfigurePasswordAsync(string password, string confirmPassword)
    {
        if (string.IsNullOrWhiteSpace(password) || password != confirmPassword)
            return AuthorizationResult.InvalidCredentials;

        // Generate brand new DEK and wrap it
        var record = PasswordHasher.Hash(password);
        var rawDek = keyWrappingService.GenerateRawDek();
        var (wrappedDek, nonce) = keyWrappingService.WrapDek(rawDek, record.Hash);

        _authorizationProfile = new AuthorizationProfile
        {
            Gate = AuthorizationGateKind.Password,
            PasswordSalt = record.Salt,
            PasswordHash = record.Hash,
            ArgonIterations = record.Iterations,
            ArgonMemorySize = record.MemorySize,
            WrappedDataEncryptionKey = wrappedDek,
            DekNonce = nonce
        };

        securityContext.SetDek(rawDek);
        Array.Clear(rawDek, 0, rawDek.Length);
        await SaveAndSyncProfileAsync();
        State.Unlock();
        return AuthorizationResult.Success;
    }

    public async Task<AuthorizationResult> ConfigureHelloAsync()
    {
        if (!await helloGate.IsAvailableAsync()) return AuthorizationResult.NotAvailable;
        if (!securityContext.IsUnlocked) return AuthorizationResult.RequiresUserInput;

        byte[] rawDek = securityContext.GetDek();
        _authorizationProfile!.HelloWrappedDek = helloGate.ProtectKey(rawDek);
        _authorizationProfile.Gate = AuthorizationGateKind.WindowsHello;

        await SaveAndSyncProfileAsync();
        return AuthorizationResult.Success;
    }

    public async Task<AuthorizationResult> ChangePasswordAsync(string newPassword, string confirmPassword)
    {
        if (string.IsNullOrWhiteSpace(newPassword) || newPassword != confirmPassword)
            return AuthorizationResult.InvalidCredentials;
        if (!securityContext.IsUnlocked) return AuthorizationResult.RequiresUserInput;

        var record = PasswordHasher.Hash(newPassword);
        byte[] currentDek = securityContext.GetDek();
        var (wrappedDek, nonce) = keyWrappingService.WrapDek(currentDek, record.Hash);

        _authorizationProfile!.PasswordSalt = record.Salt;
        _authorizationProfile.PasswordHash = record.Hash;
        _authorizationProfile.WrappedDataEncryptionKey = wrappedDek;
        _authorizationProfile.DekNonce = nonce;

        await SaveAndSyncProfileAsync();
        return AuthorizationResult.Success;
    }

    private async Task SaveAndSyncProfileAsync()
    {
        _globalProfile.Authorization = _authorizationProfile;
        await globalProfileStore.SaveAsync(_globalProfile);
        State.SetProfile(_authorizationProfile);
    }

    public Task<bool> IsHelloAvailableAsync() => helloGate.IsAvailableAsync();
}