using TOTP.Core.Platform;
using TOTP.Presentation.Platform;

namespace TOTP.Tests.Infrastructure;

public sealed class WindowsNamedMutexInstanceLockTests
{
    [Fact]
    public void Acquire_WhenNameIsNew_ReturnsAcquired()
    {
        using var sut = CreateLock();

        Assert.Equal(InstanceLockAcquireResult.Acquired, sut.Acquire());
    }

    [Fact]
    public void Acquire_WhenUnlockedNamedObjectSurvives_ReturnsRecovered()
    {
        var name = $"totp-stale-{Guid.NewGuid():N}";
        using var staleHandle = new Mutex(false, $@"Local\{name}", out var createdNew);
        using var sut = new WindowsNamedMutexInstanceLock(name, globalNamespace: false);

        var result = sut.Acquire();

        Assert.True(createdNew);
        Assert.Equal(InstanceLockAcquireResult.Recovered, result);
    }

    [Fact]
    public void Acquire_WhenOwnerThreadExited_ReturnsRecovered()
    {
        var name = $"totp-crash-{Guid.NewGuid():N}";
        var crashedOwner = new WindowsNamedMutexInstanceLock(name, globalNamespace: false);
        var ownerResult = InstanceLockAcquireResult.AlreadyRunning;
        var ownerThread = new Thread(() => ownerResult = crashedOwner.Acquire());
        ownerThread.Start();
        ownerThread.Join();

        using var recovered = new WindowsNamedMutexInstanceLock(name, globalNamespace: false);
        var result = recovered.Acquire();

        Assert.Equal(InstanceLockAcquireResult.Acquired, ownerResult);
        Assert.Equal(InstanceLockAcquireResult.Recovered, result);
        recovered.Dispose();
        crashedOwner.Dispose();
    }

    [Fact]
    public void Acquire_WhenLiveOwnerHoldsLock_ReturnsAlreadyRunning()
    {
        var name = $"totp-live-{Guid.NewGuid():N}";
        var cancellationToken = TestContext.Current.CancellationToken;
        using var ownerReady = new ManualResetEventSlim();
        using var releaseOwner = new ManualResetEventSlim();
        var owner = new WindowsNamedMutexInstanceLock(name, globalNamespace: false);
        var ownerThread = new Thread(() =>
        {
            owner.Acquire();
            ownerReady.Set();
            releaseOwner.Wait(cancellationToken);
            owner.Dispose();
        }) { IsBackground = true };
        ownerThread.Start();
        Assert.True(ownerReady.Wait(TimeSpan.FromSeconds(2), cancellationToken));

        using var contender = new WindowsNamedMutexInstanceLock(name, globalNamespace: false);
        var result = contender.Acquire();
        releaseOwner.Set();
        ownerThread.Join();

        Assert.Equal(InstanceLockAcquireResult.AlreadyRunning, result);
    }

    private static WindowsNamedMutexInstanceLock CreateLock() =>
        new($"totp-instance-{Guid.NewGuid():N}", globalNamespace: false);
}
