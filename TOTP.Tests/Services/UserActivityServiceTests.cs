using Moq;
using TOTP.Core.Models;
using TOTP.Core.Security.Interfaces;
using TOTP.Core.Security.Models;
using TOTP.Services;

namespace TOTP.Tests.Services;

public sealed class UserActivityServiceTests
{
    [UIFact]
    public async Task StartMonitoring_WhenIdleTimeoutExceeded_RaisesLockRequestedOnce()
    {
        var settings = new Mock<ISettingsService>();
        settings.SetupGet(s => s.Current).Returns(new AppSettings { IdleTimeout = TimeSpan.FromMilliseconds(200) });
        var sut = new UserActivityService(settings.Object);
        var raisedCount = 0;
        sut.LockRequested += (_, _) => raisedCount++;

        sut.StartMonitoring();
        await PumpUntilAsync(() => raisedCount == 1, 2500);
        sut.StopMonitoring();

        Assert.Equal(1, raisedCount);

        // wait one more tick to ensure no duplicate lock request while still idle
        await Task.Delay(1200);
        DispatcherUtil.DoEvents();
        Assert.Equal(1, raisedCount);
    }

    [UIFact]
    public async Task NotifyActivity_BeforeTimeout_PreventsLock()
    {
        var settings = new Mock<ISettingsService>();
        settings.SetupGet(s => s.Current).Returns(new AppSettings { IdleTimeout = TimeSpan.FromSeconds(2) });
        var sut = new UserActivityService(settings.Object);
        var raised = false;
        sut.LockRequested += (_, _) => raised = true;

        sut.StartMonitoring();
        await PumpForAsync(900);
        sut.NotifyActivity(ActivityKind.MouseMove);
        await PumpForAsync(900);
        sut.StopMonitoring();

        Assert.False(raised);
    }

    [UIFact]
    public async Task StopMonitoring_StopsIdleChecks()
    {
        var settings = new Mock<ISettingsService>();
        settings.SetupGet(s => s.Current).Returns(new AppSettings { IdleTimeout = TimeSpan.FromMilliseconds(100) });
        var sut = new UserActivityService(settings.Object);
        var raised = false;
        sut.LockRequested += (_, _) => raised = true;

        sut.StartMonitoring();
        sut.StopMonitoring();
        await PumpForAsync(1500);

        Assert.False(raised);
        Assert.Equal(TimeSpan.Zero, sut.TimeSinceLastActivity);
    }

    private static async Task PumpUntilAsync(Func<bool> predicate, int timeoutMs)
    {
        var start = DateTime.UtcNow;
        while (!predicate() && (DateTime.UtcNow - start).TotalMilliseconds < timeoutMs)
        {
            await Task.Delay(20);
            DispatcherUtil.DoEvents();
        }
    }

    private static Task PumpForAsync(int timeoutMs) => PumpUntilAsync(() => false, timeoutMs);
}
