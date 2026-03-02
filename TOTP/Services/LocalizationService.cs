using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using TOTP.Helper;
using TOTP.Infrastructure.Common;

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
            var json = File.ReadAllText(StringsConstants.AppSettingsJsonFilePath);
            var jObject = Newtonsoft.Json.Linq.JObject.Parse(json);

            if (jObject["Localization"] is JObject localization)
            {
                localization["Culture"] = cultureName;
            }
            else
            {
                jObject["Localization"] = new JObject
                {
                    ["Culture"] = cultureName
                };
            }

            File.WriteAllText(StringsConstants.AppSettingsJsonFilePath, jObject.ToString(Newtonsoft.Json.Formatting.Indented));
        }

    }

}
