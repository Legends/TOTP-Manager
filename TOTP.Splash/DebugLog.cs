using System;
using System.IO;

namespace TOTP.Splash;

internal static class DebugLog
{
    private static readonly string LogPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "TOTPManager",
        "Logs",
        "splash.log");

    public static void Write(string message)
    {
        try
        {
            var dir = Path.GetDirectoryName(LogPath);
            if (!string.IsNullOrWhiteSpace(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var line = $"{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss.fff zzz} [SPLASH] {message}{Environment.NewLine}";
            File.AppendAllText(LogPath, line);
        }
        catch
        {
        }
    }
}
