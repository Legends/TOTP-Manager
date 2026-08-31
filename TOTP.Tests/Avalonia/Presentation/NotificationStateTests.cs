using TOTP.Avalonia.Desktop.Presentation;
using TOTP.Core.Services.Interfaces;

namespace TOTP.Tests.Avalonia.Presentation;

public sealed class NotificationStateTests
{
    [Fact]
    public void ShowPersistent_ExposesStandardTextSeverityAndVisibility()
    {
        using var sut = new NotificationState();

        sut.ShowPersistent("Synthetic warning", NotificationSeverity.Warning);

        Assert.Equal("Synthetic warning", sut.Text);
        Assert.Equal(NotificationSeverity.Warning, sut.Severity);
        Assert.True(sut.HasMessage);
    }

    [Fact]
    public async Task ShowTransient_ClearsAfterConfiguredDuration()
    {
        using var sut = new NotificationState(TimeSpan.FromMilliseconds(20));
        sut.ShowTransient("Saved", NotificationSeverity.Success);

        await Task.Delay(100, TestContext.Current.CancellationToken);

        Assert.Empty(sut.Text);
        Assert.False(sut.HasMessage);
    }

    [Fact]
    public async Task NewPersistentMessage_IsNotClearedByPreviousTransientLifetime()
    {
        using var sut = new NotificationState(TimeSpan.FromMilliseconds(20));
        sut.ShowTransient("Saved", NotificationSeverity.Success);
        sut.ShowPersistent("Failed", NotificationSeverity.Error);

        await Task.Delay(100, TestContext.Current.CancellationToken);

        Assert.Equal("Failed", sut.Text);
        Assert.Equal(NotificationSeverity.Error, sut.Severity);
    }
}
