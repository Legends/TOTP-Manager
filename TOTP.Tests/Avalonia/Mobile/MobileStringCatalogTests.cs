using System.Globalization;
using TOTP.Avalonia.Mobile.Localization;

namespace TOTP.Tests.Avalonia.Mobile;

public sealed class MobileStringCatalogTests
{
    [Theory]
    [InlineData("en")]
    [InlineData("de")]
    public void GetMissingKeys_ForSupportedCulture_ReturnsNone(string cultureName)
    {
        var catalog = new MobileStringCatalog(CultureInfo.GetCultureInfo(cultureName));

        var missing = catalog.GetMissingKeys(CultureInfo.GetCultureInfo(cultureName));

        Assert.Empty(missing);
    }

    [Fact]
    public void Get_WithGermanCulture_ReturnsCompleteGermanMessage()
    {
        var catalog = new MobileStringCatalog(CultureInfo.GetCultureInfo("de"));

        var message = catalog.Get(MobileStringKeys.UnlockDescription);

        Assert.Contains("Masterpasswort", message, StringComparison.Ordinal);
        Assert.DoesNotContain("encrypted local vault", message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Get_BiometricRecoveryMessage_UsesOnlyActiveGermanLocale()
    {
        var catalog = new MobileStringCatalog(CultureInfo.GetCultureInfo("de"));

        var message = catalog.Get(MobileStringKeys.BiometricRecoveryRequired);

        Assert.Contains("Masterpasswort", message, StringComparison.Ordinal);
        Assert.Contains("biometrischen Zugriff", message, StringComparison.Ordinal);
        Assert.DoesNotContain("recovery", message, StringComparison.OrdinalIgnoreCase);
    }
}
