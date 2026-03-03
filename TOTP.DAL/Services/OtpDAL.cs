using FluentResults;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using TOTP.Core.Common;
using TOTP.Core.Enums;
using TOTP.Core.Models;
using TOTP.Core.Security.Interfaces;
using TOTP.Core.Services.Interfaces;


namespace TOTP.DAL.Services;

public sealed class OtpDAL : IOtpDAL
{
    private readonly string _secretsPath;
    private readonly IVaultService _vaultService;
    private readonly ILogger<OtpDAL> _logger;
    private static readonly SemaphoreSlim Semaphore = new(1, 1);

    public OtpDAL(ILogger<OtpDAL> logger, IVaultService vaultService, string? storageFilePath = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _vaultService = vaultService ?? throw new ArgumentNullException(nameof(vaultService));
        _secretsPath = storageFilePath ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TOTP-Manager", "master.totp");

        var directory = Path.GetDirectoryName(_secretsPath);
        if (directory != null && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    public async Task<Result<List<OtpEntry>>> GetAllAsync()
    {
        await Semaphore.WaitAsync();
        try
        {
            if (!File.Exists(_secretsPath)) return Result.Ok<List<OtpEntry>>(new ());
            byte[] blob = await File.ReadAllBytesAsync(_secretsPath);
            return Result.Ok(_vaultService.DecryptVault(blob));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load accounts.");
            return Result.Fail(new StatusError(OperationStatus.LoadingFailed));
        }
        finally { Semaphore.Release(); }
    }

    public async Task<Result> ExportEncryptedAsync(string targetPath)
    {
        await Semaphore.WaitAsync();
        try
        {
            var data = await GetAllInternalAsync();
            byte[] blob = _vaultService.EncryptVault(data);
            await File.WriteAllBytesAsync(targetPath, blob);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Export failed to {Path}", targetPath);
            return Result.Fail("Encrypted export failed.");
        }
        finally { Semaphore.Release(); }
    }

    private async Task<List<OtpEntry>> GetAllInternalAsync()
    {
        if (!File.Exists(_secretsPath)) return [];
        byte[] blob = await File.ReadAllBytesAsync(_secretsPath);
        return _vaultService.DecryptVault(blob);
    }

    public async Task<Result> AddNewAsync(OtpEntry newItem) => await ExecuteWriteAsync(list => list.Add(newItem));
    public async Task<Result> UpdateAsync(OtpEntry updated) => await ExecuteWriteAsync(list => {
        var idx = list.FindIndex(x => x.ID == updated.ID);
        if (idx != -1) list[idx] = updated;
    });
    public async Task<Result> DeleteAsync(OtpEntry account) => await ExecuteWriteAsync(list => list.RemoveAll(x => x.ID == account.ID));

    private async Task<Result> ExecuteWriteAsync(Action<List<OtpEntry>> action)
    {
        await Semaphore.WaitAsync();
        try
        {
            var list = await GetAllInternalAsync();
            action(list);
            byte[] blob = _vaultService.EncryptVault(list);

            // ATOMIC WRITE PATTERN: Prevents file corruption
            string tempPath = _secretsPath + ".tmp";
            await File.WriteAllBytesAsync(tempPath, blob);
            File.Move(tempPath, _secretsPath, overwrite: true);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Storage operation failed.");
            return Result.Fail(new StatusError(OperationStatus.StorageFailed));
        }
        finally { Semaphore.Release(); }
    }

    public async Task<Result> ReEncryptStorageAsync() => await ExportEncryptedAsync(_secretsPath);

    #region ### BACKUP & DISPOSE ###

    public async Task<Result> BackupOtpEntriesStorageFileAsync()
    {
        return await Task.Run(() =>
        {
            try
            {
                if (!File.Exists(_secretsPath)) return Result.Ok();

                var dir = Path.GetDirectoryName(_secretsPath)!;
                var fileName = Path.GetFileName(_secretsPath);

                string latestBackupPath = Path.Combine(dir, $"{fileName}.bak1");

                // Optimization: Only backup if the file has actually changed
                if (File.Exists(latestBackupPath))
                {
                    if (AreFilesIdentical(_secretsPath, latestBackupPath))
                    {
                        _logger.LogInformation("Backup skipped: No changes detected in storage file.");
                        return Result.Ok();
                    }
                }

                for (var i = 5; i >= 1; i--)
                {
                    var oldBackup = Path.Combine(dir, $"{fileName}.bak{i}");
                    var nextBackup = Path.Combine(dir, $"{fileName}.bak{i + 1}");

                    if (File.Exists(oldBackup))
                    {
                        if (i == 5) File.Delete(oldBackup);
                        else File.Move(oldBackup, nextBackup, true);
                    }
                }

                File.Copy(_secretsPath, Path.Combine(dir, $"{fileName}.bak1"), true);
                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, nameof(BackupOtpEntriesStorageFileAsync));
                return Result.Fail(new StatusError(OperationStatus.StorageFailed));
            }
        });
    }

    /// <summary>
    /// Compares two files using SHA256 to determine if a backup is actually necessary.
    /// </summary>
    private bool AreFilesIdentical(string path1, string path2)
    {
        using var hashAlgorithm = SHA256.Create();

        using var stream1 = File.OpenRead(path1);
        using var stream2 = File.OpenRead(path2);

        byte[] hash1 = hashAlgorithm.ComputeHash(stream1);
        byte[] hash2 = hashAlgorithm.ComputeHash(stream2);

        return CryptographicOperations.FixedTimeEquals(hash1, hash2);
    }

    public void Dispose()
    {
        Semaphore.Dispose();
    }

    #endregion
}