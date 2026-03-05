using System.Threading.Tasks;
using TOTP.Services;

namespace TOTP.Tests.Utilities;

public class DebounceServiceTests
{
    [Fact]
    public async Task Debounce_ShouldExecuteAction_AfterDelay()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var service = new DebounceService();
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        service.Debounce("test", 50, () => tcs.TrySetResult(true));

        var completed = await Task.WhenAny(tcs.Task, Task.Delay(1000, cancellationToken));

        Assert.Same(tcs.Task, completed);
        Assert.True(await tcs.Task.WaitAsync(cancellationToken));
    }

    [Fact]
    public async Task Debounce_ShouldOnlyExecuteLatestAction_WhenCalledMultipleTimes()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var service = new DebounceService();
        var firstCalled = false;
        var secondCalled = false;
        var thirdCalled = false;
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        service.Debounce("test", 100, () => firstCalled = true);
        service.Debounce("test", 100, () => secondCalled = true);
        service.Debounce("test", 50, () =>
        {
            thirdCalled = true;
            tcs.TrySetResult(true);
        });

        var completed = await Task.WhenAny(tcs.Task, Task.Delay(1000, cancellationToken));

        Assert.Same(tcs.Task, completed);
        Assert.False(firstCalled);
        Assert.False(secondCalled);
        Assert.True(thirdCalled);
    }

    [Fact]
    public async Task Cancel_ShouldPreventAction()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var service = new DebounceService();
        var wasCalled = false;

        service.Debounce("test", 100, () => wasCalled = true);
        service.Cancel("test");

        await Task.Delay(200, cancellationToken);

        Assert.False(wasCalled);
    }

    [Fact]
    public void Dispose_ShouldCancelPendingDebounces()
    {
        var service = new DebounceService();

        service.Debounce("test", 1000, () => { });
        service.Dispose();

        var tokens = GetPrivateField<System.Collections.Concurrent.ConcurrentDictionary<string, System.Threading.CancellationTokenSource>>(service, "_tokens");
        Assert.Empty(tokens);
    }

    [Fact]
    public void Debounce_AfterDispose_ShouldThrowObjectDisposedException()
    {
        var service = new DebounceService();
        service.Dispose();

        Assert.Throws<ObjectDisposedException>(() => service.Debounce("test", 10, () => { }));
    }

    private static T GetPrivateField<T>(object instance, string fieldName)
    {
        var field = instance.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.NotNull(field);
        return (T)field.GetValue(instance)!;
    }
}
