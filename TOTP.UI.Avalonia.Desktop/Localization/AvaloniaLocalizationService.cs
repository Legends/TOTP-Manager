using System.Globalization;
using Avalonia.Controls;

namespace TOTP.Avalonia.Desktop.Localization;

public sealed class AvaloniaLocalizationService : IAvaloniaLocalizationService
{
    private readonly LanguageOption _english;
    private readonly LanguageOption _german;
    private readonly IResourceDictionary _resources;
    private readonly AvaloniaStringCatalog _catalog;

    public AvaloniaLocalizationService(
        IResourceDictionary resources,
        AvaloniaStringCatalog catalog,
        ILanguageFlagProvider? flags = null)
    {
        _resources = resources ?? throw new ArgumentNullException(nameof(resources));
        _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        _english = new LanguageOption("en", "English")
        {
            Icon = flags?.GetFlag("en")
        };
        _german = new LanguageOption("de", "Deutsch")
        {
            Icon = flags?.GetFlag("de")
        };
        SupportedLanguages = [_english, _german];
        CurrentLanguage = _english;
    }

    public IReadOnlyList<LanguageOption> SupportedLanguages { get; }

    public event EventHandler? CultureChanged;

    public LanguageOption CurrentLanguage { get; private set; }

    public void ApplyCulture(string cultureName)
    {
        var requested = GetSafeCulture(cultureName);
        CurrentLanguage = requested.TwoLetterISOLanguageName.Equals("de", StringComparison.OrdinalIgnoreCase)
            ? _german
            : _english;
        var resourceCulture = CultureInfo.GetCultureInfo(CurrentLanguage.CultureName);
        foreach (var key in AvaloniaStringKeys.All)
            _resources[key] = _catalog.Get(key, resourceCulture);
        CultureChanged?.Invoke(this, EventArgs.Empty);
    }

    public string GetString(string key) =>
        _catalog.Get(key, CultureInfo.GetCultureInfo(CurrentLanguage.CultureName));

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
