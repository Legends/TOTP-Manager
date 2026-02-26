namespace TOTP.Security.Interfaces;

public interface IMasterPasswordService
{
    bool IsConfigured { get; }
    bool Verify(string password);
}
