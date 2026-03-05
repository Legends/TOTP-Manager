using System.Reflection;
using TOTP.Infrastructure.Common;
using TOTP.Services;

namespace TOTP.Tests.Services;

public sealed class LogFileServiceTests : IDisposable
{
    private readonly string _logsDir = StringsConstants.AppLogDirectoryPath;
    private readonly string _rollingPath = StringsConstants.CurrentRollingAppLogFilePath;
    private readonly string _fallbackPath = StringsConstants.AppLogFilePath;

    public LogFileServiceTests()
    {
        Directory.CreateDirectory(_logsDir);
        CleanupLogs();
    }

    [Fact]
    public void ResolveLogFilePath_WhenCurrentRollingExists_PrefersCurrentRolling()
    {
        File.WriteAllText(_rollingPath, "current");
        File.WriteAllText(Path.Combine(_logsDir, "app20000101.log"), "older");

        var resolved = ResolveLogFilePathViaReflection();

        Assert.Equal(_rollingPath, resolved);
    }

    [Fact]
    public void ResolveLogFilePath_WhenCurrentMissing_UsesLatestRolling()
    {
        var oldPath = Path.Combine(_logsDir, "app20000101.log");
        var newPath = Path.Combine(_logsDir, "app20000102.log");
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

        Assert.Equal(_fallbackPath, resolved);
    }

    [Fact]
    public void OpenCurrentLogFile_WhenNoResolvedFileExists_DoesNotThrow()
    {
        var sut = new LogFileService();

        var ex = Record.Exception(() => sut.OpenCurrentLogFile());

        Assert.Null(ex);
    }

    private static string ResolveLogFilePathViaReflection()
    {
        var method = typeof(LogFileService).GetMethod("ResolveLogFilePath", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);
        return Assert.IsType<string>(method!.Invoke(null, null));
    }

    private void CleanupLogs()
    {
        if (!Directory.Exists(_logsDir))
        {
            return;
        }

        foreach (var file in Directory.GetFiles(_logsDir, "app*.log"))
        {
            TryDelete(file);
        }

        TryDelete(_fallbackPath);
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch
        {
            // best-effort cleanup
        }
    }

    public void Dispose() => CleanupLogs();
}
