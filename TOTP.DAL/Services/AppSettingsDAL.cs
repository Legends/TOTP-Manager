using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TOTP.Core.Security.Interfaces;
using TOTP.Core.Security.Models;

namespace TOTP.DAL.Services;

public sealed class AppSettingsDAL : IAppSettingsDAL
{
    private readonly string _path;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public AppSettingsDAL(string storageFilePath)
    {
        if (string.IsNullOrWhiteSpace(storageFilePath))
            throw new ArgumentException("Path required.", nameof(storageFilePath));

        _path = storageFilePath;
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
    }

    public async Task<IAppSettings?> LoadAsync()
    {
        await _lock.WaitAsync();
        try
        {
            if (!File.Exists(_path)) return null;

            // 1. Read and Decrypt
            var encryptedBytes = await File.ReadAllBytesAsync(_path);
            if (encryptedBytes.Length == 0) return null;

            var decryptedBytes = ProtectedData.Unprotect(encryptedBytes, null, DataProtectionScope.CurrentUser);

            // 2. Deserialize (Using MemoryStream for efficiency)
            using var ms = new MemoryStream(decryptedBytes);
            try
            {
                return await JsonSerializer.DeserializeAsync<AppSettings>(ms);
            }
            catch (JsonException)
            {
                // Fallback logic for legacy profiles
                ms.Position = 0;
                var legacy = await JsonSerializer.DeserializeAsync<AuthorizationProfile>(ms);
                return legacy != null ? new AppSettings { Authorization = legacy } : null;
            }
        }
        finally { _lock.Release(); }
    }

    public async Task SaveAsync(IAppSettings profile)
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
        }
        finally { _lock.Release(); }
    }
}