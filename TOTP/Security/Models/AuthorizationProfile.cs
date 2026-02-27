namespace TOTP.Security.Models;

public sealed class AuthorizationProfile
{
    public AuthorizationGateKind Gate { get; set; } = AuthorizationGateKind.None;

     
    public byte[]? PasswordSalt { get; set; }
    public byte[]? PasswordHash { get; set; }
    public int ArgonIterations { get; set; }
    public int ArgonMemorySize { get; set; }

    // NEU: Der "verpackte" Tresorschlüssel (Data Encryption Key)
    public byte[]? WrappedDataEncryptionKey { get; set; }
    public byte[]? DekNonce { get; set; }

    public bool IsConfigured => Gate != AuthorizationGateKind.None;
}