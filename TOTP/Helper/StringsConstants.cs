using System;
using System.IO;

namespace TOTP.Helper
{
    internal static class StringsConstants
    {

        public class ImgUrl
        {
            private const string AssemblyNameWpf = "TOTP.UI.WPF";
            public const string ImgCancel = $"pack://application:,,,/{AssemblyNameWpf};component/Assets/Icons/Cancel.png";
            public const string ImgInfo = $"pack://application:,,,/{AssemblyNameWpf};component/Assets/Icons/Info.png";
            public const string ImgWarning = $"pack://application:,,,/{AssemblyNameWpf};component/Assets/Icons/warning.png";
            public const string ImgError = $"pack://application:,,,/{AssemblyNameWpf};component/Assets/Icons/Error.png";
            public const string ImgLockAdd = $"pack://application:,,,/{AssemblyNameWpf};component/Assets/Icons/lock-add.png";

            public const string DeFlag = $"pack://application:,,,/{AssemblyNameWpf};component/Assets/flags/de.png";
            public const string EnFlag = $"pack://application:,,,/{AssemblyNameWpf};component/Assets/flags/en.png";
        }

        public const string RootLogPath = "Logs/app-root-start.log";
        public const string AppLogPath = "Logs/app.log";
        public static string AppSettingsJsonFilePath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
        public static readonly string Syncfusion = "syncfusion";
    }
}
