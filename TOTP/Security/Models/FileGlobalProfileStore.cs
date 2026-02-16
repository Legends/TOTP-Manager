using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TOTP.Security.Interfaces;

namespace TOTP.Security.Models;

public sealed class FileGlobalProfileStore : IGlobalProfileStore
{
    private readonly string _path;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public FileGlobalProfileStore(string storageFilePath)
    {
        if (string.IsNullOrWhiteSpace(storageFilePath))
            throw new ArgumentException("Profile storage path must be provided.", nameof(storageFilePath));

        _path = storageFilePath;
        var directory = Path.GetDirectoryName(_path);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);
    }

    public async Task<GlobalProfile?> LoadAsync()
    {
        await _lock.WaitAsync();//.ConfigureAwait(false);
        try
        {
            if (!File.Exists(_path))
                return null;

            var encrypted = await File.ReadAllBytesAsync(_path);//.ConfigureAwait(false);
            if (encrypted.Length == 0)
                return null;

            var decrypted = ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);
            var json = Encoding.UTF8.GetString(decrypted);

            try
            {
                var globalProfile = JsonSerializer.Deserialize<GlobalProfile>(json);
                if (globalProfile is not null)
                    return globalProfile;
            }
            catch (JsonException)
            {
                // ignored - fall back to legacy profile format
            }

            var legacyProfile = JsonSerializer.Deserialize<AuthorizationProfile>(json);
            if (legacyProfile is null)
                return null;

            return new GlobalProfile { Authorization = legacyProfile };
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task SaveAsync(GlobalProfile profile)
    {
        await _lock.WaitAsync();//.ConfigureAwait(false);
        try
        {
            var json = JsonSerializer.Serialize(profile, new JsonSerializerOptions { WriteIndented = true });
            var bytes = Encoding.UTF8.GetBytes(json);

            var encrypted = ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser);
            await File.WriteAllBytesAsync(_path, encrypted);//.ConfigureAwait(false);
        }
        finally
        {
            _lock.Release();
        }
    }
}
