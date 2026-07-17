using Microsoft.Extensions.Logging.Abstractions;
using TOTP.Core.Models;
using TOTP.DAL.Services;
using TOTP.Tests.Common;
using TOTP.Tests.Models;

namespace TOTP.Tests.Integration;

public sealed class AppPreferencesStoreTests
{
    [Fact]
    public async Task LoadAsync_WhenMissing_ReturnsSuccessfulNull()
    {
        using var temp = new TempDir();
        using var store = CreateStore(Path.Combine(temp.Path, "preferences.json"));

        var result = await store.LoadAsync(TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task SaveThenLoad_RoundTripsPreferencesAsJson()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var temp = new TempDir();
        var path = Path.Combine(temp.Path, "preferences.json");
        using var store = CreateStore(path);
        var preferences = AppPreferencesV1CodecTests.CreatePreferences();

        var saved = await store.SaveAsync(preferences, cancellationToken);
        var loaded = await store.LoadAsync(cancellationToken);

        Assert.True(saved.IsSuccess);
        Assert.True(loaded.IsSuccess);
        Assert.Equal(preferences, loaded.Value);
        Assert.StartsWith("{", (await File.ReadAllTextAsync(path, cancellationToken)).TrimStart(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task LoadAsync_WhenOversized_ReturnsTypedFailure()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var temp = new TempDir();
        var path = Path.Combine(temp.Path, "preferences.json");
        await File.WriteAllBytesAsync(path, new byte[AppPreferencesV1Codec.MaximumPayloadSize + 1], cancellationToken);
        using var store = CreateStore(path);

        var result = await store.LoadAsync(cancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(AppPreferencesErrorCode.TooLarge, AppPreferencesV1CodecTests.ErrorCode(result.Errors));
    }

    private static AppPreferencesStore CreateStore(string path) =>
        new(path, NullLogger<AppPreferencesStore>.Instance, NoOpPlatformFileSecurity.Instance);

    private sealed class TempDir : IDisposable
    {
        public TempDir()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"totp-preferences-tests-{Guid.NewGuid():N}");
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(Path)) Directory.Delete(Path, recursive: true);
            }
            catch
            {
                // Best-effort test cleanup.
            }
        }
    }
}
