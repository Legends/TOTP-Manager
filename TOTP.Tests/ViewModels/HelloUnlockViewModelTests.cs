using Moq;
using TOTP.Core.Security.Interfaces;
using TOTP.Core.Security.Models;
using TOTP.ViewModels;

namespace TOTP.Tests.ViewModels;

public sealed class HelloUnlockViewModelTests
{
    [Fact]
    public async Task UnlockWithHelloCommand_WhenNotAvailable_SetsNotAvailableMessage()
    {
        var auth = new Mock<IAuthorizationService>();
        auth.SetupGet(a => a.State).Returns(new TOTP.Core.Security.AuthorizationState());
        auth.Setup(a => a.TryUnlockWithHelloAsync()).ReturnsAsync(AuthorizationResult.NotAvailable);

        var vm = new HelloUnlockViewModel(auth.Object);

        vm.UnlockWithHelloCommand.Execute(null);
        await WaitUntilAsync(() => vm.Message is not null);

        Assert.Equal("Windows Hello is not available.", vm.Message);
    }

    [Fact]
    public async Task UnlockWithHelloCommand_WhenFailed_SetsVerificationFailedMessage()
    {
        var auth = new Mock<IAuthorizationService>();
        auth.SetupGet(a => a.State).Returns(new TOTP.Core.Security.AuthorizationState());
        auth.Setup(a => a.TryUnlockWithHelloAsync()).ReturnsAsync(AuthorizationResult.Failed);

        var vm = new HelloUnlockViewModel(auth.Object);

        vm.UnlockWithHelloCommand.Execute(null);
        await WaitUntilAsync(() => vm.Message is not null);

        Assert.Equal("Hello verification failed.", vm.Message);
    }

    [Fact]
    public async Task UnlockWithHelloCommand_WhenSuccess_ClearsExistingMessage()
    {
        var auth = new Mock<IAuthorizationService>();
        auth.SetupGet(a => a.State).Returns(new TOTP.Core.Security.AuthorizationState());
        auth.Setup(a => a.TryUnlockWithHelloAsync()).ReturnsAsync(AuthorizationResult.Success);

        var vm = new HelloUnlockViewModel(auth.Object)
        {
            Message = "old"
        };

        vm.UnlockWithHelloCommand.Execute(null);
        await Task.Delay(80);

        Assert.True(string.IsNullOrWhiteSpace(vm.Message));
    }

    private static async Task WaitUntilAsync(Func<bool> condition, int timeoutMs = 1000)
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
}
