using System.Threading.Tasks;
using System.Windows;
using TOTP.Services;

namespace TOTP.Tests.Utilities;

public class DebounceServiceTests
{
    [UIFact]
    public async Task Debounce_ShouldExecuteAction_AfterDelay()
    {
        EnsureApplication();
        var service = new DebounceService();
        var tcs = new TaskCompletionSource<bool>();

        service.Debounce("test", 100, () => tcs.TrySetResult(true));

        await Task.Delay(150);
        DispatcherUtil.DoEvents();

        Assert.True(await tcs.Task);
    }

    [UIFact]
    public async Task Debounce_ShouldOnlyExecuteLatestAction_WhenCalledMultipleTimes()
    {
        EnsureApplication();
        var service = new DebounceService();
        string? result = null;

        service.Debounce("test", 100, () => result = "First");
        service.Debounce("test", 100, () => result = "Second");
        service.Debounce("test", 100, () => result = "Third");

        await Task.Delay(150);
        DispatcherUtil.DoEvents();

        Assert.Equal("Third", result);
    }

    [UIFact]
    public async Task Cancel_ShouldPreventAction()
    {
        EnsureApplication();
        var service = new DebounceService();
        var wasCalled = false;

        service.Debounce("test", 100, () => wasCalled = true);
        service.Cancel("test");

        await Task.Delay(150);
        DispatcherUtil.DoEvents();

        Assert.False(wasCalled);
    }

    private static void EnsureApplication()
    {
        if (Application.Current == null)
        {
            _ = new Application();
        }
    }
}
