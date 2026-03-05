using Microsoft.Extensions.Logging;
using Moq;
using TOTP.Core.Security;
using TOTP.Core.Security.Interfaces;
using TOTP.Core.Security.Models;
using TOTP.ViewModels;

namespace TOTP.Tests.ViewModels;

public sealed class UnlockViewModelTests
{
    [Fact]
    public void Ctor_WhenNotConfigured_CurrentGateIsNull()
    {
        var vm = CreateSut(out _, out _, out _);

        Assert.False(vm.IsConfigured);
        Assert.Null(vm.CurrentGate);
        Assert.False(vm.HasSelectedSetupGate);
    }

    [Fact]
    public void StateChange_WhenConfiguredPassword_CurrentGateSwitchesToPasswordVm()
    {
        var vm = CreateSut(out var auth, out _, out var passwordVm);

        auth.Object.State.SetProfile(new AuthorizationProfile { Gate = AuthorizationGateKind.Password });

        Assert.True(vm.IsConfigured);
        Assert.Same(passwordVm, vm.CurrentGate);
    }

    [Fact]
    public void ChoosePasswordCommand_EntersSetupAndSetsCurrentGate()
    {
        var vm = CreateSut(out _, out _, out var passwordVm);

        vm.ChoosePasswordCommand.Execute(null);

        Assert.Same(passwordVm, vm.CurrentGate);
        Assert.True(passwordVm.IsSetup);
    }

    [Fact]
    public async Task ChooseHelloCommand_WhenNotAvailable_SetsStatusMessage()
    {
        var vm = CreateSut(out var auth, out _, out _);
        auth.Setup(a => a.ConfigureHelloAsync()).ReturnsAsync(AuthorizationResult.NotAvailable);

        vm.ChooseHelloCommand.Execute(null);
        await WaitUntilAsync(() => !string.IsNullOrWhiteSpace(vm.StatusMessage));

        Assert.Equal("Windows Hello is not available on this device/account. Choose Password.", vm.StatusMessage);
    }

    [Fact]
    public async Task ChooseHelloCommand_WhenConfigureFails_SetsFailureMessage()
    {
        var vm = CreateSut(out var auth, out _, out _);
        auth.Setup(a => a.ConfigureHelloAsync()).ReturnsAsync(AuthorizationResult.Failed);

        vm.ChooseHelloCommand.Execute(null);
        await WaitUntilAsync(() => !string.IsNullOrWhiteSpace(vm.StatusMessage));

        Assert.Equal("Failed to configure Windows Hello.", vm.StatusMessage);
    }

    [Fact]
    public async Task ChooseHelloCommand_WhenUnlockFailsAfterSetup_SetsVerificationMessage()
    {
        var vm = CreateSut(out var auth, out _, out _);
        auth.Setup(a => a.ConfigureHelloAsync()).ReturnsAsync(AuthorizationResult.Success);
        auth.Setup(a => a.TryUnlockWithHelloAsync()).ReturnsAsync(AuthorizationResult.InvalidCredentials);

        vm.ChooseHelloCommand.Execute(null);
        await WaitUntilAsync(() => !string.IsNullOrWhiteSpace(vm.StatusMessage));

        Assert.Equal("Hello verification failed. Try again or use Password if configured.", vm.StatusMessage);
    }

    [Fact]
    public async Task ChooseHelloCommand_WhenSetupAndUnlockSucceed_LeavesStatusMessageEmpty()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var vm = CreateSut(out var auth, out _, out _);
        auth.Setup(a => a.ConfigureHelloAsync()).ReturnsAsync(AuthorizationResult.Success);
        auth.Setup(a => a.TryUnlockWithHelloAsync()).ReturnsAsync(AuthorizationResult.Success);

        vm.StatusMessage = "old";
        vm.ChooseHelloCommand.Execute(null);

        await Task.Delay(80, cancellationToken);

        Assert.True(string.IsNullOrWhiteSpace(vm.StatusMessage));
    }

    private static UnlockViewModel CreateSut(
        out Mock<IAuthorizationService> auth,
        out HelloUnlockViewModel helloVm,
        out PasswordUnlockViewModel passwordVm)
    {
        auth = new Mock<IAuthorizationService>();
        var state = new AuthorizationState();
        auth.SetupGet(a => a.State).Returns(state);

        var validator = new Mock<IPasswordValidationService>();
        validator.Setup(v => v.ValidateRequired(It.IsAny<string?>(), It.IsAny<string>()))
            .Returns(new PasswordValidationResult());
        validator.Setup(v => v.ValidateNewWithConfirmation(
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Returns(new PasswordValidationResult());

        helloVm = new HelloUnlockViewModel(auth.Object);
        passwordVm = new PasswordUnlockViewModel(auth.Object, validator.Object, Mock.Of<ILogger<PasswordUnlockViewModel>>());

        return new UnlockViewModel(
            auth.Object,
            helloVm,
            passwordVm,
            Mock.Of<ISettingsService>());
    }

    private static async Task WaitUntilAsync(Func<bool> condition, int timeoutMs = 1200)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var start = Environment.TickCount64;
        while (!condition())
        {
            if (Environment.TickCount64 - start > timeoutMs)
            {
                throw new TimeoutException("Condition was not met in time.");
            }

            await Task.Delay(20, cancellationToken);
        }
    }
}
