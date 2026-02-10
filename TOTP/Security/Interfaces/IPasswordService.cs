namespace TOTP.Security.Interfaces;

public interface IPasswordService
{
    bool IsConfigured { get; }
    bool Verify(string password);
}
