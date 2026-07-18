using FluentResults;
using Moq;
using TOTP.Core.Models;
using TOTP.Core.Security.Interfaces;
using TOTP.Avalonia.Desktop.Presentation;
using TOTP.Avalonia.Desktop.Localization;

namespace TOTP.Tests.Avalonia.Presentation;

public sealed class SettingsPageViewModelTests
{
    [Fact]
    public async Task SaveAsync_PersistsReviewedSecurityPreferences()
    {
        var current = new AppSettings();
        var settings = CreateSettings(current);
        settings.Setup(value => value.SaveAsync()).ReturnsAsync(Result.Ok());
        var sut = new SettingsPageViewModel(settings.Object)
        {
            IdleTimeoutMinutes = 25,
            LockOnMinimize = false
        };

        await sut.SaveAsync();

        Assert.Equal(TimeSpan.FromMinutes(25), current.IdleTimeout);
        Assert.False(current.LockOnMinimize);
        Assert.Equal("Settings saved.", sut.Message);
    }

    [Fact]
    public async Task SaveAsync_WhenPersistenceFails_RestoresActiveSettings()
    {
        var current = new AppSettings
        {
            IdleTimeout = TimeSpan.FromMinutes(10),
            LockOnMinimize = true
        };
        var settings = CreateSettings(current);
        settings.Setup(value => value.SaveAsync())
            .ReturnsAsync(Result.Fail("synthetic failure"));
        var sut = new SettingsPageViewModel(settings.Object)
        {
            IdleTimeoutMinutes = 60,
            LockOnMinimize = false
        };

        await sut.SaveAsync();

        Assert.Equal(TimeSpan.FromMinutes(10), current.IdleTimeout);
        Assert.True(current.LockOnMinimize);
        Assert.DoesNotContain("synthetic", sut.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void SelectedLanguage_AppliesImmediatelyWithoutChangingSecuritySettings()
    {
        var current = new AppSettings
        {
            IdleTimeout = TimeSpan.FromMinutes(12),
            LockOnMinimize = true
        };
        var localization = new Mock<IAvaloniaLocalizationService>();
        var english = new LanguageOption("en", "English");
        var german = new LanguageOption("de", "Deutsch");
        localization.SetupGet(value => value.SupportedLanguages).Returns([english, german]);
        localization.SetupGet(value => value.CurrentLanguage).Returns(english);
        var sut = new SettingsPageViewModel(CreateSettings(current).Object, localization.Object);

        sut.SelectedLanguage = german;

        localization.Verify(value => value.ApplyCulture("de"), Times.Once);
        Assert.Equal(TimeSpan.FromMinutes(12), current.IdleTimeout);
        Assert.True(current.LockOnMinimize);
    }

    private static Mock<ISettingsService> CreateSettings(AppSettings current)
    {
        var settings = new Mock<ISettingsService>();
        settings.SetupGet(value => value.Current).Returns(current);
        return settings;
    }
}
