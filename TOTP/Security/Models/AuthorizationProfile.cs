namespace TOTP.Security.Models;

public sealed class AuthorizationProfile
{
    public AuthorizationGateKind Gate { get; set; } = AuthorizationGateKind.None;

    // Password auth data (only if Gate == Password)
    public byte[]? PasswordSalt { get; set; }
    public byte[]? PasswordHash { get; set; }

    public bool IsConfigured => Gate != AuthorizationGateKind.None;
}