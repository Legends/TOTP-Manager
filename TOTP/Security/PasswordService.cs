using System.Security.Cryptography;
using TOTP.Security.Interfaces;
using TOTP.Security.Models;

namespace TOTP.Security;

public sealed class PasswordService : IPasswordService
{
    private readonly PasswordRecord _record;

    public bool IsConfigured => _record.Hash.Length > 0;

    public PasswordService(PasswordRecord record)
    {
        _record = record;
    }

    public bool Verify(string password)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(
            password,
            _record.Salt,
            _record.Iterations,
            HashAlgorithmName.SHA256);

        var candidate = pbkdf2.GetBytes(_record.Hash.Length);

        return CryptographicOperations.FixedTimeEquals(
            candidate,
            _record.Hash);
    }
}
