using TOTP.Core.Platform;
using TOTP.Infrastructure.Services;

namespace TOTP.Tests.Unix.Infrastructure;

public sealed class SingleInstanceTransportTests
{
    [Fact]
    public async Task NamedPipe_RoundTripsActivationWithinCurrentUser()
    {
        var pipeName = $"totp-test-{Guid.NewGuid():N}";
        var received = new TaskCompletionSource<ApplicationActivationRequest>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        using var listener = new NamedPipeActivationListener(pipeName);
        listener.Start(request => received.TrySetResult(request));

        Assert.True(new NamedPipeActivationDispatcher(pipeName)
            .TryDispatch(ApplicationActivationRequest.ActivateMainWindow()));
        var request = await received.Task.WaitAsync(
            TimeSpan.FromSeconds(2),
            TestContext.Current.CancellationToken);

        Assert.Equal(ApplicationActivationKind.ActivateMainWindow, request.Kind);
    }

    [Fact]
    public async Task NamedMutex_BlocksAnotherThread()
    {
        var name = $"totp-test-{Guid.NewGuid():N}";
        using var primary = new NamedMutexInstanceLock(name);
        Assert.NotEqual(InstanceLockAcquireResult.AlreadyRunning, primary.Acquire());

        var secondary = await Task.Run(() =>
        {
            using var instanceLock = new NamedMutexInstanceLock(name);
            return instanceLock.Acquire();
        }, TestContext.Current.CancellationToken);

        Assert.Equal(InstanceLockAcquireResult.AlreadyRunning, secondary);
    }
}
