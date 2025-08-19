using System;
using System.IO;

namespace TOTP.Helper
{
    internal static class StringsConstants
    {

        public class ImgUrl
        {
            private const string assemblyName = "TOTP.UI.WPF";
            public const string ImgCancel = $"pack://application:,,,/{assemblyName};component/Assets/Icons/Cancel.png";
            public const string ImgInfo = $"pack://application:,,,/{assemblyName};component/Assets/Icons/Info.png";
            public const string ImgWarning = $"pack://application:,,,/{assemblyName};component/Assets/Icons/warning.png";
            public const string ImgError = $"pack://application:,,,/{assemblyName};component/Assets/Icons/Error.png";
            public const string ImgLockAdd = $"pack://application:,,,/{assemblyName};component/Assets/Icons/lock-add.png";

            public const string DeFlag = $"pack://application:,,,/{assemblyName};component/Assets/flags/de.png";
            public const string EnFlag = $"pack://application:,,,/{assemblyName};component/Assets/flags/en.png";
        }

        public static string AppSettingsJsonFilePath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
        public static string Syncfusion = "syncfusion";
    }
}
