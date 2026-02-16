using System;
using System.Threading.Tasks;
using TOTP.Security.Interfaces;
using TOTP.Security.Models;

namespace TOTP.Security;

public sealed class AuthorizationService : IAuthorizationService
{
    private readonly IHelloGate _helloGate; // your existing hello abstraction
    private readonly IGlobalProfileStore _globalProfileStore;

    private GlobalProfile _globalProfile = new();
    private AuthorizationProfile? _authorizationProfile;

    public AuthorizationState State { get; } = new();

    public AuthorizationService(IHelloGate hello, IGlobalProfileStore store)
    {
        _helloGate = hello;
        _globalProfileStore = store;
    }

    public async Task InitializeAsync()
    {
        _globalProfile = await _globalProfileStore.LoadAsync().ConfigureAwait(false) ?? new GlobalProfile();
        _authorizationProfile = _globalProfile.Authorization;
        State.SetProfile(_authorizationProfile);

        // Always start locked.
        State.Lock();
    }

    public async Task<AuthorizationResult> TryUnlockOnStartupAsync()
    {
        if (_authorizationProfile?.IsConfigured != true)
            return AuthorizationResult.NotConfigured;

        // Your requirement: every start triggers the authorization gate.
        // Only Windows Hello can be auto-triggered without user typing.
        if (_authorizationProfile.Gate == AuthorizationGateKind.WindowsHello)
            return await TryUnlockWithHelloAsync();//.ConfigureAwait(false);

        // Password gate: user must type. Keep locked and show auth UI.
        return AuthorizationResult.RequiresUserInput;
    }

    public Task<bool> IsHelloAvailableAsync()
    {
        return _helloGate.IsAvailableAsync();
    }

    public async Task<AuthorizationResult> ConfigureHelloAsync()
    {
        if (!await _helloGate.IsAvailableAsync().ConfigureAwait(false))
            return AuthorizationResult.NotAvailable;

        _globalProfile = await _globalProfileStore.LoadAsync().ConfigureAwait(false) ?? _globalProfile;
        _authorizationProfile = new AuthorizationProfile { Gate = AuthorizationGateKind.WindowsHello };
        _globalProfile.Authorization = _authorizationProfile;
        await _globalProfileStore.SaveAsync(_globalProfile);//.ConfigureAwait(false);

        State.SetProfile(_authorizationProfile);
        return AuthorizationResult.Success; // configured ok (not unlocked yet)
    }

    public async Task<AuthorizationResult> ConfigurePasswordAsync(string password, string confirmPassword)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 3)
            return AuthorizationResult.InvalidCredentials;

        if (!string.Equals(password, confirmPassword, StringComparison.Ordinal))
            return AuthorizationResult.InvalidCredentials;

        var (salt, hash) = PasswordHasher.Hash(password);

        _authorizationProfile = new AuthorizationProfile
        {
            Gate = AuthorizationGateKind.Password,
            PasswordSalt = salt,
            PasswordHash = hash
        };

        _globalProfile = await _globalProfileStore.LoadAsync().ConfigureAwait(false) ?? _globalProfile;
        _globalProfile.Authorization = _authorizationProfile;
        await _globalProfileStore.SaveAsync(_globalProfile);//.ConfigureAwait(false);
        State.SetProfile(_authorizationProfile);

        return AuthorizationResult.Success; // configured ok
    }

    public async Task<AuthorizationResult> TryUnlockWithHelloAsync()
    {
        try
        {
            if (!await _helloGate.IsAvailableAsync().ConfigureAwait(false))
                return AuthorizationResult.NotAvailable;

            var result = await _helloGate.RequestVerificationAsync();//.ConfigureAwait(false);
            if (result == AuthorizationResult.Success)
                State.Unlock();

            return result;
        }
        catch
        {
            return AuthorizationResult.Failed;
        }
    }

    public Task<AuthorizationResult> TryUnlockWithPasswordAsync(string password)
    {
        if (_authorizationProfile?.IsConfigured != true || _authorizationProfile.Gate != AuthorizationGateKind.Password)
            return Task.FromResult(AuthorizationResult.NotConfigured);

        if (_authorizationProfile.PasswordSalt is null || _authorizationProfile.PasswordHash is null)
            return Task.FromResult(AuthorizationResult.Failed);

        var ok = PasswordHasher.Verify(password, _authorizationProfile.PasswordSalt, _authorizationProfile.PasswordHash);
        if (!ok)
            return Task.FromResult(AuthorizationResult.InvalidCredentials);

        State.Unlock();
        return Task.FromResult(AuthorizationResult.Success);
    }

    public void Lock()
    {
        State.Lock();
    }
}
