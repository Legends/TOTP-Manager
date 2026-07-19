namespace TOTP.Avalonia.Desktop.Localization;

public interface IAvaloniaLocalizationService
{
    IReadOnlyList<LanguageOption> SupportedLanguages { get; }
    LanguageOption CurrentLanguage { get; }
    void ApplyCulture(string cultureName);
    string GetString(string key);
}

public sealed record LanguageOption(string CultureName, string DisplayName)
{
    public string IconPath =>
        $"avares://TOTP.UI.Avalonia.Desktop/Assets/flags/{CultureName}.png";
}
