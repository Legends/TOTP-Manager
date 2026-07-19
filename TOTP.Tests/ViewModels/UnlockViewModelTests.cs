using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TOTP.Core.Security;
using TOTP.Core.Security.Interfaces;
using TOTP.Core.Security.Models;
using TOTP.Core.Services.Interfaces;
using TOTP.Resources;
using TOTP.Services.Interfaces;
using TOTP.Tests.Common;
using TOTP.ViewModels;

namespace TOTP.Tests.ViewModels;

public sealed class UnlockViewModelTests : BaseAutoMockTest
{
    [Fact]
    public void Constructor_NotConfiguredState_EntersPasswordSetup()
    {
        // Arrange
        var authorizationState = new AuthorizationState();
        var authMock = FreezeMock<IAuthorizationService>();
        authMock.SetupGet(x => x.State).Returns(authorizationState);

        // Act
        var sut = CreateUnlockViewModel();

        // Assert
        sut.IsConfigured.Should().BeFalse();
        sut.CurrentGate.Should().BeSameAs(sut.PasswordUnlockVM);
        sut.PasswordUnlockVM.IsSetup.Should().BeTrue();
        sut.HasSelectedSetupGate.Should().BeTrue();
    }

    [Fact]
    public void AuthorizationStateChanges_ToPasswordGate_RaisesPropertyChangedAndSetsCurrentGate()
    {
        // Arrange
        var authorizationState = new AuthorizationState();
        var authMock = FreezeMock<IAuthorizationService>();
        authMock.SetupGet(x => x.State).Returns(authorizationState);
        var sut = CreateUnlockViewModel();
        using var monitoredSubject = sut.MonitorEvents();

        // Act
        authorizationState.SetProfile(new AuthorizationProfile { Gate = AuthorizationGateKind.Password });

        // Assert
        sut.CurrentGate.Should().BeSameAs(sut.PasswordUnlockVM);
        sut.HasSelectedSetupGate.Should().BeTrue();
        monitoredSubject.Should().RaisePropertyChangeFor(x => x.ConfiguredGate);
        monitoredSubject.Should().RaisePropertyChangeFor(x => x.IsConfigured);
        monitoredSubject.Should().RaisePropertyChangeFor(x => x.CurrentGate);
        monitoredSubject.Should().RaisePropertyChangeFor(x => x.HasSelectedSetupGate);
    }

