using Avalonia.Styling;

namespace TOTP.Avalonia.Desktop.Startup;

public static class AvaloniaThemeVariants
{
    public static ThemeVariant HighContrast { get; } =
        new("HighContrast", ThemeVariant.Dark);
}
