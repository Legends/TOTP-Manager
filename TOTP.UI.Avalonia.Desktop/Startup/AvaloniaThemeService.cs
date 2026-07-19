using Avalonia.Platform;
using Avalonia.Styling;

namespace TOTP.Avalonia.Desktop.Startup;

public sealed class AvaloniaThemeService(
    IPlatformSettings? platformSettings,
    Action<ThemeVariant> applyTheme) : IDisposable
{
    private bool _started;
    private bool _disposed;

    public void Start()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_started) return;
        _started = true;

        if (platformSettings is null)
        {
            applyTheme(ThemeVariant.Dark);
            return;
        }

        platformSettings.ColorValuesChanged += OnColorValuesChanged;
        Apply(platformSettings.GetColorValues());
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        if (_started && platformSettings is not null)
            platformSettings.ColorValuesChanged -= OnColorValuesChanged;
    }

    private void OnColorValuesChanged(object? sender, PlatformColorValues values) => Apply(values);

    private void Apply(PlatformColorValues values)
    {
        applyTheme(values.ContrastPreference == ColorContrastPreference.High
            ? AvaloniaThemeVariants.HighContrast
            : ThemeVariant.Dark);
    }
}
