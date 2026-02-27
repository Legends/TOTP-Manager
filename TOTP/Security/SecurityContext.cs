using System;
using TOTP.Security.Interfaces;

namespace TOTP.Security;

public class SecurityContext: ISecurityContext
{
    private byte[]? _rawDek;

    public bool IsUnlocked => _rawDek != null;

    public void SetDek(byte[] dek)
    {
        // Wir klonen das Array für die Sicherheit
        _rawDek = (byte[])dek.Clone();
    }

    public byte[] GetDek()
    {
        return _rawDek ?? throw new InvalidOperationException("App ist gesperrt. Kein Schlüssel vorhanden.");
    }

    public void Lock()
    {
        if (_rawDek != null)
        {
            // Speicher aktiv überschreiben vor dem Freigeben
            Array.Clear(_rawDek, 0, _rawDek.Length);
            _rawDek = null;
        }
    }
}