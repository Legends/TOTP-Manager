using Microsoft.Extensions.Logging;
using Moq;
using TOTP.Core.Security;
using TOTP.Core.Security.Interfaces;
using TOTP.Core.Security.Models;
using TOTP.Resources;
using TOTP.ViewModels;

namespace TOTP.Tests.ViewModels;

public sealed class PasswordUnlockViewModelTests
{
    [Fact]
    public void EnterSetupMode_ClearsFieldsMessageAndSetsSetupTrue()
    {
        var vm = CreateSut(out _, out _);
        vm.Password = "abc";
        vm.ConfirmPassword = "def";
        vm.Message = "msg";

        vm.EnterSetupMode();

        Assert.True(vm.IsSetup);
        Assert.Equal(string.Empty, vm.Password);
        Assert.Equal(string.Empty, vm.ConfirmPassword);
        Assert.Null(vm.Message);
    }

    [Fact]
    public void SavePasswordCommand_WhenSetupValidationFails_IsBlockedByCanExecute()
    {
        var vm = CreateSut(out var auth, out var validator);
        vm.EnterSetupMode();
        vm.Password = "short";
        vm.ConfirmPassword = "nope";

        validator.Setup(v => v.ValidateNewWithConfirmation(
                "short",
                "nope",
                UI.ui_Password_Required,
                UI.ui_Password_MinLength_Format,
                UI.ui_Password_ConfirmRequired,
                UI.ui_Password_Mismatch))
            .Returns(new PasswordValidationResult { ConfirmPasswordError = "mismatch" });

        var canExecute = vm.SavePasswordCommand.CanExecute(null);

        Assert.False(canExecute);
        vm.SavePasswordCommand.Execute(null);
        Assert.True(string.IsNullOrWhiteSpace(vm.Message));
        auth.Verify(a => a.ConfigurePasswordAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task UnlockCommand_WhenSetupValidationFails_SetsInlineMessageAndDoesNotConfigure()
    {
        var vm = CreateSut(out var auth, out var validator);
        vm.EnterSetupMode();
        vm.Password = "short";
        vm.ConfirmPassword = "nope";

        validator.Setup(v => v.ValidateNewWithConfirmation(
                "short",
                "nope",
                UI.ui_Password_Required,
                UI.ui_Password_MinLength_Format,
                UI.ui_Password_ConfirmRequired,
                UI.ui_Password_Mismatch))
            .Returns(new PasswordValidationResult { ConfirmPasswordError = "mismatch" });

        vm.UnlockCommand.Execute(null);
        await WaitUntilAsync(() => vm.Message == "mismatch");

        Assert.Equal("mismatch", vm.Message);
        auth.Verify(a => a.ConfigurePasswordAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SavePasswordCommand_WhenSetupConfigureSucceeds_ExitsSetupAndClearsSecrets()
    {
        var vm = CreateSut(out var auth, out var validator);
        vm.EnterSetupMode();
        vm.Password = "StrongPwd1!";
        vm.ConfirmPassword = "StrongPwd1!";

        validator.Setup(v => v.ValidateNewWithConfirmation(
                "StrongPwd1!",
                "StrongPwd1!",
                UI.ui_Password_Required,
                UI.ui_Password_MinLength_Format,
                UI.ui_Password_ConfirmRequired,
                UI.ui_Password_Mismatch))
            .Returns(new PasswordValidationResult());
        auth.Setup(a => a.ConfigurePasswordAsync("StrongPwd1!", "StrongPwd1!"))
            .ReturnsAsync(AuthorizationResult.Success);

        vm.SavePasswordCommand.Execute(null);
        await WaitUntilAsync(() => vm.IsSetup == false && vm.Password == string.Empty);

        Assert.False(vm.IsSetup);
        Assert.Equal(string.Empty, vm.Password);
        Assert.Equal(string.Empty, vm.ConfirmPassword);
        auth.Verify(a => a.ConfigurePasswordAsync("StrongPwd1!", "StrongPwd1!"), Times.Once);
    }

    [Fact]
    public async Task UnlockCommand_WhenUnlockFails_SetsVerificationMessage()
    {
        var vm = CreateSut(out var auth, out _);
        vm.Password = "wrong";
        auth.Setup(a => a.TryUnlockWithPasswordAsync("wrong"))
            .ReturnsAsync(AuthorizationResult.InvalidCredentials);

        vm.UnlockCommand.Execute(null);
        await WaitUntilAsync(() => vm.Message == UI.ui_Password_VerificationFailed);

        Assert.Equal(UI.ui_Password_VerificationFailed, vm.Message);
    }

    [Fact]
    public void UnlockCommand_WhenPasswordEmpty_IsBlockedByCanExecute_AndDoesNotCallAuth()
    {
        var vm = CreateSut(out var auth, out _);
        vm.Password = string.Empty;

        var canExecute = vm.UnlockCommand.CanExecute(null);

        Assert.False(canExecute);
        vm.UnlockCommand.Execute(null);
        Assert.True(string.IsNullOrWhiteSpace(vm.Message));
        auth.Verify(a => a.TryUnlockWithPasswordAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void AuthorizationStateChanged_WhenUnlocked_ClearsPassword()
    {
        var vm = CreateSut(out var auth, out _);
        vm.Password = "to-clear";

        auth.Object.State.Unlock();

        Assert.Equal(string.Empty, vm.Password);
    }

    [Fact]
    public void AuthorizationStateChanged_WhenConfiguredPassword_DisablesSetup()
    {
        var vm = CreateSut(out var auth, out _);
        vm.EnterSetupMode();

        auth.Object.State.SetProfile(new AuthorizationProfile { Gate = AuthorizationGateKind.Password });

        Assert.False(vm.IsSetup);
    }

    private static PasswordUnlockViewModel CreateSut(
        out Mock<IAuthorizationService> auth,
        out Mock<IPasswordValidationService> validator)
    {
        auth = new Mock<IAuthorizationService>();
        validator = new Mock<IPasswordValidationService>();
        var state = new AuthorizationState();

        auth.SetupGet(a => a.State).Returns(state);
        validator.Setup(v => v.ValidateRequired(It.IsAny<string?>(), UI.ui_Password_Required))
            .Returns(new PasswordValidationResult { PasswordError = UI.ui_Password_Required });
        validator.Setup(v => v.ValidateNewWithConfirmation(
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                UI.ui_Password_Required,
                UI.ui_Password_MinLength_Format,
                UI.ui_Password_ConfirmRequired,
                UI.ui_Password_Mismatch))
            .Returns(new PasswordValidationResult { PasswordError = UI.ui_Password_Required });

        return new PasswordUnlockViewModel(auth.Object, validator.Object, Mock.Of<ILogger<PasswordUnlockViewModel>>());
    }

    private static async Task WaitUntilAsync(Func<bool> condition, int timeoutMs = 1200)
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
