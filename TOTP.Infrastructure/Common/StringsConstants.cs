using System;
using System.IO;

namespace TOTP.Infrastructure.Common
{
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
        public const string AssemblyNameWpf = "TOTP.UI.WPF";
        public const string AppSettingsFileName = "appsettings.json";
        public static string AppSettingsJsonFilePath => Path.Combine(AppRootDirectory, AppSettingsFileName);
        public static readonly string Syncfusion = "syncfusion";
        public const string TokensStorageFilePathConfigKey = "Accounts:StorageFilePath";
        public const string AppSettingsStorageFilePathConfigKey = "AppSettings:StorageFilePath";

        public class ImgUrl
        {
            //private const string AssemblyNameWpf = "TOTP.UI.WPF";
            public const string ImgCancel = $"pack://application:,,,/{AssemblyNameWpf};component/Assets/Icons/Cancel.png";
            public const string ImgInfo = $"pack://application:,,,/{AssemblyNameWpf};component/Assets/Icons/Info.png";
            public const string ImgWarning = $"pack://application:,,,/{AssemblyNameWpf};component/Assets/Icons/warning.png";
            public const string ImgError = $"pack://application:,,,/{AssemblyNameWpf};component/Assets/Icons/Error.png";
            public const string ImgLockAdd = $"pack://application:,,,/{AssemblyNameWpf};component/Assets/Icons/lock-add.png";

            public const string DeFlag = $"pack://application:,,,/{AssemblyNameWpf};component/Assets/flags/de.png";
            public const string EnFlag = $"pack://application:,,,/{AssemblyNameWpf};component/Assets/flags/en.png";
        }

    }
}
