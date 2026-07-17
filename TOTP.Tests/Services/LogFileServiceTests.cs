using TOTP.Services;
using TOTP.Tests.Common;
using TOTP.Infrastructure.Platform;

namespace TOTP.Tests.Services;

[Collection(NonParallelCollectionDefinition.NonParallel)]
public sealed class LogFileServiceTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"totp-log-tests-{Guid.NewGuid():N}");
    private readonly WindowsApplicationPaths _paths;
    private readonly LogFileService _sut;
    private readonly string _rollingPath;

    public LogFileServiceTests()
    {
        _paths = new WindowsApplicationPaths(_root, _root);
        _sut = new LogFileService(_paths);
        _rollingPath = Path.Combine(_paths.LogDirectory, $"app{DateTime.Now:yyyyMMdd}.log");
        Directory.CreateDirectory(_paths.LogDirectory);
    }

    [Fact]
    public void ResolveLogFilePath_WhenCurrentRollingExists_PrefersCurrentRolling()
    {
        File.WriteAllText(_rollingPath, "current");
        File.WriteAllText(Path.Combine(_paths.LogDirectory, "app20000101.log"), "older");

        var resolved = ResolveLogFilePathViaReflection();

        Assert.Equal(_rollingPath, resolved);
    }

    [Fact]
    public void ResolveLogFilePath_WhenCurrentMissing_UsesLatestRolling()
    {
        var oldPath = Path.Combine(_paths.LogDirectory, "app20000101.log");
        var newPath = Path.Combine(_paths.LogDirectory, "app20000102.log");
        File.WriteAllText(oldPath, "old");
        File.WriteAllText(newPath, "new");
        File.SetLastWriteTimeUtc(oldPath, DateTime.UtcNow.AddMinutes(-10));
        File.SetLastWriteTimeUtc(newPath, DateTime.UtcNow);

        var resolved = ResolveLogFilePathViaReflection();

        Assert.Equal(newPath, resolved);
    }

    [Fact]
    public void ResolveLogFilePath_WhenNoLogsExist_ReturnsFallbackPath()
    {
        var resolved = ResolveLogFilePathViaReflection();

        Assert.Equal(_paths.LogFilePath, resolved);
    }

    [Fact]
    public void OpenCurrentLogFile_WhenNoResolvedFileExists_DoesNotThrow()
    {
        var ex = Record.Exception(() => _sut.OpenCurrentLogFile());

        Assert.Null(ex);
    }

    private string ResolveLogFilePathViaReflection()
    {
        var method = typeof(LogFileService).GetMethod("ResolveLogFilePath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.NotNull(method);
        return Assert.IsType<string>(method!.Invoke(_sut, null));
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_root))
            {
                Directory.Delete(_root, recursive: true);
            }
        }
        catch
        {
            // best-effort cleanup
        }
    }
}
