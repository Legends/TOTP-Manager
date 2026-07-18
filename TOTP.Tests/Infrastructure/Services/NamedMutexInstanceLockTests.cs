using TOTP.Core.Platform;
using TOTP.Infrastructure.Services;

namespace TOTP.Tests.Infrastructure.Services;

public sealed class NamedMutexInstanceLockTests
{
    [Fact]
    public async Task Acquire_BlocksAnotherThreadUntilOwnerDisposes()
    {
        var name = $"totp-test-{Guid.NewGuid():N}";
        using var primary = new NamedMutexInstanceLock(name);
        Assert.Contains(
            primary.Acquire(),
            new[] { InstanceLockAcquireResult.Acquired, InstanceLockAcquireResult.Recovered });

        var secondOutcome = await Task.Run(() =>
        {
            using var secondary = new NamedMutexInstanceLock(name);
            return secondary.Acquire();
        }, TestContext.Current.CancellationToken);

        Assert.Equal(InstanceLockAcquireResult.AlreadyRunning, secondOutcome);
    }
}
