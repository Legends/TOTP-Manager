using FluentResults;
using Moq;
using TOTP.Core.Models;
using TOTP.Core.Security.Interfaces;
using TOTP.Avalonia.Desktop.Presentation;
using TOTP.Avalonia.Desktop.Localization;
using TOTP.Core.Enums;
using TOTP.Core.Services.Interfaces;

namespace TOTP.Tests.Avalonia.Presentation;

public sealed class SettingsPageViewModelTests
{
    [Theory]
    [InlineData("en", "avares://TOTP.UI.Avalonia.Desktop/Assets/flags/en.png")]
    [InlineData("de", "avares://TOTP.UI.Avalonia.Desktop/Assets/flags/de.png")]
    public void LanguageOption_UsesBundledFlagAsset(string culture, string expectedPath)
    {
        var option = new LanguageOption(culture, culture);

        Assert.Equal(expectedPath, option.IconPath);
    }

    [Fact]
    public void Constructor_DoesNotPersistValuesWhileLoading()
    {
        var settings = CreateSettings(new AppSettings());

        using var sut = new SettingsPageViewModel(
            settings.Object,
            autoSaveDelay: TimeSpan.Zero);

        settings.Verify(value => value.SaveAsync(), Times.Never);
    }

    [Fact]
    public async Task SaveAsync_PersistsReviewedSecurityPreferences()
    {
        var current = new AppSettings();
        var settings = CreateSettings(current);
        settings.Setup(value => value.SaveAsync()).ReturnsAsync(Result.Ok());
        using var sut = new SettingsPageViewModel(settings.Object)
        {
            IdleTimeoutMinutes = 25,
            LockOnMinimize = false
        };

        await sut.SaveAsync();

        Assert.Equal(TimeSpan.FromMinutes(25), current.IdleTimeout);
        Assert.False(current.LockOnMinimize);
        Assert.Equal("Settings saved automatically.", sut.Message);
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
        using var sut = new SettingsPageViewModel(settings.Object)
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
        using var sut = new SettingsPageViewModel(CreateSettings(current).Object, localization.Object);

        sut.SelectedLanguage = german;

        localization.Verify(value => value.ApplyCulture("de"), Times.Once);
        Assert.Equal(TimeSpan.FromMinutes(12), current.IdleTimeout);
        Assert.True(current.LockOnMinimize);
    }

    [Fact]
    public async Task SaveAsync_PersistsCompletePortablePreferenceSet()
    {
        var current = new AppSettings();
        var settings = CreateSettings(current);
        settings.Setup(value => value.SaveAsync()).ReturnsAsync(Result.Ok());
        using var sut = new SettingsPageViewModel(settings.Object)
        {
            IdleTimeoutMinutes = 45,
            LockOnMinimize = false,
            LockOnSessionLock = false,
            ClearClipboardEnabled = true,
            ClearClipboardSeconds = 20,
            QrPreviewScaleFactor = 2.5m,
            ExportEncrypt = true,
            OpenExportFileAfterExport = false,
            HideSecretsByDefault = false,
            MinimumLogLevel = AppLogLevel.Warning
        };

        await sut.SaveAsync();

        Assert.Equal(TimeSpan.FromMinutes(45), current.IdleTimeout);
        Assert.False(current.LockOnMinimize);
        Assert.False(current.LockOnSessionLock);
        Assert.True(current.ClearClipboardEnabled);
        Assert.Equal(20, current.ClearClipboardSeconds);
        Assert.Equal(2.5, current.QrPreviewScaleFactor);
        Assert.True(current.ExportEncrypt);
        Assert.False(current.OpenExportFileAfterExport);
        Assert.False(current.HideSecretsByDefault);
        Assert.Equal(AppLogLevel.Warning, current.MinimumLogLevel);
    }

    [Fact]
    public async Task OpenLogFolderAsync_UsesPlatformPathWithoutDisplayingIt()
    {
        var settings = CreateSettings(new AppSettings());
        var paths = new Mock<IPlatformApplicationPaths>();
        paths.SetupGet(value => value.LogDirectory).Returns(@"C:\synthetic\logs");
        var launcher = new Mock<IPlatformFolderLauncher>();
        launcher.Setup(value => value.OpenFolderAsync(
                @"C:\synthetic\logs",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());
        using var sut = new SettingsPageViewModel(
            settings.Object,
            applicationPaths: paths.Object,
            folderLauncher: launcher.Object);

        await sut.OpenLogFolderAsync();

        Assert.Contains("opened", sut.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("synthetic", sut.Message, StringComparison.OrdinalIgnoreCase);
        Assert.False(string.IsNullOrWhiteSpace(sut.VersionText));
    }

    [Fact]
    public async Task ChangedPreference_IsSavedAutomatically()
    {
        var current = new AppSettings { LockOnMinimize = true };
        var settings = CreateSettings(current);
        var saved = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        settings.Setup(value => value.SaveAsync()).Returns(() =>
        {
            saved.TrySetResult();
            return Task.FromResult(Result.Ok());
        });
        using var sut = new SettingsPageViewModel(
            settings.Object,
            autoSaveDelay: TimeSpan.Zero);

        sut.LockOnMinimize = false;
        await saved.Task.WaitAsync(
            TimeSpan.FromSeconds(1),
            TestContext.Current.CancellationToken);

        Assert.False(current.LockOnMinimize);
        settings.Verify(value => value.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task RapidPreferenceChanges_ArePersistedAsOneSnapshot()
    {
        var current = new AppSettings();
        var settings = CreateSettings(current);
        var saved = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        settings.Setup(value => value.SaveAsync()).Returns(() =>
        {
            saved.TrySetResult();
            return Task.FromResult(Result.Ok());
        });
        using var sut = new SettingsPageViewModel(
            settings.Object,
            autoSaveDelay: TimeSpan.FromMilliseconds(25));

        sut.IdleTimeoutMinutes = 15;
        sut.ClearClipboardSeconds = 20;
        sut.HideSecretsByDefault = false;
        await saved.Task.WaitAsync(
            TimeSpan.FromSeconds(1),
            TestContext.Current.CancellationToken);
        await Task.Delay(50, TestContext.Current.CancellationToken);

        Assert.Equal(TimeSpan.FromMinutes(15), current.IdleTimeout);
        Assert.Equal(20, current.ClearClipboardSeconds);
        Assert.False(current.HideSecretsByDefault);
        settings.Verify(value => value.SaveAsync(), Times.Once);
    }

    private static Mock<ISettingsService> CreateSettings(AppSettings current)
    {
        var settings = new Mock<ISettingsService>();
        settings.SetupGet(value => value.Current).Returns(current);
        return settings;
    }
}
