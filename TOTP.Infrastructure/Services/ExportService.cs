using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Globalization;
using FluentResults;
using Microsoft.Extensions.Logging;
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
    private readonly ILogger<ExportService> _logger;

    public ExportService(ILogger<ExportService> logger)
    {
        _logger = logger;
    }

    public async Task<Result<List<Account>>> ImportFromFileAsync(string filePath, string? password = null)
    {
        try
        {
            var extension = Path.GetExtension(filePath);
            if (extension.Equals(".totp", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(password))
                {
                    return Result.Fail(new AppError(AppErrorCode.ImportWrongPasswordOrTampered, "Password is required for encrypted import."));
                }

                return await ImportFromEncryptedFileAsync(password, filePath);
            }

            if (!TryGetUnencryptedFormat(extension, out var format))
            {
                return Result.Fail(new AppError(AppErrorCode.ImportInvalidFile, "Unsupported import file extension."));
            }

            var content = await File.ReadAllTextAsync(filePath, Encoding.UTF8);
            return Result.Ok(DeserializeTokens(content, format));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Import from file failed for path {Path}.", filePath);
            return Result.Fail(ExportServiceErrorMapper.MapImportError(ex));
        }
    }

    public async Task<Result> ExportToFileAsync(IEnumerable<Account> accounts, string filePath, ExportFileFormat format)
    {
        try
        {
            var payload = SerializeTokens(accounts, format);
            await File.WriteAllTextAsync(filePath, payload, Encoding.UTF8);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Export to file failed for path {Path} with format {Format}.", filePath, format);
            return Result.Fail(ExportServiceErrorMapper.MapExportError(ex));
        }
    }

    public async Task<Result> ExportToEncryptedFileAsync(IEnumerable<Account> accounts, string password, string filePath, ExportFileFormat format)
    {
        byte[]? plaintext = null;
        byte[]? passwordBytes = null;

        try
        {
            var payload = SerializeTokens(accounts, format);
            plaintext = Encoding.UTF8.GetBytes(payload);
            passwordBytes = Encoding.UTF8.GetBytes(password);

            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var nonce = RandomNumberGenerator.GetBytes(_algoAead.NonceSize);

            using var key = _kdf.DeriveKey(passwordBytes, salt, _algoAead);
            var ciphertextWithTag = _algoAead.Encrypt(key, nonce, default, plaintext);

            await using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);
            await fs.WriteAsync(MagicBytes.AsMemory());
            await fs.WriteAsync(new[] { (byte)format });
            await fs.WriteAsync(salt.AsMemory());
            await fs.WriteAsync(nonce.AsMemory());
            await fs.WriteAsync(ciphertextWithTag.AsMemory());
            await fs.FlushAsync();

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Encrypted export failed for path {Path} with format {Format}.", filePath, format);
            return Result.Fail(ExportServiceErrorMapper.MapExportError(ex));
        }
        finally
        {
            if (plaintext != null) Array.Clear(plaintext, 0, plaintext.Length);
            if (passwordBytes != null) Array.Clear(passwordBytes, 0, passwordBytes.Length);
        }
    }

    public async Task<Result<List<Account>>> ImportFromEncryptedFileAsync(string password, string filePath)
    {
        byte[]? passwordBytes = null;

        try
        {
            var fileBytes = await File.ReadAllBytesAsync(filePath);
            int nonceSize = _algoAead.NonceSize;
            int legacyHeaderSize = MagicBytes.Length + SaltSize + nonceSize;
            int versionedHeaderSize = MagicBytes.Length + 1 + SaltSize + nonceSize;

            if (fileBytes.Length < legacyHeaderSize + _algoAead.TagSize)
            {
                return Result.Fail(new AppError(AppErrorCode.ImportInvalidFile, "Encrypted import file is invalid."));
            }

            if (!fileBytes.AsSpan(0, MagicBytes.Length).SequenceEqual(MagicBytes))
            {
                return Result.Fail(new AppError(AppErrorCode.ImportInvalidFile, "Encrypted import file header is invalid."));
            }

            ExportFileFormat format = ExportFileFormat.Json;
            int offset = MagicBytes.Length;

            if (fileBytes.Length >= versionedHeaderSize + _algoAead.TagSize)
            {
                var marker = fileBytes[offset];
                if (marker is (byte)ExportFileFormat.Json or (byte)ExportFileFormat.Txt or (byte)ExportFileFormat.Csv)
                {
                    format = (ExportFileFormat)marker;
                    offset += 1;
                }
            }

            var salt = fileBytes.AsSpan(offset, SaltSize);
            var nonce = fileBytes.AsSpan(offset + SaltSize, nonceSize);
            var encryptedData = fileBytes.AsSpan(offset + SaltSize + nonceSize);

            passwordBytes = Encoding.UTF8.GetBytes(password);
            using var key = _kdf.DeriveKey(passwordBytes, salt, _algoAead);

            byte[] decryptedBytes = new byte[encryptedData.Length - _algoAead.TagSize];
            if (!_algoAead.Decrypt(key, nonce, default, encryptedData, decryptedBytes))
            {
                return Result.Fail(new AppError(AppErrorCode.ImportWrongPasswordOrTampered, "Import decryption failed."));
            }

            var content = Encoding.UTF8.GetString(decryptedBytes);
            return Result.Ok(DeserializeTokens(content, format));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Encrypted import failed for path {Path}.", filePath);
            return Result.Fail(ExportServiceErrorMapper.MapImportError(ex));
        }
        finally
        {
            if (passwordBytes != null) Array.Clear(passwordBytes, 0, passwordBytes.Length);
        }
    }

    private static string SerializeTokens(IEnumerable<Account> accounts, ExportFileFormat format)
    {
        return format switch
        {
            ExportFileFormat.Json => JsonSerializer.Serialize(accounts, new JsonSerializerOptions { WriteIndented = true }),
            ExportFileFormat.Txt => BuildTxt(accounts),
            ExportFileFormat.Csv => BuildCsv(accounts),
            _ => JsonSerializer.Serialize(accounts, new JsonSerializerOptions { WriteIndented = true })
        };
    }

    private static List<Account> DeserializeTokens(string content, ExportFileFormat format)
    {
        return format switch
        {
            ExportFileFormat.Json => JsonSerializer.Deserialize<List<Account>>(content) ?? [],
            ExportFileFormat.Txt => ParseTxt(content),
            ExportFileFormat.Csv => ParseCsv(content),
            _ => JsonSerializer.Deserialize<List<Account>>(content) ?? []
        };
    }

    private static string BuildCsv(IEnumerable<Account> accounts)
    {
        static string Escape(string? value)
        {
            var v = value ?? string.Empty;
            if (v.Contains('"') || v.Contains(',') || v.Contains('\n') || v.Contains('\r'))
            {
                return "\"" + v.Replace("\"", "\"\"") + "\"";
            }

            return v;
        }

        var lines = new List<string> { "id,issuer,account_name,secret" };
        lines.AddRange(accounts.Select(a => $"{a.ID},{Escape(a.Issuer)},{Escape(a.AccountName)},{Escape(a.Secret)}"));
        return string.Join(Environment.NewLine, lines);
    }

    private static string BuildTxt(IEnumerable<Account> accounts)
    {
        var lines = new List<string> { "issuer|account_name|secret|id" };
        lines.AddRange(accounts.Select(a => $"{a.Issuer}|{a.AccountName}|{a.Secret}|{a.ID}"));
        return string.Join(Environment.NewLine, lines);
    }

    private static List<Account> ParseTxt(string content)
    {
        var result = new List<Account>();
        var lines = content.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            if (line.Equals("issuer|account_name|secret|id", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var parts = line.Split('|');
            if (parts.Length < 3)
            {
                continue;
            }

            var issuer = parts[0];
            var accountName = string.IsNullOrWhiteSpace(parts[1]) ? null : parts[1];
            var secret = parts[2];
            var id = parts.Length >= 4 && Guid.TryParse(parts[3], out var parsedId) ? parsedId : Guid.NewGuid();
            result.Add(new Account(id, issuer, secret, accountName));
        }

        return result;
    }

    private static List<Account> ParseCsv(string content)
    {
        var result = new List<Account>();
        var lines = content.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length <= 1)
        {
            return result;
        }

        for (var i = 1; i < lines.Length; i++)
        {
            var row = SplitCsvLine(lines[i]);
            if (row.Count < 4)
            {
                continue;
            }

            var id = Guid.TryParse(row[0], out var parsedId) ? parsedId : Guid.NewGuid();
            var issuer = row[1];
            var accountName = string.IsNullOrWhiteSpace(row[2]) ? null : row[2];
            var secret = row[3];
            result.Add(new Account(id, issuer, secret, accountName));
        }

        return result;
    }

    private static List<string> SplitCsvLine(string line)
    {
        var values = new List<string>();
        var current = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            var c = line[i];
            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                    continue;
                }

                inQuotes = !inQuotes;
                continue;
            }

            if (c == ',' && !inQuotes)
            {
                values.Add(current.ToString());
                current.Clear();
                continue;
            }

            current.Append(c);
        }

        values.Add(current.ToString());
        return values;
    }

    private static bool TryGetUnencryptedFormat(string extension, out ExportFileFormat format)
    {
        if (extension.Equals(".json", StringComparison.OrdinalIgnoreCase))
        {
            format = ExportFileFormat.Json;
            return true;
        }

        if (extension.Equals(".txt", StringComparison.OrdinalIgnoreCase))
        {
            format = ExportFileFormat.Txt;
            return true;
        }

        if (extension.Equals(".csv", StringComparison.OrdinalIgnoreCase))
        {
            format = ExportFileFormat.Csv;
            return true;
        }

        format = ExportFileFormat.Json;
        return false;
    }

}
