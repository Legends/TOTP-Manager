using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FluentResults;
using NSec.Cryptography;
using TOTP.Core.Common;
using TOTP.Infrastructure.Common;
using TOTP.Core.Models;
using TOTP.Core.Services.Interfaces;

namespace TOTP.Infrastructure.Services;

public sealed class ExportService : IExportService
{
    private static readonly byte[] MagicBytes = Encoding.ASCII.GetBytes("TOTP");
    private const int SaltSize = 16;

    private static readonly Argon2Parameters _argonParameters = new()
    {
        DegreeOfParallelism = 1,
        MemorySize = 128 * 1024,
        NumberOfPasses = 4
    };

    private static readonly Argon2id _kdf = PasswordBasedKeyDerivationAlgorithm.Argon2id(in _argonParameters);
    private static readonly AeadAlgorithm _algoAead = AeadAlgorithm.Aes256Gcm;

    public async Task<Result> ExportToEncryptedFileAsync(IEnumerable<OtpEntry> accounts, string password, string filePath)
    {
        byte[]? plaintext = null;
        byte[]? passwordBytes = null;

        try
        {
            var json = JsonSerializer.Serialize(accounts);
            plaintext = Encoding.UTF8.GetBytes(json);
            passwordBytes = Encoding.UTF8.GetBytes(password);

            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var nonce = RandomNumberGenerator.GetBytes(_algoAead.NonceSize);

            using var key = _kdf.DeriveKey(passwordBytes, salt, _algoAead);
            var ciphertextWithTag = _algoAead.Encrypt(key, nonce, default, plaintext);

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
            return Result.Fail(ExportServiceErrorMapper.MapExportError(ex));
        }
        finally
        {
            if (plaintext != null) Array.Clear(plaintext, 0, plaintext.Length);
            if (passwordBytes != null) Array.Clear(passwordBytes, 0, passwordBytes.Length);
        }
    }

    public async Task<Result<List<OtpEntry>>> ImportFromEncryptedFileAsync(string password, string filePath)
    {
        byte[]? passwordBytes = null;

        try
        {
            var fileBytes = await File.ReadAllBytesAsync(filePath);
            int nonceSize = _algoAead.NonceSize;
            int headerSize = MagicBytes.Length + SaltSize + nonceSize;

            if (fileBytes.Length < headerSize + _algoAead.TagSize)
            {
                return Result.Fail(new AppError(AppErrorCode.ImportInvalidFile, "Encrypted import file is invalid."));
            }

            if (!fileBytes.AsSpan(0, MagicBytes.Length).SequenceEqual(MagicBytes))
            {
                return Result.Fail(new AppError(AppErrorCode.ImportInvalidFile, "Encrypted import file header is invalid."));
            }

            var salt = fileBytes.AsSpan(MagicBytes.Length, SaltSize);
            var nonce = fileBytes.AsSpan(MagicBytes.Length + SaltSize, nonceSize);
            var encryptedData = fileBytes.AsSpan(headerSize);

            passwordBytes = Encoding.UTF8.GetBytes(password);
            using var key = _kdf.DeriveKey(passwordBytes, salt, _algoAead);

            byte[] decryptedBytes = new byte[encryptedData.Length - _algoAead.TagSize];
            if (!_algoAead.Decrypt(key, nonce, default, encryptedData, decryptedBytes))
            {
                return Result.Fail(new AppError(AppErrorCode.ImportWrongPasswordOrTampered, "Import decryption failed."));
            }

            var json = Encoding.UTF8.GetString(decryptedBytes);
            var result = JsonSerializer.Deserialize<List<OtpEntry>>(json);
            return Result.Ok(result ?? []);
        }
        catch (Exception ex)
        {
            return Result.Fail(ExportServiceErrorMapper.MapImportError(ex));
        }
        finally
        {
            if (passwordBytes != null) Array.Clear(passwordBytes, 0, passwordBytes.Length);
        }
    }

}
