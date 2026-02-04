using System;
using System.Threading.Tasks;

namespace TOTP.Security;

public sealed class AuthorizationService : IAuthorizationService
{
    private readonly IHelloGate _hello; // your existing hello abstraction
    private readonly IAuthorizationProfileStore _store;

    private AuthorizationProfile? _profile;

    public AuthorizationState State { get; } = new();

    public AuthorizationService(IHelloGate hello, IAuthorizationProfileStore store)
    {
        _hello = hello;
        _store = store;
    }

    public async Task InitializeAsync()
    {
        _profile = await _store.LoadAsync().ConfigureAwait(false);
        State.SetProfile(_profile);

        // Always start locked.
        State.Lock();
    }

    public async Task<AuthorizationResult> TryUnlockOnStartupAsync()
    {
        if (_profile?.IsConfigured != true)
            return AuthorizationResult.NotConfigured;

        // Your requirement: every start triggers the authorization gate.
        // Only Windows Hello can be auto-triggered without user typing.
        if (_profile.Gate == AuthorizationGateKind.WindowsHello)
            return await TryUnlockWithHelloAsync().ConfigureAwait(false);

        // Password gate: user must type. Keep locked and show auth UI.
        return AuthorizationResult.RequiresUserInput;
    }

    public async Task<AuthorizationResult> ConfigureHelloAsync()
    {
        if (!await _hello.IsAvailableAsync().ConfigureAwait(false))
            return AuthorizationResult.NotAvailable;

        _profile = new AuthorizationProfile { Gate = AuthorizationGateKind.WindowsHello };
        await _store.SaveAsync(_profile).ConfigureAwait(false);

        State.SetProfile(_profile);
        return AuthorizationResult.Success; // “configured ok” (not “unlocked yet”)
    }

    public async Task<AuthorizationResult> ConfigurePasswordAsync(string password, string confirmPassword)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            return AuthorizationResult.InvalidCredentials;

        if (!string.Equals(password, confirmPassword, StringComparison.Ordinal))
            return AuthorizationResult.InvalidCredentials;

        var (salt, hash) = PasswordHasher.Hash(password);

        _profile = new AuthorizationProfile
        {
            Gate = AuthorizationGateKind.Password,
            PasswordSalt = salt,
            PasswordHash = hash
        };

        await _store.SaveAsync(_profile).ConfigureAwait(false);
        State.SetProfile(_profile);

        return AuthorizationResult.Success; // configured ok
    }

    public async Task<AuthorizationResult> TryUnlockWithHelloAsync()
    {
        try
        {
            if (!await _hello.IsAvailableAsync().ConfigureAwait(false))
                return AuthorizationResult.NotAvailable;

            var result = await _hello.RequestVerificationAsync().ConfigureAwait(false);
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
        if (_profile?.IsConfigured != true || _profile.Gate != AuthorizationGateKind.Password)
            return Task.FromResult(AuthorizationResult.NotConfigured);

        if (_profile.PasswordSalt is null || _profile.PasswordHash is null)
            return Task.FromResult(AuthorizationResult.Failed);

        var ok = PasswordHasher.Verify(password, _profile.PasswordSalt, _profile.PasswordHash);
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
