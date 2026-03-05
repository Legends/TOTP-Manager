using Microsoft.Extensions.Logging;
using Moq;
using TOTP.Core.Models;
using TOTP.Core.Security;
using TOTP.Core.Security.Interfaces;
using TOTP.Core.Security.Models;
using TOTP.Security;
using TOTP.Services.Interfaces;
using TOTP.Views.Interfaces;

namespace TOTP.Tests.Infrastructure.Services;

public sealed class MainViewSessionControllerTests
{
    [Fact]
    public void AttachAndDetachWindow_UpdateCommandStateAndMonitorBindings()
    {
        var (sut, _, monitor, _, _) = CreateSut(lockOnMinimize: true);
        var window = new Mock<IMainWindow>();
        window.SetupGet(w => w.IsActive).Returns(true);

        Assert.False(sut.DetachWindowCommand.CanExecute(null));
        Assert.False(sut.WindowStateChangedCommand.CanExecute(System.Windows.WindowState.Minimized));

        sut.AttachWindow(window.Object);

        Assert.True(sut.DetachWindowCommand.CanExecute(null));
        Assert.True(sut.WindowStateChangedCommand.CanExecute(System.Windows.WindowState.Minimized));
        monitor.Verify(m => m.Attach(window.Object), Times.Once);

        sut.DetachWindowCommand.Execute(null);

        Assert.False(sut.DetachWindowCommand.CanExecute(null));
        Assert.False(sut.WindowStateChangedCommand.CanExecute(System.Windows.WindowState.Minimized));
        monitor.Verify(m => m.Detach(), Times.Once);
    }

    [Fact]
    public void WindowStateChangedCommand_Minimized_LocksWhenConfigured()
    {
        var (sut, auth, _, _, _) = CreateSut(lockOnMinimize: true);
        var window = new Mock<IMainWindow>();
        window.SetupGet(w => w.IsActive).Returns(true);
        sut.AttachWindow(window.Object);

        sut.WindowStateChangedCommand.Execute(System.Windows.WindowState.Minimized);

        auth.Verify(a => a.Lock(), Times.Once);
    }

    [Fact]
    public async Task AuthorizationStateChanged_WhenUnlocked_CallsOnUnlockedAndReattachesMonitor()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var (sut, _, monitor, state, _) = CreateSut(lockOnMinimize: true);
        var window = new Mock<IMainWindow>();
        window.SetupGet(w => w.IsActive).Returns(true);
        sut.AttachWindow(window.Object);

        var unlockedCalled = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        sut.ConfigureCallbacks(
            onUnlockedAsync: () =>
            {
                unlockedCalled.TrySetResult(true);
                return Task.CompletedTask;
            },
            onLocked: () => { });

        state.Unlock();
        await WaitFor(unlockedCalled.Task, cancellationToken: cancellationToken);

        Assert.Equal(AppSessionLockState.Unlocked, sut.SessionState);
        monitor.Verify(m => m.Attach(window.Object), Times.AtLeast(2));
    }

    [Fact]
    public async Task AuthorizationStateChanged_WhenLocked_CallsOnLockedAndDetachesMonitor()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var (sut, _, monitor, state, _) = CreateSut(lockOnMinimize: true);
        var window = new Mock<IMainWindow>();
        window.SetupGet(w => w.IsActive).Returns(true);
        sut.AttachWindow(window.Object);

        var lockedCalled = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        sut.ConfigureCallbacks(
            onUnlockedAsync: () => Task.CompletedTask,
            onLocked: () => lockedCalled.TrySetResult(true));

        state.Unlock();
        await Task.Delay(50, cancellationToken);
        state.Lock();
        await WaitFor(lockedCalled.Task, cancellationToken: cancellationToken);

        Assert.Equal(AppSessionLockState.Locked, sut.SessionState);
        monitor.Verify(m => m.Detach(), Times.AtLeastOnce);
    }

    private static async Task WaitFor(Task task, int timeoutMs = 1500, CancellationToken cancellationToken = default)
    {
        var completed = await Task.WhenAny(task, Task.Delay(timeoutMs, cancellationToken));
        if (completed != task)
        {
            throw new TimeoutException("Expected callback was not observed in time.");
        }

        await task.WaitAsync(cancellationToken);
    }

    private static (
        MainViewSessionController sut,
        Mock<IAuthorizationService> auth,
        Mock<IInputActivityMonitor> monitor,
        AuthorizationState state,
        Mock<ISettingsService> settings) CreateSut(bool lockOnMinimize)
    {
        var state = new AuthorizationState();
        var auth = new Mock<IAuthorizationService>();
        auth.SetupGet(a => a.State).Returns(state);

        var monitor = new Mock<IInputActivityMonitor>();
        var settings = new Mock<ISettingsService>();
        settings.SetupGet(s => s.Current).Returns(new AppSettings { LockOnMinimize = lockOnMinimize });

        var logger = new Mock<ILogger<MainViewSessionController>>();
        var sut = new MainViewSessionController(auth.Object, monitor.Object, settings.Object, logger.Object);
        return (sut, auth, monitor, state, settings);
    }
}
