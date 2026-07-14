using Microsoft.Extensions.Logging;
using Moq;
using TOTP.Core.Models;
using TOTP.Core.Security;
using TOTP.Core.Security.Interfaces;
using TOTP.Core.Security.Models;
using TOTP.Presentation.Services;
using TOTP.Services.Interfaces;
using TOTP.Views.Interfaces;

namespace TOTP.Tests.Infrastructure.Services;

public sealed class MainViewSessionControllerTests
{
    [Fact]
    public async Task InitializeAsync_PreparesLockedSessionWithoutPromptingForHello()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var (sut, auth, _, _, _) = CreateSut(lockOnMinimize: true);
        var window = new Mock<IMainWindow>();

        await sut.InitializeAsync(window.Object, cancellationToken);

        auth.Verify(a => a.InitializeAsync(), Times.Once);
        auth.Verify(a => a.TryUnlockOnStartupAsync(It.IsAny<CancellationToken>()), Times.Never);
        Assert.Equal(AppSessionLockState.Locked, sut.SessionState);
    }

    [Fact]
    public async Task TryUnlockOnStartupAsync_PromptsAfterPreparationAndRestoresLockedStateOnCancellation()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var (sut, auth, _, _, _) = CreateSut(lockOnMinimize: true);
        auth.Setup(a => a.TryUnlockOnStartupAsync(cancellationToken))
            .ReturnsAsync(AuthorizationResult.Cancelled);
        var observedStates = new List<AppSessionLockState>();
        sut.SessionStateChanged += (_, state) => observedStates.Add(state);

        var result = await sut.TryUnlockOnStartupAsync(cancellationToken);

        Assert.Equal(AuthorizationResult.Cancelled, result);
        Assert.Equal([AppSessionLockState.Unlocking, AppSessionLockState.Locked], observedStates);
        Assert.Equal(AppSessionLockState.Locked, sut.SessionState);
    }

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
        window.Verify(w => w.BringToFront(), Times.Once);
    }

    [Fact]
    public async Task AuthorizationStateChanged_WhenUnlocked_BringsWindowForwardBeforePostUnlockLoadingCompletes()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var (sut, _, _, state, _) = CreateSut(lockOnMinimize: true);
        var window = new Mock<IMainWindow>();
        sut.AttachWindow(window.Object);
        var allowPostUnlockToComplete = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        sut.ConfigureCallbacks(
            onUnlockedAsync: () => allowPostUnlockToComplete.Task,
            onLocked: () => { });

        state.Unlock();
        await WaitUntilAsync(
            () => window.Invocations.Any(i => i.Method.Name == nameof(IMainWindow.BringToFront)));

        window.Verify(w => w.BringToFront(), Times.Once);
        Assert.False(allowPostUnlockToComplete.Task.IsCompleted);

        allowPostUnlockToComplete.TrySetResult();
        await allowPostUnlockToComplete.Task.WaitAsync(cancellationToken);
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

    [Fact]
    public void BringWindowToFront_WhenAttachedWindowIsNotWpfWindow_CallsBringToFront()
    {
        var (sut, _, _, _, _) = CreateSut(lockOnMinimize: true);
        var window = new Mock<IMainWindow>();
        sut.AttachWindow(window.Object);

        var bringToFront = typeof(MainViewSessionController).GetMethod(
            "BringWindowToFront",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        Assert.NotNull(bringToFront);

        bringToFront!.Invoke(sut, null);

        window.Verify(w => w.BringToFront(), Times.Once);
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

    private static async Task WaitUntilAsync(Func<bool> condition, int timeoutMs = 1500)
    {
        var start = Environment.TickCount64;
        while (!condition())
        {
            if (Environment.TickCount64 - start > timeoutMs)
            {
                throw new TimeoutException("Condition was not met in time.");
            }

            await Task.Delay(20);
        }
    }

    private static void RunInSta(Action testBody)
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                testBody();
            }
            catch (Exception ex)
            {
                failure = ex;
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        if (failure != null)
        {
            throw failure;
        }
    }

}
