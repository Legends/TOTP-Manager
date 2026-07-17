using FluentResults;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TOTP.Core.Common;
using TOTP.DAL.Common;
using TOTP.Core.Security.Interfaces;
using TOTP.Core.Security.Models;
using TOTP.Core.Models;
using TOTP.Core.Services.Interfaces;

namespace TOTP.DAL.Services;

public sealed class AppSettingsDAL : IAppSettingsDAL
{
    private readonly string _path;
    private readonly ILogger<AppSettingsDAL> _logger;
    private readonly IPlatformFileSecurity _fileSecurity;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public AppSettingsDAL(
        string storageFilePath,
        ILogger<AppSettingsDAL> logger,
        IPlatformFileSecurity fileSecurity)
    {
        if (string.IsNullOrWhiteSpace(storageFilePath))
            throw new ArgumentException("Path required.", nameof(storageFilePath));

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _fileSecurity = fileSecurity ?? throw new ArgumentNullException(nameof(fileSecurity));
        _path = Path.GetFullPath(Environment.ExpandEnvironmentVariables(storageFilePath));
    }

    public async Task<Result<IAppSettings?>> LoadAsync()
    {
        await _lock.WaitAsync();
        try
        {
            if (!File.Exists(_path)) return Result.Ok<IAppSettings?>(null);

            SecureStorageDirectory();
            _fileSecurity.RestrictFileToCurrentUser(_path);

            // 1. Read and Decrypt
            var encryptedBytes = await File.ReadAllBytesAsync(_path);
            if (encryptedBytes.Length == 0) return Result.Ok<IAppSettings?>(null);

            var decryptedBytes = ProtectedData.Unprotect(encryptedBytes, null, DataProtectionScope.CurrentUser);

            // 2. Deserialize (Using MemoryStream for efficiency)
            using var ms = new MemoryStream(decryptedBytes);

            try
            {
                return Result.Ok<IAppSettings?>(await JsonSerializer.DeserializeAsync<AppSettings>(ms));
            }
            catch (JsonException ex)
            {
                _logger.LogInformation(ex, "Settings payload is not AppSettings. Falling back to legacy AuthorizationProfile parsing.");
                // Fallback logic for legacy profiles
                ms.Position = 0;
                var legacy = await JsonSerializer.DeserializeAsync<AuthorizationProfile>(ms);
                return Result.Ok<IAppSettings?>(legacy != null ? new AppSettings { Authorization = legacy } : null);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load app settings.");
            return Result.Fail(AppSettingsDalErrorMapper.MapLoadError(ex));
        }
        finally { _lock.Release(); }
    }

    public async Task<Result> SaveAsync(IAppSettings profile)
    {
        await _lock.WaitAsync();
        try
        {
            if (Directory.Exists(_path))
            {
                throw new UnauthorizedAccessException("The app settings path refers to a directory.");
            }

            // 1. Serialize to JSON bytes
            var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(profile, _jsonOptions);

            // 2. Encrypt
            var encryptedBytes = ProtectedData.Protect(jsonBytes, null, DataProtectionScope.CurrentUser);

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
                    await stream.WriteAsync(encryptedBytes);
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save app settings.");
            return Result.Fail(AppSettingsDalErrorMapper.MapSaveError(ex));
        }
        finally { _lock.Release(); }
    }

    public void Dispose()
    {
        _lock.Dispose();
    }

    private void SecureStorageDirectory()
    {
        var directory = Path.GetDirectoryName(_path);
        if (string.IsNullOrWhiteSpace(directory))
        {
            throw new DirectoryNotFoundException("The app settings directory is unavailable.");
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
}
