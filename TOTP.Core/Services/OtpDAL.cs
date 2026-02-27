using FluentResults;
using Microsoft.Extensions.Logging;
using NSec.Cryptography;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using TOTP.Core.Common;
using TOTP.Core.Enums;
using TOTP.Core.Models;
using TOTP.Core.Services.Interfaces;


namespace TOTP.Core.Services;

public sealed class OtpDAL : IOtpDAL
{
    private readonly string _secretsPath;
    private readonly ISecurityContext _securityContext;
    private readonly ILogger<OtpDAL> _logger;
    private static readonly SemaphoreSlim Semaphore = new(1, 1);
    private static readonly AeadAlgorithm _algorithm = AeadAlgorithm.Aes256Gcm;

    // Header to identify our specific vault format
    private static readonly byte[] FileHeader = "TVLT"u8.ToArray();

    public OtpDAL(ILogger<OtpDAL> logger, ISecurityContext securityContext, string? storageFilePath = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _securityContext = securityContext ?? throw new ArgumentNullException(nameof(securityContext));
        _secretsPath = storageFilePath ?? AlternativeStoragePath();
    }

    private static string AlternativeStoragePath() =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TOTP-Manager", "secrets.dat");

    #region ### PUBLIC INTERFACE METHODS ###

    public async Task<Result<List<OtpEntry>>> GetAllAsync()
    {
        await Semaphore.WaitAsync();
        try
        {
            var list = await LoadAccountsFromFileAsync();
            return Result.Ok(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Method}", nameof(GetAllAsync));
            return new StatusError(OperationStatus.LoadingFailed);
        }
        finally { Semaphore.Release(); }
    }

