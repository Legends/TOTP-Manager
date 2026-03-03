using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Linq;
using TOTP.Core.Security;
using TOTP.Core.Security.Interfaces;
using TOTP.Core.Security.Models;
using TOTP.Core.Services.Interfaces;

namespace TOTP.Infrastructure.Security;

public sealed class AuthorizationService : IAuthorizationService
{
    private readonly ISettingsService _settingsService;
    private readonly ISecurityContext _securityContext;
    private readonly IMasterPasswordService _passwordService;
    private readonly IHelloGate _helloGate;
    private readonly ILogger<AuthorizationService> _logger;

    // We hold the profile in memory after LoadAsync to avoid constant disk hits
    private IAppSettings? _appSettings;

    public AuthorizationState State { get; }

    public AuthorizationService(
        ISettingsService settingsService,
        ISecurityContext securityContext,
        IMasterPasswordService passwordService,
        IHelloGate helloGate,
        AuthorizationState state,
        ILogger<AuthorizationService> logger)
    {
        _settingsService = settingsService;
        _securityContext = securityContext;
        _passwordService = passwordService;
        _helloGate = helloGate;
        State = state;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        _appSettings = _settingsService.Current;
        State.SetProfile(_appSettings?.Authorization);
        //State.Lock();
    }

    private async Task<bool> SaveCurrentProfileAsync()
    {
        if (_appSettings != null)
        {
            var saveResult = await _settingsService.SaveAsync();
            if (saveResult.IsFailed)
            {
                _logger.LogError("Failed to persist authorization profile: {Errors}", string.Join("; ", saveResult.Errors.Select(e => e.Message)));
                return false;
            }
        }

        return true;
    }

    public async Task<bool> IsHelloAvailableAsync() => await _helloGate.IsAvailableAsync();

    public async Task<AuthorizationResult> TryUnlockOnStartupAsync()
    {
        if (_appSettings?.Authorization == null || !_appSettings.Authorization.IsConfigured)
            return AuthorizationResult.NotConfigured;

        var auth = _appSettings.Authorization;

        if (auth.Gate == AuthorizationGateKind.Hello)
            return await TryUnlockWithHelloAsync();

        return AuthorizationResult.PasswordRequired;
    }

    public async Task<AuthorizationResult> TryUnlockWithPasswordAsync(string password)
    {
        var auth = _appSettings?.Authorization;
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
        var auth = _appSettings?.Authorization;
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

        //_appSettings ??= new AppSettings();
        _appSettings.Authorization = new AuthorizationProfile
        {
            Gate = AuthorizationGateKind.Password,
            PasswordSalt = wrapped.Salt,
            ArgonIterations = wrapped.Iterations,
            ArgonMemorySize = wrapped.MemorySize,
            PasswordWrappedDek = wrapped.WrappedDek,
            DekNonce = wrapped.Nonce
        };

        if (!await SaveCurrentProfileAsync())
            return AuthorizationResult.Failed;

        _securityContext.SetDek(rawDek);
        State.SetProfile(_appSettings.Authorization);
        State.Unlock();

        return AuthorizationResult.Success;
    }

    public async Task<AuthorizationResult> ConfigureHelloAsync()
    {
        if (!_securityContext.IsUnlocked) return AuthorizationResult.PasswordRequired;
        if (_appSettings?.Authorization == null) return AuthorizationResult.NotConfigured;

        string keyId = $"TOTP_TPM_{Guid.NewGuid()}";
        var helloWrapped = await _helloGate.ProtectKeyAsync(_securityContext.GetDek(), keyId);

        _appSettings.Authorization.HelloWrappedDek = helloWrapped;
        _appSettings.Authorization.HelloKeyId = keyId;
        _appSettings.Authorization.Gate = AuthorizationGateKind.Hello;

        if (!await SaveCurrentProfileAsync())
            return AuthorizationResult.Failed;
        State.SetProfile(_appSettings.Authorization);

        return AuthorizationResult.Success;
    }

    public async Task<AuthorizationResult> SetGateAsync(AuthorizationGateKind gate)
    {
        if (_appSettings?.Authorization == null) return AuthorizationResult.NotConfigured;

        _appSettings.Authorization.Gate = gate;
        if (!await SaveCurrentProfileAsync())
            return AuthorizationResult.Failed;
        State.SetProfile(_appSettings.Authorization);

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
