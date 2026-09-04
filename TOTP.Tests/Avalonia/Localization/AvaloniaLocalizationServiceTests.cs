using Avalonia.Controls;
using TOTP.Avalonia.Desktop.Localization;

namespace TOTP.Tests.Avalonia.Localization;

public sealed class AvaloniaLocalizationServiceTests
{
    [Fact]
    public void ApplyCulture_UpdatesExistingDynamicResourceHostInPlace()
    {
        var resources = new ResourceDictionary();
        var sut = new AvaloniaLocalizationService(resources, new AvaloniaStringCatalog());
        var cultureChanged = 0;
        sut.CultureChanged += (_, _) => cultureChanged++;

        sut.ApplyCulture("de-DE");

        Assert.Equal("de", sut.CurrentLanguage.CultureName);
        Assert.Equal("Erneut versuchen", resources[AvaloniaStringKeys.Retry]);
        Assert.Equal("Master-Passwort", resources[AvaloniaStringKeys.MasterPassword]);
        Assert.Equal("Anzeigen", resources[AvaloniaStringKeys.RevealSecret]);
        Assert.Equal("Vorhandene Konten überspringen", resources[AvaloniaStringKeys.ImportSkipExisting]);
        Assert.Equal(1, cultureChanged);
    }

    [Theory]
    [InlineData("it-IT")]
    [InlineData("not-a-culture")]
    [InlineData("")]
    public void ApplyCulture_WhenUnsupported_FallsBackToEnglish(string cultureName)
    {
        var resources = new ResourceDictionary();
        var sut = new AvaloniaLocalizationService(resources, new AvaloniaStringCatalog());

        sut.ApplyCulture(cultureName);

        Assert.Equal("en", sut.CurrentLanguage.CultureName);
        Assert.Equal("Retry", resources[AvaloniaStringKeys.Retry]);
    }

    [Fact]
    public void Catalog_HasEveryDeclaredKeyInEverySupportedLanguage()
    {
        var sut = new AvaloniaStringCatalog();

        Assert.Empty(sut.GetMissingKeys(System.Globalization.CultureInfo.GetCultureInfo("en")));
        Assert.Empty(sut.GetMissingKeys(System.Globalization.CultureInfo.GetCultureInfo("de")));
        Assert.Empty(sut.GetMissingKeys(System.Globalization.CultureInfo.GetCultureInfo("fr")));
        Assert.Empty(sut.GetMissingKeys(System.Globalization.CultureInfo.GetCultureInfo("es")));
    }

    [Theory]
    [InlineData("en", "OTP Harbor")]
    [InlineData("de", "OTP Harbor")]
    [InlineData("fr", "OTP Harbor")]
    [InlineData("es", "OTP Harbor")]
    public void Catalog_AppTitleUsesStableProductName(string cultureName, string expected)
    {
        var sut = new AvaloniaStringCatalog();

        Assert.Equal(
            expected,
            sut.Get(AvaloniaStringKeys.AppTitle, System.Globalization.CultureInfo.GetCultureInfo(cultureName)));
    }

    [Theory]
    [InlineData("en", "Export accounts")]
    [InlineData("de", "Konten exportieren")]
    [InlineData("fr", "Exporter les comptes")]
    [InlineData("es", "Exportar cuentas")]
    public void Catalog_ExportSectionUsesAccountFocusedHeading(string cultureName, string expected)
    {
        var sut = new AvaloniaStringCatalog();

        Assert.Equal(
            expected,
            sut.Get(AvaloniaStringKeys.EncryptedBackup, System.Globalization.CultureInfo.GetCultureInfo(cultureName)));
    }

    [Theory]
    [InlineData("fr-FR", "fr", "Réessayer", "Mot de passe principal")]
    [InlineData("es-ES", "es", "Reintentar", "Contraseña maestra")]
    public void ApplyCulture_ForAdditionalLanguage_UsesCompleteLocale(
        string requestedCulture,
        string expectedCulture,
        string expectedRetry,
        string expectedPassword)
    {
        var resources = new ResourceDictionary();
        var sut = new AvaloniaLocalizationService(resources, new AvaloniaStringCatalog());

        sut.ApplyCulture(requestedCulture);

        Assert.Equal(expectedCulture, sut.CurrentLanguage.CultureName);
        Assert.Equal(expectedRetry, resources[AvaloniaStringKeys.Retry]);
        Assert.Equal(expectedPassword, resources[AvaloniaStringKeys.MasterPassword]);
        Assert.Equal(4, sut.SupportedLanguages.Count);
        Assert.Equal(
            ["English", "Deutsch", "Français", "Español"],
            sut.SupportedLanguages.Select(value => value.DisplayName));
    }
}
