namespace TOTP.Core.Services.Interfaces;

public interface ISecurityContext
{
    bool IsUnlocked { get; }
    void SetDek(byte[] dek);
    byte[] GetDek();
    void Lock();
}