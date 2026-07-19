using Avalonia.Platform;
using Avalonia.Styling;
using Moq;
using TOTP.Avalonia.Desktop.Startup;

namespace TOTP.Tests.Avalonia.Startup;

public sealed class AvaloniaThemeServiceTests
{
    [Fact]
    public void Start_WhenPlatformRequestsHighContrast_AppliesDedicatedVariant()
    {
        var platform = new Mock<IPlatformSettings>();
        platform.Setup(value => value.GetColorValues()).Returns(Colors(ColorContrastPreference.High));
        ThemeVariant? applied = null;
        using var sut = new AvaloniaThemeService(platform.Object, value => applied = value);

        sut.Start();

        Assert.Same(AvaloniaThemeVariants.HighContrast, applied);
    }

    [Fact]
    public void ColorChange_ReturnsToProductDarkThemeAndDisposeUnsubscribes()
    {
        var platform = new Mock<IPlatformSettings>();
        platform.Setup(value => value.GetColorValues()).Returns(Colors(ColorContrastPreference.High));
        var applied = new List<ThemeVariant>();
        var sut = new AvaloniaThemeService(platform.Object, applied.Add);
        sut.Start();

        platform.Raise(
            value => value.ColorValuesChanged += null,
            platform.Object,
            Colors(ColorContrastPreference.NoPreference));
        sut.Dispose();
        platform.Raise(
            value => value.ColorValuesChanged += null,
            platform.Object,
            Colors(ColorContrastPreference.High));

        Assert.Equal([AvaloniaThemeVariants.HighContrast, ThemeVariant.Dark], applied);
    }

    [Fact]
    public void Start_WithoutPlatformSettings_UsesSafeDefault()
    {
        ThemeVariant? applied = null;
        using var sut = new AvaloniaThemeService(null, value => applied = value);

        sut.Start();

        Assert.Same(ThemeVariant.Dark, applied);
    }

    private static PlatformColorValues Colors(ColorContrastPreference contrast) =>
        new() { ContrastPreference = contrast };
}
