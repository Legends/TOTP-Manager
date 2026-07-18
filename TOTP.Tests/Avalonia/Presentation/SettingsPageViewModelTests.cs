using FluentResults;
using Moq;
using TOTP.Core.Models;
using TOTP.Core.Security.Interfaces;
using TOTP.Avalonia.Desktop.Presentation;

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

    private static Mock<ISettingsService> CreateSettings(AppSettings current)
    {
        var settings = new Mock<ISettingsService>();
        settings.SetupGet(value => value.Current).Returns(current);
        return settings;
    }
}
