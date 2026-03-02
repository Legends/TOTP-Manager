using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using TOTP.Core.Security;
using TOTP.Core.Security.Interfaces;
using TOTP.Core.Services.Interfaces;
using TOTP.Security.Interfaces;
using TOTP.Security.Models;

namespace TOTP.Infrastructure.Security;

public sealed class AuthorizationService : IAuthorizationService
{
    private readonly IGlobalProfileStore _profileStore;
    private readonly ISecurityContext _securityContext;
    private readonly IMasterPasswordService _passwordService;
    private readonly IHelloGate _helloGate;
    private readonly ILogger<AuthorizationService> _logger;

    // We hold the profile in memory after LoadAsync to avoid constant disk hits
    private GlobalProfile? _cachedProfile;

    public AuthorizationState State { get; }

    public AuthorizationService(
        IGlobalProfileStore profileStore,
        ISecurityContext securityContext,
        IMasterPasswordService passwordService,
        IHelloGate helloGate,
        AuthorizationState state,
        ILogger<AuthorizationService> logger)
    {
        _profileStore = profileStore;
        _securityContext = securityContext;
        _passwordService = passwordService;
        _helloGate = helloGate;
        State = state;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        _cachedProfile = await _profileStore.LoadAsync();
        State.SetProfile(_cachedProfile?.Authorization);
    }

    private async Task SaveCurrentProfileAsync()
    {
        if (_cachedProfile != null)
        {
            await _profileStore.SaveAsync(_cachedProfile);
        }
    }

    public async Task<bool> IsHelloAvailableAsync() => await _helloGate.IsAvailableAsync();

    public async Task<AuthorizationResult> TryUnlockOnStartupAsync()
    {
        if (_cachedProfile?.Authorization == null || !_cachedProfile.Authorization.IsConfigured)
            return AuthorizationResult.NotConfigured;

        var auth = _cachedProfile.Authorization;

        if (auth.Gate == AuthorizationGateKind.Hello)
            return await TryUnlockWithHelloAsync();

        return AuthorizationResult.PasswordRequired;
    }

    public async Task<AuthorizationResult> TryUnlockWithPasswordAsync(string password)
    {
        var auth = _cachedProfile?.Authorization;
        if (auth == null || auth.PasswordWrappedDek == null) return AuthorizationResult.NotConfigured;

        var dek = await _passwordService.UnwrapKeyAsync(
            auth.PasswordWrappedDek!,
            password,
            auth.PasswordSalt!,
            auth.ArgonIterations,
            auth.ArgonMemorySize,
            auth.DekNonce!);

        if (dek == null) return AuthorizationResult.InvalidCredentials;

        _securityContext.SetDek(dek);
        State.Unlock();
        return AuthorizationResult.Success;
    }

    public async Task<AuthorizationResult> TryUnlockWithHelloAsync()
    {
        var auth = _cachedProfile?.Authorization;
        if (auth?.HelloWrappedDek == null) return AuthorizationResult.PasswordRequired;

        var result = await _helloGate.RequestVerificationAsync();
        if (result != AuthorizationResult.Success) return result;

        // TPM Unwrapping
        var dek = await _helloGate.UnprotectKeyAsync(auth.HelloWrappedDek, auth.HelloKeyId!);
        if (dek == null) return AuthorizationResult.PasswordRequired;

        _securityContext.SetDek(dek);
        State.Unlock();
        return AuthorizationResult.Success;
    }

    public async Task<AuthorizationResult> ConfigurePasswordAsync(string password, string confirmPassword)
    {
        if (password != confirmPassword) return AuthorizationResult.InvalidCredentials;

        // Generate a cryptographically strong 256-bit DEK
        byte[] rawDek = RandomNumberGenerator.GetBytes(32);

        var wrapped = await _passwordService.WrapKeyAsync(rawDek, password);

        _cachedProfile ??= new GlobalProfile();
        _cachedProfile.Authorization = new AuthorizationProfile
        {
            Gate = AuthorizationGateKind.Password,
            PasswordSalt = wrapped.Salt,
            ArgonIterations = wrapped.Iterations,
            ArgonMemorySize = wrapped.MemorySize,
            PasswordWrappedDek = wrapped.WrappedDek,
            DekNonce = wrapped.Nonce
        };

        await SaveCurrentProfileAsync();

        _securityContext.SetDek(rawDek);
        State.SetProfile(_cachedProfile.Authorization);
        State.Unlock();

        return AuthorizationResult.Success;
    }

    public async Task<AuthorizationResult> ConfigureHelloAsync()
    {
        if (!_securityContext.IsUnlocked) return AuthorizationResult.PasswordRequired;
        if (_cachedProfile?.Authorization == null) return AuthorizationResult.NotConfigured;

        string keyId = $"TOTP_TPM_{Guid.NewGuid()}";
        var helloWrapped = await _helloGate.ProtectKeyAsync(_securityContext.GetDek(), keyId);

        _cachedProfile.Authorization.HelloWrappedDek = helloWrapped;
        _cachedProfile.Authorization.HelloKeyId = keyId;
        _cachedProfile.Authorization.Gate = AuthorizationGateKind.Hello;

        await SaveCurrentProfileAsync();
        State.SetProfile(_cachedProfile.Authorization);

        return AuthorizationResult.Success;
    }

    public async Task<AuthorizationResult> SetGateAsync(AuthorizationGateKind gate)
    {
        if (_cachedProfile?.Authorization == null) return AuthorizationResult.NotConfigured;

        _cachedProfile.Authorization.Gate = gate;
        await SaveCurrentProfileAsync();
        State.SetProfile(_cachedProfile.Authorization);

        return AuthorizationResult.Success;
    }

    public async Task<AuthorizationResult> ChangePasswordAsync(string currentPassword, string newPassword)
    {
        var currentAuth = await TryUnlockWithPasswordAsync(currentPassword);
        if (currentAuth != AuthorizationResult.Success) return currentAuth;

        return await ConfigurePasswordAsync(newPassword, newPassword);
    }

    public void Logout() => Lock();

    public void Lock()
    {
        _securityContext.Lock();
        State.Lock();
    }
}