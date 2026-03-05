using Moq;
using TOTP.Core.Security.Interfaces;
using TOTP.Core.Security.Models;
using TOTP.Resources;
using TOTP.ViewModels;

namespace TOTP.Tests.ViewModels;

public sealed class ExportPasswordPromptViewModelTests
{
    [Fact]
    public async Task ConfirmCommand_MasterPasswordRequiredValidationFails_SetsErrorAndDoesNotClose()
    {
        var validation = new Mock<IPasswordValidationService>();
        validation.Setup(v => v.ValidateRequired("", UI.ui_ExportPasswordRequired))
            .Returns(new PasswordValidationResult { PasswordError = "required" });

        var vm = new ExportPasswordPromptViewModel("Export", validation.Object)
        {
            MasterPassword = ""
        };

        var closed = false;
        vm.RequestClose += (_, _) => closed = true;

        vm.ConfirmCommand.Execute(null);
        await WaitUntilAsync(() => vm.ErrorMessage == "required");

        Assert.Equal("required", vm.ErrorMessage);
        Assert.False(closed);
        Assert.Equal(string.Empty, vm.SelectedPassword);
    }

    [Fact]
    public async Task ConfirmCommand_MasterPasswordWrong_SetsWrongPasswordMessage()
    {
        var validation = new Mock<IPasswordValidationService>();
        validation.Setup(v => v.ValidateRequired("master", UI.ui_ExportPasswordRequired))
            .Returns(new PasswordValidationResult());

        var vm = new ExportPasswordPromptViewModel("Export", validation.Object)
        {
            MasterPassword = "master",
            ValidateMasterPasswordAsync = _ => Task.FromResult(false)
        };

        var closed = false;
        vm.RequestClose += (_, _) => closed = true;

        vm.ConfirmCommand.Execute(null);
        await WaitUntilAsync(() => !string.IsNullOrWhiteSpace(vm.ErrorMessage));

        Assert.Equal(UI.ui_ExportPwd_WrongMasterPassword, vm.ErrorMessage);
        Assert.False(closed);
    }

    [Fact]
    public async Task ConfirmCommand_MasterPasswordValid_ClosesAndSetsSelectedPassword()
    {
        var validation = new Mock<IPasswordValidationService>();
        validation.Setup(v => v.ValidateRequired("master", UI.ui_ExportPasswordRequired))
            .Returns(new PasswordValidationResult());

        var vm = new ExportPasswordPromptViewModel("Export", validation.Object)
        {
            MasterPassword = "master",
            ValidateMasterPasswordAsync = _ => Task.FromResult(true)
        };

        var closed = false;
        vm.RequestClose += (_, _) => closed = true;

        vm.ConfirmCommand.Execute(null);
        await WaitUntilAsync(() => closed);

        Assert.Equal("master", vm.SelectedPassword);
        Assert.True(closed);
        Assert.Equal(string.Empty, vm.ErrorMessage);
    }

    [Fact]
    public async Task ConfirmCommand_CustomPasswordValidationFails_UsesValidationMessage()
    {
        var validation = new Mock<IPasswordValidationService>();
        validation.Setup(v => v.ValidateNewWithConfirmation(
                "pw1",
                "pw2",
                UI.ui_Password_Required,
                UI.ui_Password_MinLength_Format,
                UI.ui_ExportPasswordRequired,
                UI.ui_ExportPwd_CustomPasswordMismatch))
            .Returns(new PasswordValidationResult { ConfirmPasswordError = "mismatch" });

        var vm = new ExportPasswordPromptViewModel("Export", validation.Object)
        {
            UseMasterPassword = false,
            CustomPassword = "pw1",
            ConfirmCustomPassword = "pw2"
        };

        vm.ConfirmCommand.Execute(null);
        await WaitUntilAsync(() => vm.ErrorMessage == "mismatch");

        Assert.Equal("mismatch", vm.ErrorMessage);
        Assert.Equal(string.Empty, vm.SelectedPassword);
    }

    [Fact]
    public async Task ConfirmCommand_CustomPasswordValid_ClosesAndSetsSelectedPassword()
    {
        var validation = new Mock<IPasswordValidationService>();
        validation.Setup(v => v.ValidateNewWithConfirmation(
                "pw",
                "pw",
                UI.ui_Password_Required,
                UI.ui_Password_MinLength_Format,
                UI.ui_ExportPasswordRequired,
                UI.ui_ExportPwd_CustomPasswordMismatch))
            .Returns(new PasswordValidationResult());

        var vm = new ExportPasswordPromptViewModel("Export", validation.Object)
        {
            UseMasterPassword = false,
            CustomPassword = "pw",
            ConfirmCustomPassword = "pw"
        };

        var closed = false;
        vm.RequestClose += (_, _) => closed = true;

        vm.ConfirmCommand.Execute(null);
        await WaitUntilAsync(() => closed);

        Assert.Equal("pw", vm.SelectedPassword);
        Assert.True(closed);
    }

    [Fact]
    public void UseMasterPassword_Toggle_ClearsErrorAndUpdatesInverseFlag()
    {
        var vm = new ExportPasswordPromptViewModel("Export", Mock.Of<IPasswordValidationService>())
        {
            ErrorMessage = "error"
        };

        vm.UseMasterPassword = false;

        Assert.Equal(string.Empty, vm.ErrorMessage);
        Assert.True(vm.UseCustomPassword);
    }

    [Fact]
    public void PasswordFieldChanges_ClearExistingErrorMessage()
    {
        var vm = new ExportPasswordPromptViewModel("Export", Mock.Of<IPasswordValidationService>())
        {
            ErrorMessage = "error"
        };

        vm.MasterPassword = "a";
        Assert.Equal(string.Empty, vm.ErrorMessage);

        vm.ErrorMessage = "error2";
        vm.CustomPassword = "b";
        Assert.Equal(string.Empty, vm.ErrorMessage);

        vm.ErrorMessage = "error3";
        vm.ConfirmCustomPassword = "c";
        Assert.Equal(string.Empty, vm.ErrorMessage);
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
