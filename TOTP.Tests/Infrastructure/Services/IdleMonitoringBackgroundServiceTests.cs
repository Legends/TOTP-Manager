using FluentResults;
using Microsoft.Extensions.Logging;
using Moq;
using TOTP.Core.Models;
using TOTP.Core.Security;
using TOTP.Core.Security.Interfaces;
using TOTP.Infrastructure.Services;

namespace TOTP.Tests.Infrastructure.Services;

public sealed class IdleMonitoringBackgroundServiceTests
{
    [Fact]
    public async Task ExecuteAsync_WhenUnlockedAndIdleTimeoutReached_CallsLock()
    {
        var state = new AuthorizationState();
        state.Unlock();

        var auth = new Mock<IAuthorizationService>();
        auth.SetupGet(a => a.State).Returns(state);
        auth.Setup(a => a.Lock()).Callback(state.Lock);

        var settings = new Mock<ISettingsService>();
        settings.SetupGet(s => s.Current).Returns(new AppSettings { IdleTimeout = TimeSpan.FromMilliseconds(100) });
        settings.Setup(s => s.LoadAsync()).ReturnsAsync(Result.Ok<IAppSettings>(settings.Object.Current));

        var logger = new Mock<ILogger<IdleMonitoringBackgroundService>>();
        var sut = new IdleMonitoringBackgroundService(auth.Object, settings.Object, logger.Object);
        typeof(IdleMonitoringBackgroundService)
            .GetField("_wasUnlocked", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .SetValue(sut, true);
        typeof(IdleMonitoringBackgroundService)
            .GetField("<LastActivity>k__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .SetValue(sut, DateTime.UtcNow - TimeSpan.FromMinutes(1));

        await sut.StartAsync(CancellationToken.None);
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(6));
        }
        finally
        {
            await sut.StopAsync(CancellationToken.None);
        }

        auth.Verify(a => a.Lock(), Times.AtLeastOnce);
    }

    [Fact]
    public async Task ExecuteAsync_WhenIdleTimeoutDisabled_DoesNotLock()
    {
        var state = new AuthorizationState();
        state.Unlock();

        var auth = new Mock<IAuthorizationService>();
        auth.SetupGet(a => a.State).Returns(state);

        var settings = new Mock<ISettingsService>();
        settings.SetupGet(s => s.Current).Returns(new AppSettings { IdleTimeout = TimeSpan.Zero });
        settings.Setup(s => s.LoadAsync()).ReturnsAsync(Result.Ok<IAppSettings>(settings.Object.Current));

        var logger = new Mock<ILogger<IdleMonitoringBackgroundService>>();
        var sut = new IdleMonitoringBackgroundService(auth.Object, settings.Object, logger.Object);

        await sut.StartAsync(CancellationToken.None);
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(6));
        }
        finally
        {
            await sut.StopAsync(CancellationToken.None);
        }

        auth.Verify(a => a.Lock(), Times.Never);
    }
}
