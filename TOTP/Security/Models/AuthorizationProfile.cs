namespace TOTP.Security.Models;

public sealed class AuthorizationProfile
{
    // The preferred gate (the one shown by default)
    public AuthorizationGateKind Gate { get; set; } = AuthorizationGateKind.None;

    // --- Password Auth Metadata ---
    public byte[]? PasswordSalt { get; set; }
    public byte[]? PasswordHash { get; set; }
    public int ArgonIterations { get; set; }
    public int ArgonMemorySize { get; set; }
    public byte[]? WrappedDataEncryptionKey { get; set; } // DEK wrapped by Password
    public byte[]? DekNonce { get; set; }

    // --- Windows Hello Metadata ---
    public byte[]? HelloWrappedDek { get; set; } // DEK wrapped by Biometrics

    public bool IsConfigured => Gate != AuthorizationGateKind.None;

    // Check if a specific method is set up even if it's not the active 'Gate'
    public bool HasPasswordSetup => PasswordHash != null;
    public bool HasHelloSetup => HelloWrappedDek != null;

    public bool IsHelloSetup => HelloWrappedDek != null;
    public bool IsPasswordSetup => PasswordHash != null;
}