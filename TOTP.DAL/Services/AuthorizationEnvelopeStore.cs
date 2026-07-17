using System.Security.Cryptography;
using FluentResults;
using Microsoft.Extensions.Logging;
using TOTP.Core.Security;
using TOTP.Core.Security.Interfaces;
using TOTP.Core.Security.Models;
using TOTP.Core.Services.Interfaces;

namespace TOTP.DAL.Services;

public sealed class AuthorizationEnvelopeStore : IAuthorizationEnvelopeStore
{
    private readonly string _path;
    private readonly ILogger<AuthorizationEnvelopeStore> _logger;
    private readonly IPlatformFileSecurity _fileSecurity;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public AuthorizationEnvelopeStore(
        string storageFilePath,
        ILogger<AuthorizationEnvelopeStore> logger,
        IPlatformFileSecurity fileSecurity)
    {
        if (string.IsNullOrWhiteSpace(storageFilePath))
            throw new ArgumentException("Path required.", nameof(storageFilePath));

        _path = Path.GetFullPath(Environment.ExpandEnvironmentVariables(storageFilePath));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _fileSecurity = fileSecurity ?? throw new ArgumentNullException(nameof(fileSecurity));
    }

    public async Task<Result<AuthorizationEnvelopeV2?>> LoadAsync(CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (!File.Exists(_path)) return Result.Ok<AuthorizationEnvelopeV2?>(null);

            SecureStorageDirectory();
            _fileSecurity.RestrictFileToCurrentUser(_path);
            var payload = new byte[AuthorizationEnvelopeV2Codec.MaximumPayloadSize + 1];
            try
            {
                var length = await ReadBoundedAsync(payload, cancellationToken);
                if (length > AuthorizationEnvelopeV2Codec.MaximumPayloadSize)
                {
                    return Result.Fail<AuthorizationEnvelopeV2?>(new AuthorizationEnvelopeError(
                        AuthorizationEnvelopeErrorCode.TooLarge,
                        "Authorization envelope exceeds the size limit."));
                }

                var decoded = AuthorizationEnvelopeV2Codec.Deserialize(payload.AsMemory(0, length));
                return decoded.IsSuccess
                    ? Result.Ok<AuthorizationEnvelopeV2?>(decoded.Value)
                    : Result.Fail<AuthorizationEnvelopeV2?>(decoded.Errors);
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
            _logger.LogError(ex, "Failed to load the authorization envelope.");
            var code = ex is UnauthorizedAccessException
                ? AuthorizationEnvelopeErrorCode.ReadAccessDenied
                : AuthorizationEnvelopeErrorCode.ReadFailed;
            return Result.Fail<AuthorizationEnvelopeV2?>(new AuthorizationEnvelopeError(
                code,
                "Failed to load the authorization envelope.",
                ex));
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<Result> SaveAsync(
        AuthorizationEnvelopeV2 envelope,
        CancellationToken cancellationToken = default)
    {
        var encoded = AuthorizationEnvelopeV2Codec.Serialize(envelope);
        if (encoded.IsFailed) return Result.Fail(encoded.Errors);

        var lockTaken = false;
        try
        {
            await _lock.WaitAsync(cancellationToken);
            lockTaken = true;
            if (Directory.Exists(_path))
                throw new UnauthorizedAccessException("The authorization-envelope path refers to a directory.");

            SecureStorageDirectory();
            var tempPath = $"{_path}.{Guid.NewGuid():N}.tmp";
            try
            {
                await using (var stream = new FileStream(
                    tempPath,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None,
                    bufferSize: 4096,
                    useAsync: true))
                {
                    await stream.WriteAsync(encoded.Value, cancellationToken);
                }

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
            _logger.LogError(ex, "Failed to save the authorization envelope.");
            var code = ex is UnauthorizedAccessException
                ? AuthorizationEnvelopeErrorCode.WriteAccessDenied
                : AuthorizationEnvelopeErrorCode.WriteFailed;
            return Result.Fail(new AuthorizationEnvelopeError(code, "Failed to save the authorization envelope.", ex));
        }
        finally
        {
            CryptographicOperations.ZeroMemory(encoded.Value);
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
            ?? throw new DirectoryNotFoundException("The authorization-envelope directory is unavailable.");
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
