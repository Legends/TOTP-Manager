using System;
using System.Globalization;

namespace TOTP.Services
{
    public static class LocalizationService
    {
        public static event Action? LanguageChanged;

        public static void ChangeCulture(string cultureCode)
        {
            CultureInfo.CurrentUICulture = new CultureInfo(cultureCode);
            LanguageChanged?.Invoke();
        }
    }

}
