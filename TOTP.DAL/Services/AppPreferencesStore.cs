using FluentResults;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using TOTP.Core.Models;
using TOTP.Core.Security.Interfaces;
using TOTP.Core.Services.Interfaces;

namespace TOTP.DAL.Services;

public sealed class AppPreferencesStore : IAppPreferencesStore
{
    private readonly string _path;
    private readonly ILogger<AppPreferencesStore> _logger;
    private readonly IPlatformFileSecurity _fileSecurity;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public AppPreferencesStore(
        string storageFilePath,
        ILogger<AppPreferencesStore> logger,
        IPlatformFileSecurity fileSecurity)
    {
        if (string.IsNullOrWhiteSpace(storageFilePath))
            throw new ArgumentException("Path required.", nameof(storageFilePath));

        _path = Path.GetFullPath(Environment.ExpandEnvironmentVariables(storageFilePath));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _fileSecurity = fileSecurity ?? throw new ArgumentNullException(nameof(fileSecurity));
    }

    public async Task<Result<AppPreferencesV1?>> LoadAsync(CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (!File.Exists(_path)) return Result.Ok<AppPreferencesV1?>(null);
            SecureStorageDirectory();
            _fileSecurity.RestrictFileToCurrentUser(_path);

            var payload = new byte[AppPreferencesV1Codec.MaximumPayloadSize + 1];
            try
            {
                var length = await ReadBoundedAsync(payload, cancellationToken);
                if (length > AppPreferencesV1Codec.MaximumPayloadSize)
                {
                    return Result.Fail<AppPreferencesV1?>(new AppPreferencesError(
                        AppPreferencesErrorCode.TooLarge,
                        "Preferences exceed the size limit."));
                }

                var decoded = AppPreferencesV1Codec.Deserialize(payload.AsMemory(0, length));
                return decoded.IsSuccess
                    ? Result.Ok<AppPreferencesV1?>(decoded.Value)
                    : Result.Fail<AppPreferencesV1?>(decoded.Errors);
            }
            finally
            {
                CryptographicOperations.ZeroMemory(payload);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load preferences.");
            var code = ex is UnauthorizedAccessException
                ? AppPreferencesErrorCode.ReadAccessDenied
                : AppPreferencesErrorCode.ReadFailed;
            return Result.Fail<AppPreferencesV1?>(new AppPreferencesError(code, "Failed to load preferences.", ex));
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<Result> SaveAsync(
        AppPreferencesV1 preferences,
        CancellationToken cancellationToken = default)
    {
        var encoded = AppPreferencesV1Codec.Serialize(preferences);
        if (encoded.IsFailed) return Result.Fail(encoded.Errors);

        var lockTaken = false;
        try
        {
            await _lock.WaitAsync(cancellationToken);
            lockTaken = true;
            if (Directory.Exists(_path))
                throw new UnauthorizedAccessException("The preferences path refers to a directory.");

            SecureStorageDirectory();
            var tempPath = $"{_path}.{Guid.NewGuid():N}.tmp";
            try
            {
                await File.WriteAllBytesAsync(tempPath, encoded.Value, cancellationToken);
                _fileSecurity.RestrictFileToCurrentUser(tempPath);
                File.Move(tempPath, _path, overwrite: true);
            }
            finally
            {
                TryDeleteTemporaryFile(tempPath);
            }

            return Result.Ok();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save preferences.");
            var code = ex is UnauthorizedAccessException
                ? AppPreferencesErrorCode.WriteAccessDenied
                : AppPreferencesErrorCode.WriteFailed;
            return Result.Fail(new AppPreferencesError(code, "Failed to save preferences.", ex));
        }
        finally
        {
            if (lockTaken) _lock.Release();
        }
    }

    public void Dispose() => _lock.Dispose();

    private async Task<int> ReadBoundedAsync(byte[] buffer, CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(
            _path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 4096,
            useAsync: true);
        var total = 0;
        while (total < buffer.Length)
        {
            var read = await stream.ReadAsync(buffer.AsMemory(total), cancellationToken);
            if (read == 0) break;
            total += read;
        }

        return total;
    }

    private void SecureStorageDirectory()
    {
        var directory = Path.GetDirectoryName(_path)
            ?? throw new DirectoryNotFoundException("The preferences directory is unavailable.");
        Directory.CreateDirectory(directory);
        _fileSecurity.RestrictDirectoryToCurrentUser(directory);
    }

    private static void TryDeleteTemporaryFile(string path)
    {
        try
        {
            if (File.Exists(path)) File.Delete(path);
        }
        catch
        {
            // Best effort after the primary operation has completed or failed.
        }
    }
}
