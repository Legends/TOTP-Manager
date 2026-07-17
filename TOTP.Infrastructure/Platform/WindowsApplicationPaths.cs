using TOTP.Core.Common;
using TOTP.Core.Services.Interfaces;

namespace TOTP.Infrastructure.Platform;

public sealed class WindowsApplicationPaths : IPlatformApplicationPaths
{
    public WindowsApplicationPaths(
        string? executableDirectory = null,
        string? roamingApplicationDataDirectory = null)
    {
        ExecutableDirectory = Path.GetFullPath(executableDirectory ?? ResolveExecutableDirectory());

        var roamingRoot = roamingApplicationDataDirectory
            ?? Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        if (string.IsNullOrWhiteSpace(roamingRoot))
        {
            throw new InvalidOperationException("The Windows roaming application-data directory is unavailable.");
        }

        ApplicationDataDirectory = Path.Combine(
            Path.GetFullPath(roamingRoot),
            StringsConstants.AppDataDirectoryName);

        ConfigurationFilePath = Path.Combine(ExecutableDirectory, StringsConstants.AppSettingsFileName);
        VaultFilePath = Path.Combine(ApplicationDataDirectory, StringsConstants.TokensStorageFileName);
        SettingsFilePath = Path.Combine(ApplicationDataDirectory, StringsConstants.AppSettingsStorageFileName);
        AuthorizationEnvelopeFilePath = Path.Combine(ApplicationDataDirectory, StringsConstants.AuthorizationEnvelopeFileName);
        BackupDirectory = ApplicationDataDirectory;
        LogDirectory = Path.Combine(ExecutableDirectory, "Logs");
        LogFilePath = Path.Combine(LogDirectory, "app.log");
        UpdateStateFilePath = Path.Combine(ApplicationDataDirectory, StringsConstants.AutoUpdateStateFileName);
    }

    public string ExecutableDirectory { get; }
    public string ConfigurationFilePath { get; }
    public string ApplicationDataDirectory { get; }
    public string VaultFilePath { get; }
    public string SettingsFilePath { get; }
    public string AuthorizationEnvelopeFilePath { get; }
    public string BackupDirectory { get; }
    public string LogDirectory { get; }
    public string LogFilePath { get; }
    public string UpdateStateFilePath { get; }

    private static string ResolveExecutableDirectory() =>
        Path.GetDirectoryName(Environment.ProcessPath)
        ?? AppDomain.CurrentDomain.BaseDirectory;
}
