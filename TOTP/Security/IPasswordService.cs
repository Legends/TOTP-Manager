namespace TOTP.Security;

public interface IPasswordService
{
    bool IsConfigured { get; }
    bool Verify(string password);
}
