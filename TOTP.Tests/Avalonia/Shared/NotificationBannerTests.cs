using Avalonia.Automation;
using TOTP.Avalonia.Shared.Controls;
using TOTP.Core.Services.Interfaces;

namespace TOTP.Tests.Avalonia.Shared;

public sealed class NotificationBannerTests
{
    [Fact]
    public void EmptyText_IsNotPresented()
    {
        var sut = new NotificationBanner();

        Assert.False(sut.IsVisible);
    }

    [Theory]
    [InlineData(NotificationSeverity.Information, AutomationLiveSetting.Polite)]
    [InlineData(NotificationSeverity.Success, AutomationLiveSetting.Polite)]
    [InlineData(NotificationSeverity.Warning, AutomationLiveSetting.Polite)]
    [InlineData(NotificationSeverity.Error, AutomationLiveSetting.Assertive)]
    public void Severity_SelectsAccessibleAnnouncementPriority(
        NotificationSeverity severity,
        AutomationLiveSetting expected)
    {
        var sut = new NotificationBanner
        {
            Text = "Safe synthetic status",
            Severity = severity
        };

        Assert.True(sut.IsVisible);
        Assert.Equal(expected, sut.LiveSetting);
    }
}
