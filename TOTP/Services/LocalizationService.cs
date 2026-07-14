using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using TOTP.Core.Security.Interfaces;
using TOTP.Services.Interfaces;

namespace TOTP.Services;

public sealed class LocalizationService(
    ISettingsService settingsService,
    ILogger<LocalizationService> logger) : ILocalizationService
{
    public event Action? LanguageChanged;

    public void ApplyCurrentCulture()
        => ApplyCulture(settingsService.Current.CultureName, persist: false);

    public void ChangeCulture(string cultureName)
        => ApplyCulture(cultureName, persist: true);

    private void ApplyCulture(string cultureName, bool persist)
    {
        CultureInfo culture;
        try
        {
            culture = CultureInfo.GetCultureInfo(
                string.IsNullOrWhiteSpace(cultureName) ? "en" : cultureName);
        }
        catch (CultureNotFoundException ex)
        {
            logger.LogWarning(ex, "Ignoring unsupported UI culture {CultureName}", cultureName);
            return;
        }

        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        settingsService.Current.CultureName = culture.Name;

        LanguageChanged?.Invoke();
        LocalizationEventHub.RaiseLanguageChanged();

        if (persist)
        {
            _ = PersistCultureAsync();
        }
    }

    private async Task PersistCultureAsync()
    {
        try
        {
            var result = await settingsService.SaveAsync();
            if (result.IsFailed)
            {
                logger.LogWarning("Could not persist the selected UI culture: {Errors}",
                    string.Join("; ", result.Errors));
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected failure while persisting the selected UI culture.");
        }
    }
}

internal static class LocalizationEventHub
{
    internal static event Action? LanguageChanged;

    internal static void RaiseLanguageChanged() => LanguageChanged?.Invoke();
}
