using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;
using TOTP.Avalonia.Desktop;
using TOTP.Avalonia.Shared.Controls;
using TOTP.Avalonia.Shared.Styles;

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

    [AvaloniaFact]
    public void HighContrastVariant_ResolvesDedicatedSemanticPalette()
    {
        var application = Assert.IsType<App>(Application.Current);
        application.RequestedThemeVariant = AvaloniaThemeVariants.HighContrast;
        var window = new MainWindow();

        try
        {
            window.Show();

            Assert.True(window.TryFindResource(
                "BrushWindowBackground",
                AvaloniaThemeVariants.HighContrast,
                out var background));
            Assert.Equal(Colors.Black, Assert.IsType<SolidColorBrush>(background).Color);
            Assert.True(window.TryFindResource(
                "BrushFocus",
                AvaloniaThemeVariants.HighContrast,
                out var focus));
            Assert.Equal(Color.Parse("#00FFFF"), Assert.IsType<SolidColorBrush>(focus).Color);
        }
        finally
        {
            window.Close();
            application.RequestedThemeVariant = ThemeVariant.Default;
        }
    }
}
