using FluentResults;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using TOTP.Core.Common;
using TOTP.Core.Models;
using TOTP.Core.Security.Interfaces;
using TOTP.Core.Services.Interfaces;
using TOTP.DAL.Common;

namespace TOTP.DAL.Services;

public sealed class OtpDAL : IOtpDAL
{
    private readonly string _secretsPath;
    private readonly IVaultService _vaultService;
    private readonly ILogger<OtpDAL> _logger;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public OtpDAL(ILogger<OtpDAL> logger, IVaultService vaultService, string? storageFilePath = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _vaultService = vaultService ?? throw new ArgumentNullException(nameof(vaultService));

        _secretsPath = storageFilePath ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TOTP-Manager", "master.totp");
        _secretsPath = Environment.ExpandEnvironmentVariables(_secretsPath);

        var directory = Path.GetDirectoryName(_secretsPath);
        if (directory != null && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    public async Task<Result<List<OtpEntry>>> GetAllAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            if (!File.Exists(_secretsPath))
            {
                return Result.Ok<List<OtpEntry>>(new());
            }

            byte[] blob = await File.ReadAllBytesAsync(_secretsPath);
            return Result.Ok(_vaultService.DecryptVault(blob));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load accounts.");
            return Result.Fail(OtpDalErrorMapper.MapReadError(ex));
        }
        finally { _semaphore.Release(); }
    }

    public async Task<Result> ExportEncryptedAsync(string targetPath)
    {
        await _semaphore.WaitAsync();
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
            return Result.Fail(OtpDalErrorMapper.MapExportError(ex));
        }
        finally { _semaphore.Release(); }
    }

    private async Task<List<OtpEntry>> GetAllInternalAsync()
    {
        if (!File.Exists(_secretsPath))
        {
            return [];
        }

        byte[] blob = await File.ReadAllBytesAsync(_secretsPath);
        return _vaultService.DecryptVault(blob);
    }

    public async Task<Result> AddNewAsync(OtpEntry newItem) =>
        await ExecuteWriteAsync(list => list.Add(newItem), AppErrorCode.OtpCreateFailed, "Failed to create OTP entry.");

    public async Task<Result> UpdateAsync(OtpEntry updated) =>
        await ExecuteWriteAsync(list =>
        {
            var idx = list.FindIndex(x => x.ID == updated.ID);
            if (idx != -1)
            {
                list[idx] = updated;
            }
        }, AppErrorCode.OtpUpdateFailed, "Failed to update OTP entry.");

    public async Task<Result> DeleteAsync(OtpEntry token) =>
        await ExecuteWriteAsync(list => list.RemoveAll(x => x.ID == token.ID), AppErrorCode.OtpDeleteFailed, "Failed to delete OTP entry.");

    private async Task<Result> ExecuteWriteAsync(Action<List<OtpEntry>> action, AppErrorCode operationCode, string operationMessage)
    {
        await _semaphore.WaitAsync();
        try
        {
            var list = await GetAllInternalAsync();
            action(list);
            byte[] blob = _vaultService.EncryptVault(list);

            string tempPath = _secretsPath + ".tmp";
            await File.WriteAllBytesAsync(tempPath, blob);
            File.Move(tempPath, _secretsPath, overwrite: true);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Storage operation failed.");
            return Result.Fail(OtpDalErrorMapper.MapWriteError(ex, operationCode, operationMessage));
        }
        finally { _semaphore.Release(); }
    }

    public async Task<Result> ReEncryptStorageAsync() => await ExportEncryptedAsync(_secretsPath);

    public async Task<Result> BackupOtpEntriesStorageFileAsync()
    {
        return await Task.Run(() =>
        {
            try
            {
                if (!File.Exists(_secretsPath))
                {
                    return Result.Ok();
                }

                var dir = Path.GetDirectoryName(_secretsPath)!;
                var fileName = Path.GetFileName(_secretsPath);
                string latestBackupPath = Path.Combine(dir, $"{fileName}.bak1");

                if (File.Exists(latestBackupPath) && AreFilesIdentical(_secretsPath, latestBackupPath))
                {
                    _logger.LogInformation("Backup skipped: No changes detected in storage file.");
                    return Result.Ok();
                }

                for (var i = 5; i >= 1; i--)
                {
                    var oldBackup = Path.Combine(dir, $"{fileName}.bak{i}");
                    var nextBackup = Path.Combine(dir, $"{fileName}.bak{i + 1}");

                    if (!File.Exists(oldBackup))
                    {
                        continue;
                    }

                    if (i == 5)
                    {
                        File.Delete(oldBackup);
                    }
                    else
                    {
                        File.Move(oldBackup, nextBackup, true);
                    }
                }

                File.Copy(_secretsPath, Path.Combine(dir, $"{fileName}.bak1"), true);
                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, nameof(BackupOtpEntriesStorageFileAsync));
                return Result.Fail(new AppError(AppErrorCode.OtpStorageBackupFailed, "Failed to create OTP storage backup.", ex));
            }
        });
    }

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
        _semaphore.Dispose();
    }
}
