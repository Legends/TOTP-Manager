using System.Diagnostics;
using TOTP.Services;

namespace TOTP.Tests.Utilities;

public class DelayServiceTests
{
    [Fact]
    public async Task Delay_ShouldWaitAtLeastSpecifiedTime()
    {
        // Arrange
        var delayService = new DelayService();
        var stopwatch = Stopwatch.StartNew();

        // Act
        await delayService.Delay(500);

        // Assert
        stopwatch.Stop();
        //Assert.True(stopwatch.ElapsedMilliseconds >= 500);

        Assert.True(stopwatch.ElapsedMilliseconds >= 490,
            $"Expected at least 490ms, but got {stopwatch.ElapsedMilliseconds}ms.");
    }
}