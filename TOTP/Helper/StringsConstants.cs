using System;
using System.IO;

namespace TOTP.Helper
{
    internal static class StringsConstants
    {

        public class ImgUrl
        {
            public const string ImgCancel = "pack://application:,,,/TOTP.Manager;component/Assets/Icons/Cancel.png";
            public const string ImgInfo = "pack://application:,,,/TOTP.Manager;component/Assets/Icons/Info.png";
            public const string ImgWarning = "pack://application:,,,/TOTP.Manager;component/Assets/Icons/warning.png";
            public const string ImgError = "pack://application:,,,/TOTP.Manager;component/Assets/Icons/Error.png";
            public const string ImgLockAdd = "pack://application:,,,/TOTP.Manager;component/Assets/Icons/lock-add.png";

            public const string DeFlag = "pack://application:,,,/TOTP.Manager;component/Assets/flags/de.png";
            public const string EnFlag = "pack://application:,,,/TOTP.Manager;component/Assets/flags/en.png";
        }
        //public const string ImgCancel = "pack://application:,,,/TOTP.Manager;component/Assets/Icons/Cancel.png";
        //public const string ImgInfo = "pack://application:,,,/TOTP.Manager;component/Assets/Icons/Info.png";
        //public const string ImgWarning = "pack://application:,,,/TOTP.Manager;component/Assets/Icons/warning.png";
        //public const string ImgError = "pack://application:,,,/TOTP.Manager;component/Assets/Icons/Error.png";
        //public const string ImgLockAdd = "pack://application:,,,/TOTP.Manager;component/Assets/Icons/lock-add.png";

        //public const string DeFlag = "pack://application:,,,/TOTP.Manager;component/Assets/flags/de.png";
        //public const string EnFlag = "pack://application:,,,/TOTP.Manager;component/Assets/flags/en.png";

        public static string AppSettingsJsonFilePath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
    }
}
