using Moq;
using TOTP.Core.Security.Interfaces;
using TOTP.Core.Security.Models;
using TOTP.Resources;
using TOTP.ViewModels;

namespace TOTP.Tests.ViewModels;

public sealed class PasswordPromptViewModelTests
{
    [Fact]
    public async Task ConfirmCommand_WithEmptyPassword_UsesCustomRequiredErrorIfProvided()
    {
        var validation = new Mock<IPasswordValidationService>();
        validation
            .Setup(v => v.ValidateRequired("", "required"))
            .Returns(new PasswordValidationResult { PasswordError = "required" });

        var vm = new PasswordPromptViewModel("Title", "Msg", validation.Object, requiredErrorMessage: "required");

        var closed = false;
        vm.RequestClose += (_, _) => closed = true;

        vm.ConfirmCommand.Execute(null);
        await WaitUntilAsync(() => vm.HasErrorMessage);

        Assert.Equal("required", vm.ErrorMessage);
        Assert.False(closed);
    }

    [Fact]
    public async Task ConfirmCommand_WithEmptyPassword_UsesDefaultRequiredResourceWhenMissingCustom()
    {
        var validation = new Mock<IPasswordValidationService>();
        validation
            .Setup(v => v.ValidateRequired("", UI.ui_ImportPasswordRequired))
            .Returns(new PasswordValidationResult { PasswordError = UI.ui_ImportPasswordRequired });

        var vm = new PasswordPromptViewModel("Title", "Msg", validation.Object);

        vm.ConfirmCommand.Execute(null);
        await WaitUntilAsync(() => vm.HasErrorMessage);

        Assert.Equal(UI.ui_ImportPasswordRequired, vm.ErrorMessage);
    }

    [Fact]
    public async Task ConfirmCommand_WhenValidatorReturnsError_SetsErrorAndDoesNotClose()
    {
        var validation = new Mock<IPasswordValidationService>();
        validation
            .Setup(v => v.ValidateRequired("pw", UI.ui_ImportPasswordRequired))
            .Returns(new PasswordValidationResult());

        var vm = new PasswordPromptViewModel("Title", "Msg", validation.Object)
        {
            Password = "pw",
            ValidatePasswordAsync = _ => Task.FromResult<string?>("bad password")
        };

        var closed = false;
        vm.RequestClose += (_, _) => closed = true;

        vm.ConfirmCommand.Execute(null);
        await WaitUntilAsync(() => vm.ErrorMessage == "bad password");

        Assert.Equal("bad password", vm.ErrorMessage);
        Assert.False(closed);
        Assert.Equal(string.Empty, vm.Password);
    }

    [Fact]
    public async Task ConfirmCommand_WhenValidatorPasses_ClosesDialog()
    {
        var validation = new Mock<IPasswordValidationService>();
        validation
            .Setup(v => v.ValidateRequired("pw", UI.ui_ImportPasswordRequired))
            .Returns(new PasswordValidationResult());

        var vm = new PasswordPromptViewModel("Title", "Msg", validation.Object)
        {
            Password = "pw",
            ValidatePasswordAsync = _ => Task.FromResult<string?>(null)
        };

        var closed = false;
        vm.RequestClose += (_, _) => closed = true;

        vm.ConfirmCommand.Execute(null);
        await WaitUntilAsync(() => closed);

        Assert.True(closed);
        Assert.False(vm.HasErrorMessage);
        Assert.Equal(string.Empty, vm.Password);
    }

    [Fact]
    public void PasswordSetter_ClearsExistingErrorMessage()
    {
        var vm = new PasswordPromptViewModel("Title", "Msg", Mock.Of<IPasswordValidationService>(), errorMessage: "initial");

        Assert.True(vm.HasErrorMessage);

        vm.Password = "new";

        Assert.Equal(string.Empty, vm.ErrorMessage);
        Assert.False(vm.HasErrorMessage);
    }

    [Fact]
    public void Constructor_SetsInitialProperties()
    {
        var vm = new PasswordPromptViewModel("T", "M", Mock.Of<IPasswordValidationService>(), errorMessage: "E", requiredErrorMessage: "R");

        Assert.Equal("T", vm.Title);
        Assert.Equal("M", vm.Message);
        Assert.Equal("E", vm.ErrorMessage);
        Assert.Equal("R", vm.RequiredErrorMessage);
        Assert.True(vm.HasErrorMessage);
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
