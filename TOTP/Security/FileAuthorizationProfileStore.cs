using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace TOTP.Security;

public sealed class FileAuthorizationProfileStore : IAuthorizationProfileStore
{
    private readonly string _path;

    public FileAuthorizationProfileStore(string baseDir)
    {
        Directory.CreateDirectory(baseDir);
        _path = Path.Combine(baseDir, "auth.profile");
    }

    public async Task<AuthorizationProfile?> LoadAsync()
    {
        if (!File.Exists(_path))
            return null;

        var encrypted = await File.ReadAllBytesAsync(_path).ConfigureAwait(false);
        if (encrypted.Length == 0)
            return null;

        var decrypted = ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);
        var json = Encoding.UTF8.GetString(decrypted);

        return JsonSerializer.Deserialize<AuthorizationProfile>(json);
    }

    public async Task SaveAsync(AuthorizationProfile profile)
    {
        var json = JsonSerializer.Serialize(profile, new JsonSerializerOptions { WriteIndented = true });
        var bytes = Encoding.UTF8.GetBytes(json);

        var encrypted = ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser);
        await File.WriteAllBytesAsync(_path, encrypted).ConfigureAwait(false);
    }

    public Task ClearAsync()
    {
        if (File.Exists(_path))
            File.Delete(_path);

        return Task.CompletedTask;
    }
}