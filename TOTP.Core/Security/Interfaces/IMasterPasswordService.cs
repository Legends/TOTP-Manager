namespace TOTP.Security.Interfaces;

public interface IMasterPasswordService
{
    // Generates a new salt and wraps the DEK for initial setup
    Task<(byte[] WrappedDek, byte[] Salt, int Iterations, int MemorySize, byte[] Nonce)>
        WrapKeyAsync(byte[] rawDek, string password);

    // Attempts to recover the DEK using the password; returns null if incorrect
    Task<byte[]?> UnwrapKeyAsync(byte[] wrappedDek, string password, byte[] salt, int iterations, int memorySize, byte[] nonce);
}