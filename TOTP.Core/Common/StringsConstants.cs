using System;
using System.IO;

namespace TOTP.Core.Common;

public static class StringsConstants
{
    public static string AppLogPath => Path.Combine(AppLogDirectoryPath, "app.log");
    public static string AppRootDirectory =>
        Path.GetDirectoryName(Environment.ProcessPath) ??
        AppDomain.CurrentDomain.BaseDirectory;
    public static string AppLogDirectoryPath =>
        Path.Combine(AppRootDirectory, "Logs");
    public static string AppLogFilePath => Path.Combine(AppLogDirectoryPath, "app.log");
    public static string CurrentRollingAppLogFilePath => Path.Combine(AppLogDirectoryPath, $"app{DateTime.Now:yyyyMMdd}.log");
    public const string AppSettingsFileName = "appsettings.json";
    public static string AppSettingsJsonFilePath => Path.Combine(AppRootDirectory, AppSettingsFileName);
    public static readonly string Syncfusion = "syncfusion";
    public const string TokensStorageFilePathConfigKey = "Accounts:StorageFilePath";
    public const string AppSettingsStorageFilePathConfigKey = "AppSettings:StorageFilePath";
    public const string AppDataDirectoryName = "TOTP-Manager";
    public const string TokensStorageFileName = "master.totp";
    public const string AppSettingsStorageFileName = "settings.totp";
    public const string AutoUpdateStateFileName = "autoupdate-state.json";
    public static string RoamingAppDataDirectoryPath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppDataDirectoryName);
    public static string DefaultTokensStorageFilePath => Path.Combine(RoamingAppDataDirectoryPath, TokensStorageFileName);
    //public static string DefaultAppSettingsStorageFilePath => Path.Combine(RoamingAppDataDirectoryPath, AppSettingsStorageFileName);
    public static string AutoUpdateStateFilePath => Path.Combine(RoamingAppDataDirectoryPath, AutoUpdateStateFileName);

}
