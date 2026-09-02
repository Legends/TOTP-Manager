using System.Globalization;
using TOTP.Tests.Common;

namespace TOTP.Tests.Updater;

[Collection(NonParallelCollectionDefinition.NonParallel)]
public sealed class UpdaterTextTests
{
    [Fact]
    public void GermanCulture_UsesCompleteLocalizedUpdaterMessages()
    {
        var previousCulture = CultureInfo.CurrentCulture;
        var previousUiCulture = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("de-DE");

            Assert.Equal(
                "Das signierte Update-Paket wird angewendet und die App neu gestartet.",
                TOTP.Updater.UpdaterText.HeaderDescription);
            Assert.Equal("3 Datei(en) in Warteschlange", TOTP.Updater.UpdaterText.FilesQueued(3));
            Assert.Equal("Schließen", TOTP.Updater.UpdaterText.Close);
            Assert.Equal(
                "Prüfen Sie das Updater-Protokoll auf Diagnosedetails und versuchen Sie es erneut.",
                TOTP.Updater.UpdaterText.UpdateFailedDetail);
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
            CultureInfo.CurrentUICulture = previousUiCulture;
        }
    }
}
