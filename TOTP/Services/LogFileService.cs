using System.Diagnostics;
using System.IO;
using System.Linq;
using TOTP.Core.Common;
using TOTP.Services.Interfaces;

namespace TOTP.Services;

public sealed class LogFileService : ILogFileService
{
    public bool CanOpenLogFolder() => Directory.Exists(StringsConstants.AppLogDirectoryPath);

    public void OpenLogFolder()
    {
        if (!CanOpenLogFolder())
        {
            return;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = StringsConstants.AppLogDirectoryPath,
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

    private static string ResolveLogFilePath()
    {
        var currentRolling = StringsConstants.CurrentRollingAppLogFilePath;
        if (File.Exists(currentRolling))
        {
            return currentRolling;
        }

        if (Directory.Exists(StringsConstants.AppLogDirectoryPath))
        {
            var latestRolling = Directory.GetFiles(StringsConstants.AppLogDirectoryPath, "app*.log")
                .OrderByDescending(File.GetLastWriteTimeUtc)
                .FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(latestRolling))
            {
                return latestRolling;
            }
        }

        return StringsConstants.AppLogFilePath;
    }
}
