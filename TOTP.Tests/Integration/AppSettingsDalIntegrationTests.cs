using Microsoft.Extensions.Logging.Abstractions;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using TOTP.Core.Common;
using TOTP.Core.Enums;
using TOTP.Core.Models;
using TOTP.Core.Security.Models;
using TOTP.DAL.Services;

namespace TOTP.Tests.Integration;

public sealed class AppSettingsDalIntegrationTests
{
    [Fact]
    public async Task LoadAsync_WhenFileMissing_ReturnsNullSettings()
    {
        using var temp = new TempDir();
        var path = Path.Combine(temp.Path, "settings.totp");
        var sut = new AppSettingsDAL(path, NullLogger<AppSettingsDAL>.Instance);

        var result = await sut.LoadAsync();

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task SaveAsync_ThenLoadAsync_RoundTripsSettings()
    {
        using var temp = new TempDir();
        var path = Path.Combine(temp.Path, "settings.totp");
        var sut = new AppSettingsDAL(path, NullLogger<AppSettingsDAL>.Instance);
        var input = new AppSettings
        {
            MinimumLogLevel = AppLogLevel.Warning,
            IdleTimeout = TimeSpan.FromMinutes(3),
            LockOnSessionLock = false,
            LockOnMinimize = false,
            ClearClipboardEnabled = true,
            ClearClipboardSeconds = 9,
            QrPreviewScaleFactor = 2.5,
            ExportEncrypt = false,
            OpenExportFileAfterExport = true,
            HideSecretsByDefault = false,
            Authorization = new AuthorizationProfile
            {
                Gate = AuthorizationGateKind.Password,
                PasswordSalt = [1, 2, 3],
                PasswordWrappedDek = [4, 5, 6],
                DekNonce = [7, 8, 9],
                ArgonIterations = 4,
                ArgonMemorySize = 1024
            }
        };

        var saveResult = await sut.SaveAsync(input);
        var loadResult = await sut.LoadAsync();

        Assert.True(saveResult.IsSuccess);
        Assert.True(loadResult.IsSuccess);

        var loaded = Assert.IsType<AppSettings>(loadResult.Value);
        Assert.Equal(AppLogLevel.Warning, loaded.MinimumLogLevel);
        Assert.Equal(TimeSpan.FromMinutes(3), loaded.IdleTimeout);
        Assert.False(loaded.LockOnSessionLock);
        Assert.False(loaded.LockOnMinimize);
        Assert.True(loaded.ClearClipboardEnabled);
        Assert.Equal(9, loaded.ClearClipboardSeconds);
        Assert.Equal(2.5, loaded.QrPreviewScaleFactor);
        Assert.False(loaded.ExportEncrypt);
        Assert.True(loaded.OpenExportFileAfterExport);
        Assert.False(loaded.HideSecretsByDefault);
        Assert.Equal(AuthorizationGateKind.Password, loaded.Authorization.Gate);
        Assert.Equal([1, 2, 3], loaded.Authorization.PasswordSalt);
        Assert.Equal([4, 5, 6], loaded.Authorization.PasswordWrappedDek);
        Assert.Equal([7, 8, 9], loaded.Authorization.DekNonce);
    }

    [Fact]
    public async Task LoadAsync_WhenLegacyAuthorizationProfileStored_MapsToAppSettings()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var temp = new TempDir();
        var path = Path.Combine(temp.Path, "settings.totp");
        var sut = new AppSettingsDAL(path, NullLogger<AppSettingsDAL>.Instance);

        // "Authorization" as number breaks AppSettings deserialization (expects object),
        // while AuthorizationProfile can still deserialize Gate/PasswordSalt from same payload.
        var json = Encoding.UTF8.GetBytes("""{"Authorization":5,"Gate":1,"PasswordSalt":"CQgH"}""");
        var encrypted = ProtectedData.Protect(json, null, DataProtectionScope.CurrentUser);
        await File.WriteAllBytesAsync(path, encrypted, cancellationToken);

        var result = await sut.LoadAsync();

        Assert.True(result.IsSuccess);
        var loaded = Assert.IsType<AppSettings>(result.Value);
        Assert.Equal(AuthorizationGateKind.Hello, loaded.Authorization.Gate);
        Assert.Equal([9, 8, 7], loaded.Authorization.PasswordSalt);
    }

    [Fact]
    public async Task LoadAsync_WhenEncryptedBlobIsInvalid_ReturnsDecryptError()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var temp = new TempDir();
        var path = Path.Combine(temp.Path, "settings.totp");
        var sut = new AppSettingsDAL(path, NullLogger<AppSettingsDAL>.Instance);

        await File.WriteAllBytesAsync(path, Encoding.UTF8.GetBytes("not-dpapi"), cancellationToken);

        var result = await sut.LoadAsync();

        Assert.False(result.IsSuccess);
        Assert.Equal(AppErrorCode.AppSettingsDecryptFailed, result.GetErrorCode());
    }

    [Fact]
    public async Task LoadAsync_WhenDecryptedJsonIsInvalid_ReturnsDeserializeError()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var temp = new TempDir();
        var path = Path.Combine(temp.Path, "settings.totp");
        var sut = new AppSettingsDAL(path, NullLogger<AppSettingsDAL>.Instance);

        var encrypted = ProtectedData.Protect(Encoding.UTF8.GetBytes("{not-valid-json"), null, DataProtectionScope.CurrentUser);
        await File.WriteAllBytesAsync(path, encrypted, cancellationToken);

        var result = await sut.LoadAsync();

        Assert.False(result.IsSuccess);
        Assert.Equal(AppErrorCode.AppSettingsDeserializeFailed, result.GetErrorCode());
    }

    [Fact]
    public async Task SaveAsync_WhenPathIsDirectory_ReturnsSaveAccessDeniedError()
    {
        using var temp = new TempDir();
        var directoryPath = Path.Combine(temp.Path, "as-directory");
        Directory.CreateDirectory(directoryPath);
        var sut = new AppSettingsDAL(directoryPath, NullLogger<AppSettingsDAL>.Instance);

        var result = await sut.SaveAsync(new AppSettings());

        Assert.False(result.IsSuccess);
        Assert.Equal(AppErrorCode.AppSettingsSaveAccessDenied, result.GetErrorCode());
    }

    private sealed class TempDir : IDisposable
    {
        public TempDir()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "totp-tests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(Path))
                {
                    Directory.Delete(Path, recursive: true);
                }
            }
            catch
            {
                // best-effort test cleanup
            }
        }
    }
}
