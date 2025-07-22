namespace TOTP.Tests.Utilities;

using TOTP.Helper;
using System.Threading.Tasks;
using Xunit;

public class DebounceDispatcherTests
{
    //[StaFact]
    //public async Task Debounce_ShouldExecuteAction_AfterDelay()
    //{
    //    var dispatcher = new DebounceDispatcher();
    //    bool wasCalled = false;

    //    dispatcher.Debounce(100, () => wasCalled = true);

    //    // Wait for more than debounce interval
    //     Task.Delay(150).Wait();
    //    //await Task.Delay(150);
    //    DispatcherUtil.DoEvents();

    //    Assert.True(wasCalled);
    //}
    //[StaFact]
    [UIFact]
    public async Task Debounce_ShouldExecuteAction_AfterDelay()
    {
        var dispatcher = new DebounceDispatcher();
        var tcs = new TaskCompletionSource<bool>();

        dispatcher.Debounce(100, () => tcs.SetResult(true));

        await Task.Delay(150);
        DispatcherUtil.DoEvents();

        Assert.True(await tcs.Task);
    }


    //[StaFact]
    [UIFact]
    public async Task Debounce_ShouldNotExecuteAction_AfterDelay()
    {
        var dispatcher = new DebounceDispatcher();
        bool wasCalled = false;

        dispatcher.Debounce(100, () => wasCalled = true);

        // Wait for more than debounce interval
        //Task.Delay(50).Wait();
        await Task.Delay(50);
        DispatcherUtil.DoEvents();

        Assert.False(wasCalled);
    }


    [UIFact]
    public async Task Debounce_ShouldOnlyExecuteLatestAction_WhenCalledMultipleTimes()
    {
        // Arrange
        var debounce = new DebounceDispatcher();
        string? result = null;

        // Act
        debounce.Debounce(100, () => result = "First");
        debounce.Debounce(100, () => result = "Second");
        debounce.Debounce(100, () => result = "Third");

        // Wait for the timer to tick
        //Task.Delay(150).Wait();
        await Task.Delay(150);
        DispatcherUtil.DoEvents();
        // Assert
        Assert.Equal("Third", result);
    }

    [UIFact]
    public async Task Debounce_ShouldResetTimer_WhenCalledAgain()
    {
        // Arrange
        var debounce = new DebounceDispatcher();
        bool wasCalled = false;

        // Act
        debounce.Debounce(100, () => wasCalled = true);

        // Wait 50 ms, call again (should reset timer)
        await Task.Delay(50);
        DispatcherUtil.DoEvents();
        debounce.Debounce(100, () => wasCalled = true);

        // Wait 75 ms: total since 2nd call = 75ms, should NOT have fired yet
        await Task.Delay(75);
        DispatcherUtil.DoEvents();
        Assert.False(wasCalled);

        // Wait additional time so it can fire
        await Task.Delay(50);
        DispatcherUtil.DoEvents();
        Assert.True(wasCalled);
    }

    [UIFact]
    public async Task Debounce_ShouldInvokeActionOnlyOnce()
    {
        // Arrange
        var debounce = new DebounceDispatcher();
        int count = 0;

        // Act
        debounce.Debounce(50, () => count++);
        debounce.Debounce(50, () => count++);
        debounce.Debounce(50, () => count++);

        // Wait for the timer
        await Task.Delay(100);
        DispatcherUtil.DoEvents();
        // Assert
        Assert.Equal(1, count);
    }
}
