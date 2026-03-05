using FluentResults;
using Moq;
using TOTP.Core.Enums;
using TOTP.Core.Models;
using TOTP.Core.Security.Interfaces;
using TOTP.Core.Services.Interfaces;
using TOTP.Services;
using TOTP.Services.Interfaces;

namespace TOTP.Tests.Services;

public sealed class SettingsPersistenceServiceTests
{
    [Fact]
    public void ReadCurrentGeneralSettings_WhenCliOverrideActive_UsesOverrideAndDefaults()
    {
        var appSettings = new AppSettings
        {
            MinimumLogLevel = AppLogLevel.Information,
            QrPreviewScaleFactor = 10.0,
            IdleTimeout = TimeSpan.Zero,
            ClearClipboardSeconds = 0
        };

        var settings = new Mock<ISettingsService>();
        var logSwitch = new Mock<ILogSwitchService>();
        var qr = new Mock<IQrPreviewService>();
        settings.SetupGet(s => s.Current).Returns(appSettings);
        logSwitch.SetupGet(l => l.IsCliOverrideActive).Returns(true);
        logSwitch.Setup(l => l.GetLevel()).Returns(AppLogLevel.Warning);

        var sut = new SettingsPersistenceService(settings.Object, logSwitch.Object, qr.Object);

        var snapshot = sut.ReadCurrentGeneralSettings();

        Assert.Equal(AppLogLevel.Warning, snapshot.SelectedLogLevel);
        Assert.False(snapshot.LockOnIdleTimeout);
        Assert.Equal(AppSettings.DefaultClearClipboardSeconds, snapshot.ClearClipboardSeconds);
        Assert.Equal(6.0, snapshot.QrPreviewScaleFactor);
    }

    [Fact]
    public async Task SaveGeneralSettingsAsync_WhenSaveFails_ReturnsError()
    {
        var appSettings = new AppSettings();
        var settings = new Mock<ISettingsService>();
        var logSwitch = new Mock<ILogSwitchService>();
        var qr = new Mock<IQrPreviewService>();
        settings.SetupGet(s => s.Current).Returns(appSettings);
        settings.Setup(s => s.SaveAsync()).ReturnsAsync(Result.Fail("save failed"));
        logSwitch.Setup(l => l.GetLevel()).Returns(AppLogLevel.Information);

        var sut = new SettingsPersistenceService(settings.Object, logSwitch.Object, qr.Object);

        var result = await sut.SaveGeneralSettingsAsync(new SettingsGeneralSnapshot(
            AppLogLevel.Warning, true, true, true, 10, true, 15, 2.0, true, true, true));

        Assert.False(result.IsSuccess);
        Assert.Contains("save failed", result.ErrorMessage);
    }

    [Fact]
    public async Task SaveGeneralSettingsAsync_WhenLevelChanges_UpdatesLogSwitchAndQrPreview()
    {
        var appSettings = new AppSettings();
        var settings = new Mock<ISettingsService>();
        var logSwitch = new Mock<ILogSwitchService>();
        var qr = new Mock<IQrPreviewService>();

        settings.SetupGet(s => s.Current).Returns(appSettings);
        settings.Setup(s => s.SaveAsync()).ReturnsAsync(Result.Ok());
        logSwitch.Setup(l => l.GetLevel()).Returns(AppLogLevel.Information);
        logSwitch.SetupProperty(l => l.IsCliOverrideActive, true);

        var sut = new SettingsPersistenceService(settings.Object, logSwitch.Object, qr.Object);

        var result = await sut.SaveGeneralSettingsAsync(new SettingsGeneralSnapshot(
            AppLogLevel.Error,
            true,
            true,
            true,
            15,
            true,
            20,
            9.0,
            true,
            true,
            true));

        Assert.True(result.IsSuccess);
        Assert.True(result.LogSwitchStateChanged);
        Assert.False(logSwitch.Object.IsCliOverrideActive);
        Assert.Equal(6.0, appSettings.QrPreviewScaleFactor);
        logSwitch.Verify(l => l.SetLevel(AppLogLevel.Error), Times.Once);
        qr.VerifySet(q => q.PreviewScaleFactor = 6.0, Times.Once);
    }
}
