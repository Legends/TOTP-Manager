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

namespace TOTP.DAL.Services;

public sealed class AppSettingsDAL : IAppSettingsDAL
{
    private readonly string _path;
    private readonly ILogger<AppSettingsDAL> _logger;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public AppSettingsDAL(string storageFilePath, ILogger<AppSettingsDAL> logger)
    {
        if (string.IsNullOrWhiteSpace(storageFilePath))
            throw new ArgumentException("Path required.", nameof(storageFilePath));

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        //_path = storageFilePath;

        _path = storageFilePath ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TOTP-Manager", "settings.totp");
        _path = Environment.ExpandEnvironmentVariables(_path);

        var directory = Path.GetDirectoryName(_path);
        if (directory != null && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
       
    }

    public async Task<Result<IAppSettings?>> LoadAsync()
    {
        await _lock.WaitAsync();
        try
        {
            if (!File.Exists(_path)) return Result.Ok<IAppSettings?>(null);

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
            catch (JsonException)
            {
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
            // 1. Serialize to JSON bytes
            var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(profile, _jsonOptions);

            // 2. Encrypt
            var encryptedBytes = ProtectedData.Protect(jsonBytes, null, DataProtectionScope.CurrentUser);

            // 3. Atomic Write
            await File.WriteAllBytesAsync(_path, encryptedBytes);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save app settings.");
            return Result.Fail(AppSettingsDalErrorMapper.MapSaveError(ex));
        }
        finally { _lock.Release(); }
    }
}
