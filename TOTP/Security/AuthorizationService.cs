using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TOTP.Security.Interfaces;
using TOTP.Security.Models;

namespace TOTP.Security;

public sealed class AuthorizationService : IAuthorizationService
{
    private readonly IHelloGate _helloGate; // your existing hello abstraction
    private readonly IGlobalProfileStore _globalProfileStore;
    private ILogger<AuthorizationService> _logger;
    private GlobalProfile _globalProfile = new();
    private AuthorizationProfile? _authorizationProfile;

    public AuthorizationState State { get; } = new();

    public AuthorizationService(IHelloGate hello, IGlobalProfileStore store, ILogger<AuthorizationService> logger)
    {
        _logger = logger;
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
        // 1. Sicherheits-Check: Passwort-Stärke
        // Empfehlung: Erhöhe das Minimum auf mind. 8-12 Zeichen für eine TOTP-App
        if (string.IsNullOrWhiteSpace(password) || password.Length < 1)
            return AuthorizationResult.InvalidCredentials;

        // 2. Passwort-Vergleich
        if (!string.Equals(password, confirmPassword, StringComparison.Ordinal))
            return AuthorizationResult.InvalidCredentials;

        PasswordRecord? record = null;
        try
        {
            // 3. Argon2id Hashing (gibt den kompletten Record zurück)
            record = PasswordHasher.Hash(password);

            // 4. Profil-Erstellung mit allen Metadaten (wichtig für spätere Verifizierung)
            _authorizationProfile = new AuthorizationProfile
            {
                Gate = AuthorizationGateKind.Password,
                PasswordSalt = record.Salt,
                PasswordHash = record.Hash,
                // Du solltest diese Felder in dein AuthorizationProfile-Model aufnehmen:
                ArgonIterations = record.Iterations,
                ArgonMemorySize = record.MemorySize
            };

            // 5. Persistenz
            _globalProfile = await _globalProfileStore.LoadAsync().ConfigureAwait(false) ?? _globalProfile;
            _globalProfile.Authorization = _authorizationProfile;

            await _globalProfileStore.SaveAsync(_globalProfile).ConfigureAwait(false);

            State.SetProfile(_authorizationProfile);

            return AuthorizationResult.Success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Authentication failed");
            // Logge den Fehler (optional)
            return AuthorizationResult.InvalidCredentials; // Oder ein spezifischerer Fehler
        }

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