    public async Task<Result> AddNewAsync(OtpEntry newItem)
    {
        await Semaphore.WaitAsync();
        try
        {
            var list = await LoadAccountsFromFileAsync();

            if (list.Any(x => x.Issuer == newItem.Issuer && x.AccountName == newItem.AccountName))
                return new StatusError(OperationStatus.AlreadyExists);

            list.Add(newItem);

            await WriteEncryptedFileAsync(list);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Method}", nameof(AddNewAsync));
            return new StatusError(OperationStatus.StorageFailed);
        }
        finally { Semaphore.Release(); }
    }

    public async Task<Result> UpdateAsync(OtpEntry updated)
    {
        await Semaphore.WaitAsync();
        try
        {
            var list = await LoadAccountsFromFileAsync();
            var existing = list.FirstOrDefault(x => x.ID == updated.ID);

            if (existing == null)
                return new StatusError(OperationStatus.NotFound, $"{updated.Issuer} not found");

            list.Remove(existing);
            list.Add(updated);

            await WriteEncryptedFileAsync(list);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Method}", nameof(UpdateAsync));
            return new StatusError(OperationStatus.StorageFailed);
        }
        finally { Semaphore.Release(); }
    }

    public async Task<Result> DeleteAccountAsync(OtpEntry account)
    {
        await Semaphore.WaitAsync();
        try
        {
            var list = await LoadAccountsFromFileAsync();
            var item = list.FirstOrDefault(x => x.ID == account.ID);

            if (item == null)
                return new StatusError(OperationStatus.NotFound);

            list.Remove(item);

            await WriteEncryptedFileAsync(list);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Method}", nameof(DeleteAccountAsync));
            return new StatusError(OperationStatus.DeleteFailed);
        }
        finally { Semaphore.Release(); }
    }

    public async Task<Result> ReEncryptStorageAsync()
    {
        await Semaphore.WaitAsync();
        try
        {
            // Decrypts with current DEK and immediately re-encrypts.
            // Useful if the user switches authorization gates.
            var list = await LoadAccountsFromFileAsync();
            await WriteEncryptedFileAsync(list);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Method}", nameof(ReEncryptStorageAsync));
            return new StatusError(OperationStatus.StorageFailed);
        }
        finally { Semaphore.Release(); }
    }

    #endregion

    #region ### PRIVATE ENCRYPTION LOGIC ###

    private async Task<List<OtpEntry>> LoadAccountsFromFileAsync()
    {
        if (!File.Exists(_secretsPath) || new FileInfo(_secretsPath).Length == 0)
            return [];

        if (!_securityContext.IsUnlocked)
            throw new InvalidOperationException("Vault access denied: Locked.");

        byte[] fileContent = await File.ReadAllBytesAsync(_secretsPath);

        // Validation: [Header 4] + [Nonce 12] + [EncryptedData + Tag]
        int minRequiredSize = FileHeader.Length + _algorithm.NonceSize;
        if (fileContent.Length < minRequiredSize)
            throw new CryptographicException("Secrets file is corrupted or invalid size.");

        // Check "TVLT" Header
        if (!fileContent.Take(FileHeader.Length).SequenceEqual(FileHeader))
            throw new CryptographicException("Invalid file format detected.");

        byte[] nonce = fileContent.Skip(FileHeader.Length).Take(_algorithm.NonceSize).ToArray();
        byte[] ciphertext = fileContent.Skip(FileHeader.Length + _algorithm.NonceSize).ToArray();

        byte[] dek = _securityContext.GetDek();
        using var key = Key.Import(_algorithm, dek, KeyBlobFormat.RawSymmetricKey);

        byte[]? decrypted = _algorithm.Decrypt(key, nonce, null, ciphertext);

        if (decrypted == null)
            throw new CryptographicException("Authentication failed: Wrong key or data tampered.");

        try
        {
            var json = Encoding.UTF8.GetString(decrypted);
            return JsonSerializer.Deserialize<List<OtpEntry>>(json) ?? [];
        }
        finally
        {
            Array.Clear(decrypted, 0, decrypted.Length);
        }
    }

    private async Task WriteEncryptedFileAsync(List<OtpEntry> list)
    {
        if (!_securityContext.IsUnlocked)
            throw new InvalidOperationException("Cannot write to vault while locked.");

        var json = JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true });
        byte[] data = Encoding.UTF8.GetBytes(json);
        byte[] nonce = RandomNumberGenerator.GetBytes(_algorithm.NonceSize);

        byte[] dek = _securityContext.GetDek();
        using var key = Key.Import(_algorithm, dek, KeyBlobFormat.RawSymmetricKey);

        byte[] encrypted = _algorithm.Encrypt(key, nonce, null, data);

        // Build composite file
        using var ms = new MemoryStream();
        await ms.WriteAsync(FileHeader);
        await ms.WriteAsync(nonce);
        await ms.WriteAsync(encrypted);

        var directory = Path.GetDirectoryName(_secretsPath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        await File.WriteAllBytesAsync(_secretsPath, ms.ToArray());
    }

    #endregion

    #region ### BACKUP & DISPOSE ###

    public async Task<Result> BackupOtpEntriesStorageFileAsync()
    {
        return await Task.Run(() =>
        {
            try
            {
                if (!File.Exists(_secretsPath)) return Result.Ok();

                var dir = Path.GetDirectoryName(_secretsPath)!;
                var file = Path.GetFileName(_secretsPath);

                for (var i = 5; i >= 1; i--)
                {
                    var oldBackup = Path.Combine(dir, $"{file}.bak{i}");
                    var nextBackup = Path.Combine(dir, $"{file}.bak{i + 1}");

                    if (File.Exists(oldBackup))
                    {
                        if (i == 5) File.Delete(oldBackup);
                        else File.Move(oldBackup, nextBackup, true);
                    }
                }

                File.Copy(_secretsPath, Path.Combine(dir, $"{file}.bak1"), true);
                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, nameof(BackupOtpEntriesStorageFileAsync));
                return Result.Fail(new StatusError(OperationStatus.StorageFailed));
            }
        });
    }

    public void Dispose()
    {
        Semaphore.Dispose();
    }

    #endregion
}