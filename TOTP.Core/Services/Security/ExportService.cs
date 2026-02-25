using FluentResults;
using NSec.Cryptography;
using System.Text;
using System.Text.Json;
using TOTP.Core.Models;
using TOTP.Core.Services.Security.Interfaces;

namespace TOTP.Core.Services.Security;

public sealed class ExportService : IExportService
{
    private static readonly byte[] MagicBytes = Encoding.ASCII.GetBytes("TOTP");
    private const int SaltSize = 16;

    private static readonly Argon2Parameters _argonParameters = new Argon2Parameters
    {
        DegreeOfParallelism = 1, // NSec v25.4.0 Limitation
        MemorySize = 128 * 1024, // 128 MiB
        NumberOfPasses = 4
    };

    private static readonly Argon2id _kdf = PasswordBasedKeyDerivationAlgorithm.Argon2id(in _argonParameters);
    private static readonly AeadAlgorithm _aead = AeadAlgorithm.Aes256Gcm;

    public async Task<Result> ExportToEncryptedFileAsync(IEnumerable<OtpEntry> accounts, string password, string filePath)
    {
        
        byte[]? plaintext = null;
        byte[]? passwordBytes = null;

        try
        {
            var json = JsonSerializer.Serialize(accounts);
            plaintext = Encoding.UTF8.GetBytes(json);
            passwordBytes = Encoding.UTF8.GetBytes(password);

            var salt = System.Security.Cryptography.RandomNumberGenerator.GetBytes(SaltSize);
            var nonce = System.Security.Cryptography.RandomNumberGenerator.GetBytes(_aead.NonceSize);

            using var key = _kdf.DeriveKey(passwordBytes, salt, _aead);
            var ciphertextWithTag = _aead.Encrypt(key, nonce, default, plaintext);

            await using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);
            await fs.WriteAsync(MagicBytes.AsMemory());
            await fs.WriteAsync(salt.AsMemory());
            await fs.WriteAsync(nonce.AsMemory());
            await fs.WriteAsync(ciphertextWithTag.AsMemory());
            await fs.FlushAsync();

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Export fehlgeschlagen.").CausedBy(ex));
        }
        finally
        {
            if (plaintext != null) Array.Clear(plaintext, 0, plaintext.Length);
            if (passwordBytes != null) Array.Clear(passwordBytes, 0, passwordBytes.Length);
        }
    }

    public async Task<Result<List<OtpEntry>>> ImportFromEncryptedFileAsync(string password, string filePath)
    {
        //if (string.IsNullOrWhiteSpace(password)) return Result.Fail("Passwort wurde nicht angegeben.");
        //if (!File.Exists(filePath)) return Result.Fail("Die gewählte Datei existiert nicht.");

        byte[]? passwordBytes = null;

        try
        {
            var fileBytes = await File.ReadAllBytesAsync(filePath);
            int nonceSize = _aead.NonceSize;
            int headerSize = MagicBytes.Length + SaltSize + nonceSize;

            if (fileBytes.Length < headerSize + _aead.TagSize)
                return Result.Fail("Datei ist beschädigt oder zu klein.");

            if (!fileBytes.AsSpan(0, MagicBytes.Length).SequenceEqual(MagicBytes))
                return Result.Fail("Dies ist keine gültige TOTP-Exportdatei.");

            var salt = fileBytes.AsSpan(MagicBytes.Length, SaltSize);
            var nonce = fileBytes.AsSpan(MagicBytes.Length + SaltSize, nonceSize);
            var encryptedData = fileBytes.AsSpan(headerSize);

            passwordBytes = Encoding.UTF8.GetBytes(password);
            using var key = _kdf.DeriveKey(passwordBytes, salt, _aead);

            byte[] decryptedBytes = new byte[encryptedData.Length - _aead.TagSize];

            if (!_aead.Decrypt(key, nonce, default, encryptedData, decryptedBytes))
                return Result.Fail("Entschlüsselung fehlgeschlagen. Passwort falsch oder Datei manipuliert.");

            var json = Encoding.UTF8.GetString(decryptedBytes);
            var result = JsonSerializer.Deserialize<List<OtpEntry>>(json);

            return Result.Ok(result ?? new List<OtpEntry>());
        }
        catch (JsonException)
        {
            return Result.Fail("Die Datei konnte entschlüsselt werden, aber der Inhalt ist kein gültiges JSON.");
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Ein unerwarteter Fehler ist aufgetreten.").CausedBy(ex));
        }
        finally
        {
            if (passwordBytes != null) Array.Clear(passwordBytes, 0, passwordBytes.Length);
        }
    }
}