    [Fact]
    public void FirstRunGateSelectionCommands_AreDisabledBecausePasswordSetupIsMandatory()
    {
        // Arrange
        var authorizationState = new AuthorizationState();
        var authMock = FreezeMock<IAuthorizationService>();
        authMock.SetupGet(x => x.State).Returns(authorizationState);
        var sut = CreateUnlockViewModel();

        // Assert
        sut.ChoosePasswordCommand.CanExecute(null).Should().BeFalse();
        sut.ChooseHelloCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public async Task PasswordConfigured_WhenHelloAvailableAndAccepted_ConfiguresHello()
    {
        // Arrange
        var authorizationState = new AuthorizationState();
        var authMock = FreezeMock<IAuthorizationService>();
        authMock.SetupGet(x => x.State).Returns(authorizationState);
        authMock.Setup(x => x.ConfigurePasswordAsync("StrongPwd1!", "StrongPwd1!"))
            .ReturnsAsync(() =>
            {
                authorizationState.Unlock();
                return AuthorizationResult.Success;
            });
        authMock.Setup(x => x.IsHelloAvailableAsync()).ReturnsAsync(true);
        authMock.Setup(x => x.ConfigureHelloAsync("StrongPwd1!")).ReturnsAsync(AuthorizationResult.Success);
        FreezeMock<IPasswordPromptService>()
            .Setup(x => x.Prompt(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<Func<string, Task<string?>>?>()))
            .Returns("StrongPwd1!");
        SetupValidPasswordValidation(FreezeMock<IPasswordValidationService>(), "StrongPwd1!", "StrongPwd1!");
        var messageMock = FreezeMock<IMessageService>();
        messageMock
            .Setup(x => x.ConfirmInfo(
                UI.ui_EnableHelloAfterPasswordSetup_Message,
                UI.ui_EnableHelloAfterPasswordSetup_Enable,
                UI.ui_EnableHelloAfterPasswordSetup_NotNow))
            .Returns(true);
        var sut = CreateUnlockViewModel();
        SetupValidPassword(sut.PasswordUnlockVM, "StrongPwd1!", "StrongPwd1!");

        // Act
        sut.PasswordUnlockVM.SavePasswordCommand.Execute(null);
        await WaitUntilAsync(() => authMock.Invocations.Any(i => i.Method.Name == nameof(IAuthorizationService.ConfigureHelloAsync)));

        // Assert
        authMock.Verify(x => x.IsHelloAvailableAsync(), Times.Once);
        authMock.Verify(x => x.ConfigureHelloAsync("StrongPwd1!"), Times.Once);
        messageMock.Verify(x => x.ShowSuccess(UI.ui_EnableHelloAfterPasswordSetup_Success, null), Times.Once);
    }

    [Fact]
    public async Task PasswordConfigured_WhenHelloIsDeclined_DoesNotConfigureHello()
    {
        // Arrange
        var authorizationState = new AuthorizationState();
        var authMock = FreezeMock<IAuthorizationService>();
        authMock.SetupGet(x => x.State).Returns(authorizationState);
        authMock.Setup(x => x.ConfigurePasswordAsync("StrongPwd1!", "StrongPwd1!"))
            .ReturnsAsync(() =>
            {
                authorizationState.Unlock();
                return AuthorizationResult.Success;
            });
        authMock.Setup(x => x.IsHelloAvailableAsync()).ReturnsAsync(true);
        SetupValidPasswordValidation(FreezeMock<IPasswordValidationService>(), "StrongPwd1!", "StrongPwd1!");
        var messageMock = FreezeMock<IMessageService>();
        messageMock
            .Setup(x => x.ConfirmInfo(
                UI.ui_EnableHelloAfterPasswordSetup_Message,
                UI.ui_EnableHelloAfterPasswordSetup_Enable,
                UI.ui_EnableHelloAfterPasswordSetup_NotNow))
            .Returns(false);
        var sut = CreateUnlockViewModel();
        SetupValidPassword(sut.PasswordUnlockVM, "StrongPwd1!", "StrongPwd1!");

        // Act
        sut.PasswordUnlockVM.SavePasswordCommand.Execute(null);
        await WaitUntilAsync(() => authMock.Invocations.Any(i => i.Method.Name == nameof(IAuthorizationService.IsHelloAvailableAsync)));

        // Assert
        authMock.Verify(x => x.ConfigureHelloAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task PasswordConfigured_WhenHelloUnavailable_ShowsWarning()
    {
        // Arrange
        var authorizationState = new AuthorizationState();
        var authMock = FreezeMock<IAuthorizationService>();
        authMock.SetupGet(x => x.State).Returns(authorizationState);
        authMock.Setup(x => x.ConfigurePasswordAsync("StrongPwd1!", "StrongPwd1!"))
            .ReturnsAsync(() =>
            {
                authorizationState.Unlock();
                return AuthorizationResult.Success;
            });
        authMock.Setup(x => x.IsHelloAvailableAsync()).ReturnsAsync(false);
        SetupValidPasswordValidation(FreezeMock<IPasswordValidationService>(), "StrongPwd1!", "StrongPwd1!");
        var messageMock = FreezeMock<IMessageService>();
        var sut = CreateUnlockViewModel();
        SetupValidPassword(sut.PasswordUnlockVM, "StrongPwd1!", "StrongPwd1!");

        // Act
        sut.PasswordUnlockVM.SavePasswordCommand.Execute(null);
        await WaitUntilAsync(() => authMock.Invocations.Any(i => i.Method.Name == nameof(IAuthorizationService.IsHelloAvailableAsync)));

        // Assert
        authMock.Verify(x => x.ConfigureHelloAsync(It.IsAny<string>()), Times.Never);
        messageMock.Verify(x => x.ShowWarning(UI.ui_EnableHelloAfterPasswordSetup_NotAvailable), Times.Once);
    }

    [Fact]
    public void AutoMockerContainer_PreconfiguredAuthorizationState_CreatesConfiguredSut()
    {
        // Arrange
        var authorizationState = new AuthorizationState();
        authorizationState.SetProfile(new AuthorizationProfile { Gate = AuthorizationGateKind.Hello });
        var authMock = new Mock<IAuthorizationService>(MockBehavior.Strict);
        authMock.SetupGet(x => x.State).Returns(authorizationState);

        var helloVm = new HelloUnlockViewModel(authMock.Object);
        var passwordValidator = new Mock<IPasswordValidationService>();
        passwordValidator
            .Setup(x => x.ValidateRequired(It.IsAny<string?>(), It.IsAny<string>()))
            .Returns(new PasswordValidationResult());
        passwordValidator
            .Setup(x => x.ValidateNewWithConfirmation(
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Returns(new PasswordValidationResult());
        var passwordVm = new PasswordUnlockViewModel(
            authMock.Object,
            passwordValidator.Object,
            Mock.Of<ILogger<PasswordUnlockViewModel>>());

        AutoMocker.Use(authMock.Object);
        AutoMocker.Use(helloVm);
        AutoMocker.Use(passwordVm);
        AutoMocker.Use(Mock.Of<ISettingsService>());
        AutoMocker.Use(Mock.Of<IPasswordPromptService>());
        AutoMocker.Use(Mock.Of<IMessageService>());

        // Act
        var sut = CreateWithAutoMocker<UnlockViewModel>();

        // Assert
        sut.IsConfigured.Should().BeTrue();
        sut.CurrentGate.Should().BeSameAs(sut.HelloUnlockVM);
        sut.ConfiguredGate.Should().Be(AuthorizationGateKind.Hello);
    }

    private UnlockViewModel CreateUnlockViewModel()
    {
        var auth = FreezeMock<IAuthorizationService>();
        var validator = FreezeMock<IPasswordValidationService>();
        validator
            .Setup(value => value.ValidateRequired(It.IsAny<string?>(), It.IsAny<string>()))
            .Returns(new PasswordValidationResult());
        validator
            .Setup(value => value.ValidateNewWithConfirmation(
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Returns(new PasswordValidationResult());

        return new UnlockViewModel(
            auth.Object,
            new HelloUnlockViewModel(auth.Object),
            new PasswordUnlockViewModel(
                auth.Object,
                validator.Object,
                Mock.Of<ILogger<PasswordUnlockViewModel>>()),
            FreezeMock<IPasswordPromptService>().Object,
            FreezeMock<IMessageService>().Object);
    }

    private static async Task WaitUntilAsync(Func<bool> condition, int timeoutMs = 1500)
    {
        var start = Environment.TickCount64;
        while (!condition())
        {
            if (Environment.TickCount64 - start > timeoutMs)
            {
                throw new TimeoutException("Condition was not met in time.");
            }

            await Task.Delay(20, TestContext.Current.CancellationToken);
        }
    }

    private static void SetupValidPassword(PasswordUnlockViewModel passwordVm, string password, string confirmPassword)
    {
        passwordVm.Password = password;
        passwordVm.ConfirmPassword = confirmPassword;
    }

    private static void SetupValidPasswordValidation(
        Mock<IPasswordValidationService> validator,
        string password,
        string confirmPassword)
    {
        validator.Setup(x => x.ValidateNewWithConfirmation(
                password,
                confirmPassword,
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Returns(new PasswordValidationResult());
    }
}
