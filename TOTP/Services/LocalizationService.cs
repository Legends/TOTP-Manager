using System;
using System.Globalization;
using System.IO;
using System.Threading;

namespace TOTP.Services
{
    public static class LocalizationService
    {
        public static event Action? LanguageChanged;

        public static void ChangeCulture(string cultureCode)
        {
            var culture = new CultureInfo(cultureCode);
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            UpdateCultureSetting(cultureCode);
            LanguageChanged?.Invoke();
            //LanguageChanged?.Invoke(this, value.Culture);
        }

        private static void UpdateCultureSetting(string cultureName)
        {
            var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");

            var json = File.ReadAllText(configPath);
            var jObject = Newtonsoft.Json.Linq.JObject.Parse(json);

            jObject["Localization"]["Culture"] = cultureName;

            File.WriteAllText(configPath, jObject.ToString(Newtonsoft.Json.Formatting.Indented));
        }

    }

}
