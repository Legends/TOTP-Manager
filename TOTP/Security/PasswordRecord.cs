namespace TOTP.Security;

public sealed record PasswordRecord(
    byte[] Salt,
    byte[] Hash,
    int Iterations);
