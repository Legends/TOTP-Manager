using Moq;
using Microsoft.Extensions.Logging.Abstractions;
using System.Diagnostics;
using System.Windows;
using TOTP.Core.Security.Interfaces;
using TOTP.Core.Security.Models;
using TOTP.Services;
using TOTP.Services.Interfaces;
using TOTP.ViewModels;

namespace TOTP.Tests.Services;

public sealed class PasswordPromptServiceTests
{
    [Fact]
    public void PromptForEncryptedExportPassword_WhenConfirmedAndValid_ReturnsSelectedPassword()
    {
        var auth = new Mock<IAuthorizationService>();
        var validation = new Mock<IPasswordValidationService>();
        var dialogFactory = new Mock<IPasswordPromptDialogFactory>();
        var dialog = new FakeDialog();

        dialogFactory.Setup(f => f.CreateExportPasswordPromptDialog()).Returns(dialog);
        validation.Setup(v => v.ValidateRequired("master", It.IsAny<string>())).Returns(new PasswordValidationResult());
        auth.Setup(a => a.TryUnlockWithPasswordAsync("master")).ReturnsAsync(AuthorizationResult.Success);

        dialog.OnShowDialog = () =>
        {
            var vm = (ExportPasswordPromptViewModel)dialog.DataContext!;
            var closed = false;
            vm.RequestClose += (_, _) => closed = true;
            vm.MasterPassword = "master";
            vm.ConfirmCommand.Execute(null);
            SpinWait.SpinUntil(() => closed, 1000);
            return closed;
        };

        var sut = new PasswordPromptService(auth.Object, validation.Object, dialogFactory.Object, NullLogger<PasswordPromptService>.Instance);

        var result = sut.PromptForEncryptedExportPassword("Export");

        Assert.Equal("master", result);
        auth.Verify(a => a.TryUnlockWithPasswordAsync("master"), Times.Once);
    }

    [Fact]
    public void PromptForEncryptedExportPassword_WhenMasterPasswordInvalid_ReturnsNull()
    {
        var auth = new Mock<IAuthorizationService>();
        var validation = new Mock<IPasswordValidationService>();
        var dialogFactory = new Mock<IPasswordPromptDialogFactory>();
        var dialog = new FakeDialog();

        dialogFactory.Setup(f => f.CreateExportPasswordPromptDialog()).Returns(dialog);
        validation.Setup(v => v.ValidateRequired("master", It.IsAny<string>())).Returns(new PasswordValidationResult());
        auth.Setup(a => a.TryUnlockWithPasswordAsync("master")).ReturnsAsync(AuthorizationResult.InvalidCredentials);

        dialog.OnShowDialog = () =>
        {
            var vm = (ExportPasswordPromptViewModel)dialog.DataContext!;
            var closed = false;
            vm.RequestClose += (_, _) => closed = true;
            vm.MasterPassword = "master";
            vm.ConfirmCommand.Execute(null);
            SpinWait.SpinUntil(() => !string.IsNullOrWhiteSpace(vm.ErrorMessage), 1000);
            return closed;
        };

        var sut = new PasswordPromptService(auth.Object, validation.Object, dialogFactory.Object, NullLogger<PasswordPromptService>.Instance);

        var result = sut.PromptForEncryptedExportPassword("Export");

        Assert.Null(result);
        auth.Verify(a => a.TryUnlockWithPasswordAsync("master"), Times.Once);
    }

    [Fact]
    public void PromptForEncryptedExportPassword_WhenDialogCanceled_ReturnsNull()
    {
        var sut = new PasswordPromptService(
            Mock.Of<IAuthorizationService>(),
            Mock.Of<IPasswordValidationService>(),
            CreateFactoryFor(new FakeDialog { OnShowDialog = () => false }, null),
            NullLogger<PasswordPromptService>.Instance);

        var result = sut.PromptForEncryptedExportPassword("Export");

        Assert.Null(result);
    }

    [Fact]
    public void PromptForEncryptedExportPassword_WhenViewModelIsClearedOnDialogClose_StillReturnsConfirmedPassword()
    {
        var auth = new Mock<IAuthorizationService>();
        var validation = new Mock<IPasswordValidationService>();
        var dialogFactory = new Mock<IPasswordPromptDialogFactory>();
        var dialog = new FakeDialog();

        dialogFactory.Setup(f => f.CreateExportPasswordPromptDialog()).Returns(dialog);
        validation.Setup(v => v.ValidateRequired("new-master", It.IsAny<string>())).Returns(new PasswordValidationResult());
        auth.Setup(a => a.TryUnlockWithPasswordAsync("new-master")).ReturnsAsync(AuthorizationResult.Success);

        dialog.OnShowDialog = () =>
        {
            var vm = (ExportPasswordPromptViewModel)dialog.DataContext!;
            var closed = false;
            vm.RequestClose += (_, _) => closed = true;
            vm.MasterPassword = "new-master";
            vm.ConfirmCommand.Execute(null);
            SpinWait.SpinUntil(() => closed, 1000);

            // Simulate window OnClosed sensitive-data cleanup before ShowDialog() returns.
            vm.ClearSensitiveData();
            return true;
        };

        var sut = new PasswordPromptService(auth.Object, validation.Object, dialogFactory.Object, NullLogger<PasswordPromptService>.Instance);

        var result = sut.PromptForEncryptedExportPassword("Export");

        Assert.Equal("new-master", result);
    }

