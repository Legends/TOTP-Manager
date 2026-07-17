using FluentResults;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using TOTP.Core.Common;
using TOTP.Core.Models;
using TOTP.Core.Security.Interfaces;
using TOTP.Core.Services.Interfaces;
using TOTP.DAL.Common;

namespace TOTP.DAL.Services;

public sealed class AccountDAL : IAccountDAL
{
    private readonly string _secretsPath;
    private readonly IVaultService _vaultService;
    private readonly ILogger<AccountDAL> _logger;
    private readonly IPlatformFileSecurity _fileSecurity;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public AccountDAL(
        ILogger<AccountDAL> logger,
        IVaultService vaultService,
        string storageFilePath,
        IPlatformFileSecurity fileSecurity)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _vaultService = vaultService ?? throw new ArgumentNullException(nameof(vaultService));
        _fileSecurity = fileSecurity ?? throw new ArgumentNullException(nameof(fileSecurity));

        if (string.IsNullOrWhiteSpace(storageFilePath))
            throw new ArgumentException("Path required.", nameof(storageFilePath));

        _secretsPath = Path.GetFullPath(Environment.ExpandEnvironmentVariables(storageFilePath));
    }

    public async Task<Result<List<Account>>> GetAllAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            if (!File.Exists(_secretsPath))
            {
                return Result.Ok<List<Account>>(new());
            }

            SecureStorageDirectory();
            _fileSecurity.RestrictFileToCurrentUser(_secretsPath);
            byte[] blob = await File.ReadAllBytesAsync(_secretsPath);
            return Result.Ok(_vaultService.DecryptVault(blob));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load accounts.");
            return Result.Fail(AccountDalErrorMapper.MapReadError(ex));
        }
        finally { _semaphore.Release(); }
    }

    public async Task<Result> ExportEncryptedAsync(string targetPath)
    {
        await _semaphore.WaitAsync();
        try
        {
            targetPath = Path.GetFullPath(targetPath);
            var data = await GetAllInternalAsync();
            byte[] blob = _vaultService.EncryptVault(data);
            var targetDirectory = Path.GetDirectoryName(targetPath);
            if (string.IsNullOrWhiteSpace(targetDirectory) || !Directory.Exists(targetDirectory))
            {
                throw new DirectoryNotFoundException("The export directory was not found.");
            }

            var tempPath = CreateStagingPath(targetPath);
            try
            {
                await WriteStagingFileAsync(tempPath, blob);
                _fileSecurity.RestrictFileToCurrentUser(tempPath);
                File.Move(tempPath, targetPath, overwrite: true);
            }
            finally
            {
                TryDeleteTemporaryFile(tempPath);
            }

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Export failed to {Path}", targetPath);
            return Result.Fail(AccountDalErrorMapper.MapExportError(ex));
        }
        finally { _semaphore.Release(); }
    }

    private async Task<List<Account>> GetAllInternalAsync()
    {
        if (!File.Exists(_secretsPath))
        {
            return [];
        }

        SecureStorageDirectory();
        _fileSecurity.RestrictFileToCurrentUser(_secretsPath);
        byte[] blob = await File.ReadAllBytesAsync(_secretsPath);
        return _vaultService.DecryptVault(blob);
    }

    public async Task<Result> AddNewAsync(Account newItem) =>
        await ExecuteWriteAsync(list => list.Add(newItem), AppErrorCode.OtpCreateFailed, "Failed to create OTP entry.");

    public async Task<Result> UpdateAsync(Account updated) =>
        await ExecuteWriteAsync(list =>
        {
            var idx = list.FindIndex(x => x.ID == updated.ID);
            if (idx != -1)
            {
                list[idx] = updated;
            }
        }, AppErrorCode.OtpUpdateFailed, "Failed to update OTP entry.");

    public async Task<Result> DeleteAsync(Account account) =>
        await ExecuteWriteAsync(list => list.RemoveAll(x => x.ID == account.ID), AppErrorCode.OtpDeleteFailed, "Failed to delete OTP entry.");

    private async Task<Result> ExecuteWriteAsync(Action<List<Account>> action, AppErrorCode operationCode, string operationMessage)
    {
        await _semaphore.WaitAsync();
        try
        {
            var list = await GetAllInternalAsync();
            action(list);
            byte[] blob = _vaultService.EncryptVault(list);

            SecureStorageDirectory();
            string tempPath = CreateStagingPath(_secretsPath);
            try
            {
                await WriteStagingFileAsync(tempPath, blob);
                _fileSecurity.RestrictFileToCurrentUser(tempPath);
                File.Move(tempPath, _secretsPath, overwrite: true);
            }
            finally
            {
                TryDeleteTemporaryFile(tempPath);
            }

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Storage operation failed.");
            return Result.Fail(AccountDalErrorMapper.MapWriteError(ex, operationCode, operationMessage));
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

                SecureStorageDirectory();
                _fileSecurity.RestrictFileToCurrentUser(_secretsPath);

                if (File.Exists(latestBackupPath))
                {
                    _fileSecurity.RestrictFileToCurrentUser(latestBackupPath);
                    if (AreFilesIdentical(_secretsPath, latestBackupPath))
                    {
                        _logger.LogInformation("Backup skipped: No changes detected in storage file.");
                        return Result.Ok();
                    }
                }

                var stagedBackupPath = CreateStagingPath(Path.Combine(dir, $"{fileName}.bak"));
                try
                {
                    File.Copy(_secretsPath, stagedBackupPath, true);
                    _fileSecurity.RestrictFileToCurrentUser(stagedBackupPath);

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

                    File.Move(stagedBackupPath, latestBackupPath, true);
                }
                finally
                {
                    TryDeleteTemporaryFile(stagedBackupPath);
                }
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

    private void SecureStorageDirectory()
    {
        var directory = Path.GetDirectoryName(_secretsPath);
        if (string.IsNullOrWhiteSpace(directory))
        {
            throw new DirectoryNotFoundException("The OTP storage directory is unavailable.");
        }

        Directory.CreateDirectory(directory);
        _fileSecurity.RestrictDirectoryToCurrentUser(directory);
    }

    private static void TryDeleteTemporaryFile(string tempPath)
    {
        try
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
        catch
        {
            // Best effort after the primary failure has already been captured.
        }
    }

    private static string CreateStagingPath(string destinationPath) =>
        $"{destinationPath}.{Guid.NewGuid():N}.tmp";

    private static async Task WriteStagingFileAsync(string path, byte[] data)
    {
        await using var stream = new FileStream(
            path,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 4096,
            useAsync: true);
        await stream.WriteAsync(data);
    }
}
