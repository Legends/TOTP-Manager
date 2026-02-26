namespace TOTP.Security.Models;

public sealed class AuthorizationProfile
{
    public AuthorizationGateKind Gate { get; set; } = AuthorizationGateKind.None;

    // Passwort-Authentifizierungsdaten
    public byte[]? PasswordSalt { get; set; }
    public byte[]? PasswordHash { get; set; }

    /// <summary>
    /// Die Anzahl der Argon2-Durchläufe (Passes/Iterations).
    /// </summary>
    public int ArgonIterations { get; set; }

    /// <summary>
    /// Der Argon2-Speicherverbrauch in KiB.
    /// </summary>
    public int ArgonMemorySize { get; set; }

    public bool IsConfigured => Gate != AuthorizationGateKind.None;
}