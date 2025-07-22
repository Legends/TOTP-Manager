using Github2FA.Services;

namespace Github2FA.Tests.Utilities
{
    public class DelayServiceTests
    {
        [Fact]
        public async Task Delay_ShouldWaitAtLeastSpecifiedTime()
        {
            // Arrange
            var delayService = new DelayService();
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Act
            await delayService.Delay(500);

            // Assert
            stopwatch.Stop();
            Assert.True(stopwatch.ElapsedMilliseconds >= 500);
        }
    }

}
