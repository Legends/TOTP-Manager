using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using TOTP.Avalonia.Desktop;
using TOTP.Avalonia.Shared.Controls;

namespace TOTP.Tests.Avalonia.Headless;

public sealed class MainWindowSmokeTests
{
    [AvaloniaFact]
    public void MainWindow_LoadsRealXamlAndEssentialSurfaces()
    {
        var window = new MainWindow();

        try
        {
            window.Show();

            Assert.NotNull(window.Icon);
            Assert.Single(window.GetVisualDescendants().OfType<BusyOverlay>());
            Assert.True(window.GetVisualDescendants().OfType<Button>().Count() >= 5);
            Assert.True(window.GetVisualDescendants().OfType<Border>().Count() >= 5);
            Assert.Single(window.GetVisualDescendants().OfType<ScrollViewer>());
        }
        finally
        {
            window.Close();
        }
    }
}
