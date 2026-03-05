using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Moq;
using TOTP.Core.Models;
using TOTP.Core.Security.Interfaces;
using TOTP.Infrastructure.Services;
using TOTP.Security.Interfaces;

namespace TOTP.Tests.Infrastructure.Services;

public sealed class SessionLockBackgroundServiceTests
{
    [Fact]
    public void OnSessionSwitch_WhenSessionLockAndEnabled_LocksApplication()
    {
        var auth = new Mock<IAuthorizationService>();
        var settings = new Mock<ISettingsService>();
        var logger = new Mock<ILogger<SessionLockBackgroundService>>();
        settings.SetupGet(s => s.Current).Returns(new AppSettings { LockOnSessionLock = true });

        var sut = new SessionLockBackgroundService(auth.Object, settings.Object, logger.Object);

        InvokeSessionSwitch(sut, SessionSwitchReason.SessionLock);

        auth.Verify(a => a.Lock(), Times.Once);
    }

    [Fact]
    public void OnSessionSwitch_WhenSessionLockAndDisabled_DoesNotLock()
    {
        var auth = new Mock<IAuthorizationService>();
        var settings = new Mock<ISettingsService>();
        var logger = new Mock<ILogger<SessionLockBackgroundService>>();
        settings.SetupGet(s => s.Current).Returns(new AppSettings { LockOnSessionLock = false });

        var sut = new SessionLockBackgroundService(auth.Object, settings.Object, logger.Object);

        InvokeSessionSwitch(sut, SessionSwitchReason.SessionLock);

        auth.Verify(a => a.Lock(), Times.Never);
    }

    [Fact]
    public void OnSessionSwitch_WhenReasonIsNotSessionLock_DoesNotLock()
    {
        var auth = new Mock<IAuthorizationService>();
        var settings = new Mock<ISettingsService>();
        var logger = new Mock<ILogger<SessionLockBackgroundService>>();
        settings.SetupGet(s => s.Current).Returns(new AppSettings { LockOnSessionLock = true });

        var sut = new SessionLockBackgroundService(auth.Object, settings.Object, logger.Object);

        InvokeSessionSwitch(sut, SessionSwitchReason.SessionUnlock);

        auth.Verify(a => a.Lock(), Times.Never);
    }

    [Fact]
    public void OnSessionSwitch_WhenLockThrows_LogsCritical()
    {
        var auth = new Mock<IAuthorizationService>();
        var settings = new Mock<ISettingsService>();
        var logger = new Mock<ILogger<SessionLockBackgroundService>>();
        settings.SetupGet(s => s.Current).Returns(new AppSettings { LockOnSessionLock = true });
        auth.Setup(a => a.Lock()).Throws(new InvalidOperationException("boom"));

        var sut = new SessionLockBackgroundService(auth.Object, settings.Object, logger.Object);

        InvokeSessionSwitch(sut, SessionSwitchReason.SessionLock);

        VerifyLog(logger, LogLevel.Critical, Times.Once());
    }

    private static void InvokeSessionSwitch(SessionLockBackgroundService sut, SessionSwitchReason reason)
    {
        var method = typeof(SessionLockBackgroundService)
            .GetMethod("OnSessionSwitch", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.NotNull(method);

        var args = new SessionSwitchEventArgs(reason);
        method.Invoke(sut, [new object(), args]);
    }

    private static void VerifyLog(Mock<ILogger<SessionLockBackgroundService>> logger, LogLevel level, Times times)
    {
        logger.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == level),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((_, _) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            times);
    }
}
