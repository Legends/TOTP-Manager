using System.Globalization;

namespace TOTP.Models
{
    public class CultureDisplay
    {
        public CultureInfo Culture { get; }
        public string DisplayName => Culture.NativeName;
        public string IconPath { get; }

        public CultureDisplay(CultureInfo culture, string iconPath)
        {
            Culture = culture;
            IconPath = iconPath;
        }

        public override string ToString() => DisplayName;
    }

}
