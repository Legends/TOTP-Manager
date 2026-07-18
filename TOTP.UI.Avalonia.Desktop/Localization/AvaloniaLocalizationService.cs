using System.Globalization;
using Avalonia.Controls;

namespace TOTP.Avalonia.Desktop.Localization;

public sealed class AvaloniaLocalizationService : IAvaloniaLocalizationService
{
    private static readonly LanguageOption English = new("en", "English");
    private static readonly LanguageOption German = new("de", "Deutsch");
    private readonly IResourceDictionary _resources;
    private readonly AvaloniaStringCatalog _catalog;

    public AvaloniaLocalizationService(
        IResourceDictionary resources,
        AvaloniaStringCatalog catalog)
    {
        _resources = resources ?? throw new ArgumentNullException(nameof(resources));
        _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        SupportedLanguages = [English, German];
        CurrentLanguage = English;
    }

    public IReadOnlyList<LanguageOption> SupportedLanguages { get; }

    public LanguageOption CurrentLanguage { get; private set; }

    public void ApplyCulture(string cultureName)
    {
        var requested = GetSafeCulture(cultureName);
        CurrentLanguage = requested.TwoLetterISOLanguageName.Equals("de", StringComparison.OrdinalIgnoreCase)
            ? German
            : English;
        var resourceCulture = CultureInfo.GetCultureInfo(CurrentLanguage.CultureName);
        foreach (var key in AvaloniaStringKeys.All)
            _resources[key] = _catalog.Get(key, resourceCulture);
    }

    private static CultureInfo GetSafeCulture(string cultureName)
    {
        if (string.IsNullOrWhiteSpace(cultureName)) return CultureInfo.GetCultureInfo("en");
        try
        {
            return CultureInfo.GetCultureInfo(cultureName);
        }
        catch (CultureNotFoundException)
        {
            return CultureInfo.GetCultureInfo("en");
        }
    }
}