    [Fact]
    public void Prompt_WhenConfirmedAndValidationPasses_ReturnsPassword()
    {
        var auth = new Mock<IAuthorizationService>();
        var validation = new Mock<IPasswordValidationService>();
        var dialogFactory = new Mock<IPasswordPromptDialogFactory>();
        var dialog = new FakeDialog();

        dialogFactory.Setup(f => f.CreatePasswordPromptDialog()).Returns(dialog);
        validation.Setup(v => v.ValidateRequired("import-pass", It.IsAny<string>()))
            .Returns(new PasswordValidationResult());

        dialog.OnShowDialog = () =>
        {
            var vm = (PasswordPromptViewModel)dialog.DataContext!;
            var closed = false;
            vm.RequestClose += (_, _) => closed = true;
            vm.Password = "import-pass";
            vm.ValidatePasswordAsync = _ => Task.FromResult<string?>(null);
            vm.ConfirmCommand.Execute(null);
            SpinWait.SpinUntil(() => closed, 1000);
            return closed;
        };

        var sut = new PasswordPromptService(auth.Object, validation.Object, dialogFactory.Object, NullLogger<PasswordPromptService>.Instance);

        var result = sut.Prompt("Title", "Message");

        Assert.Equal("import-pass", result);
    }

    [Fact]
    public void Prompt_WhenValidationFails_ReturnsNull()
    {
        var auth = new Mock<IAuthorizationService>();
        var validation = new Mock<IPasswordValidationService>();
        var dialogFactory = new Mock<IPasswordPromptDialogFactory>();
        var dialog = new FakeDialog();

        dialogFactory.Setup(f => f.CreatePasswordPromptDialog()).Returns(dialog);
        validation.Setup(v => v.ValidateRequired("import-pass", It.IsAny<string>()))
            .Returns(new PasswordValidationResult());

        dialog.OnShowDialog = () =>
        {
            var vm = (PasswordPromptViewModel)dialog.DataContext!;
            var closed = false;
            vm.RequestClose += (_, _) => closed = true;
            vm.Password = "import-pass";
            vm.ValidatePasswordAsync = _ => Task.FromResult<string?>("bad");
            vm.ConfirmCommand.Execute(null);
            SpinWait.SpinUntil(() => !string.IsNullOrWhiteSpace(vm.ErrorMessage), 1000);
            return closed;
        };

        var sut = new PasswordPromptService(auth.Object, validation.Object, dialogFactory.Object, NullLogger<PasswordPromptService>.Instance);

        var result = sut.Prompt("Title", "Message");

        Assert.Null(result);
    }

    [Fact]
    public void Prompt_WhenDialogCanceled_ReturnsNull()
    {
        var sut = new PasswordPromptService(
            Mock.Of<IAuthorizationService>(),
            Mock.Of<IPasswordValidationService>(),
            CreateFactoryFor(null, new FakeDialog { OnShowDialog = () => false }),
            NullLogger<PasswordPromptService>.Instance);

        var result = sut.Prompt("Title", "Message");

        Assert.Null(result);
    }

    [Fact]
    public void Prompt_WhenViewModelIsClearedOnDialogClose_StillReturnsConfirmedPassword()
    {
        var auth = new Mock<IAuthorizationService>();
        var validation = new Mock<IPasswordValidationService>();
        var dialogFactory = new Mock<IPasswordPromptDialogFactory>();
        var dialog = new FakeDialog();

        dialogFactory.Setup(f => f.CreatePasswordPromptDialog()).Returns(dialog);
        validation.Setup(v => v.ValidateRequired("import-pass", It.IsAny<string>()))
            .Returns(new PasswordValidationResult());

        dialog.OnShowDialog = () =>
        {
            var vm = (PasswordPromptViewModel)dialog.DataContext!;
            var closed = false;
            vm.RequestClose += (_, _) => closed = true;
            vm.Password = "import-pass";
            vm.ValidatePasswordAsync = _ => Task.FromResult<string?>(null);
            vm.ConfirmCommand.Execute(null);
            SpinWait.SpinUntil(() => closed, 1000);

            // Simulate window OnClosed sensitive-data cleanup before ShowDialog() returns.
            vm.ClearSensitiveData();
            return true;
        };

        var sut = new PasswordPromptService(auth.Object, validation.Object, dialogFactory.Object, NullLogger<PasswordPromptService>.Instance);

        var result = sut.Prompt("Title", "Message");

        Assert.Equal("import-pass", result);
    }

    private static IPasswordPromptDialogFactory CreateFactoryFor(FakeDialog? exportDialog, FakeDialog? promptDialog)
    {
        var factory = new Mock<IPasswordPromptDialogFactory>();
        factory.Setup(f => f.CreateExportPasswordPromptDialog()).Returns(exportDialog ?? new FakeDialog());
        factory.Setup(f => f.CreatePasswordPromptDialog()).Returns(promptDialog ?? new FakeDialog());
        return factory.Object;
    }

    private sealed class FakeDialog : IPasswordPromptDialog
    {
        public object? DataContext { get; set; }
        public Window? Owner { get; set; }
        public Func<bool?>? OnShowDialog { get; set; }

        public bool? ShowDialog() => OnShowDialog?.Invoke();
    }
}
