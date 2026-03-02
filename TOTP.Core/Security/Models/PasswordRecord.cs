namespace TOTP.Core.Security.Models;

public sealed record PasswordRecord(
    byte[] Salt,
    byte[] Hash,
    int Iterations,
    int MemorySize); 
