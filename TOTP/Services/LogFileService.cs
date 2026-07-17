using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using TOTP.Core.Services.Interfaces;
using TOTP.Services.Interfaces;

namespace TOTP.Services;

public sealed class LogFileService : ILogFileService
{
    private readonly IPlatformApplicationPaths _applicationPaths;

    public LogFileService(IPlatformApplicationPaths applicationPaths)
    {
        _applicationPaths = applicationPaths ?? throw new ArgumentNullException(nameof(applicationPaths));
    }

    public bool CanOpenLogFolder() => Directory.Exists(_applicationPaths.LogDirectory);

    public void OpenLogFolder()
    {
        if (!CanOpenLogFolder())
        {
            return;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = _applicationPaths.LogDirectory,
            UseShellExecute = true
        });
    }

    public void OpenCurrentLogFile()
    {
        try
        {
            var fullPath = ResolveLogFilePath();
            if (!File.Exists(fullPath))
            {
                return;
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = fullPath,
                UseShellExecute = true
            });
        }
        catch
        {
        }
    }

    private string ResolveLogFilePath()
    {
        var currentRolling = Path.Combine(_applicationPaths.LogDirectory, $"app{DateTime.Now:yyyyMMdd}.log");
        if (File.Exists(currentRolling))
        {
            return currentRolling;
        }

        if (Directory.Exists(_applicationPaths.LogDirectory))
        {
            var latestRolling = Directory.GetFiles(_applicationPaths.LogDirectory, "app*.log")
                .OrderByDescending(File.GetLastWriteTimeUtc)
                .FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(latestRolling))
            {
                return latestRolling;
            }
        }

        return _applicationPaths.LogFilePath;
    }
}
