using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TOTP.Core.Security;
using TOTP.Core.Security.Interfaces;
using TOTP.Core.Security.Models;
using TOTP.Tests.Common;
using TOTP.ViewModels;

namespace TOTP.Tests.ViewModels;

public sealed class UnlockViewModelTests : BaseAutoMockTest
{
    [Fact]
    public void Constructor_NotConfiguredState_CurrentGateIsNullAndChooserVisible()
    {
        // Arrange
        var authorizationState = new AuthorizationState();
        var authMock = FreezeMock<IAuthorizationService>();
        authMock.SetupGet(x => x.State).Returns(authorizationState);

        // Act
        var sut = CreateWithFixture<UnlockViewModel>();

        // Assert
        sut.IsConfigured.Should().BeFalse();
        sut.CurrentGate.Should().BeNull();
        sut.HasSelectedSetupGate.Should().BeFalse();
    }

    [Fact]
    public void AuthorizationStateChanges_ToPasswordGate_RaisesPropertyChangedAndSetsCurrentGate()
    {
        // Arrange
        var authorizationState = new AuthorizationState();
        var authMock = FreezeMock<IAuthorizationService>();
        authMock.SetupGet(x => x.State).Returns(authorizationState);
        var sut = CreateWithFixture<UnlockViewModel>();
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
    public void ChoosePasswordCommand_Execute_InvokesSetupModeAndSelectsPasswordGate()
    {
        // Arrange
        var authorizationState = new AuthorizationState();
        var authMock = FreezeMock<IAuthorizationService>();
        authMock.SetupGet(x => x.State).Returns(authorizationState);
        var sut = CreateWithFixture<UnlockViewModel>();

        // Act
        sut.ChoosePasswordCommand.Execute(null);

        // Assert
        sut.CurrentGate.Should().BeSameAs(sut.PasswordUnlockVM);
        sut.PasswordUnlockVM.IsSetup.Should().BeTrue();
        sut.StatusMessage.Should().BeNull();
    }

    [Theory]
    [InlineData(AuthorizationResult.NotAvailable, "Windows Hello is not available on this device/account. Choose Password.")]
    [InlineData(AuthorizationResult.Failed, "Failed to configure Windows Hello.")]
    public async Task ChooseHelloCommand_ConfigurationFailure_ShowsExpectedStatusMessage(
        AuthorizationResult configurationResult,
        string expectedMessage)
    {
        // Arrange
        var authorizationState = new AuthorizationState();
        var authMock = FreezeMock<IAuthorizationService>();
        authMock.SetupGet(x => x.State).Returns(authorizationState);
        authMock.Setup(x => x.ConfigureHelloAsync()).ReturnsAsync(configurationResult);
        var sut = CreateWithFixture<UnlockViewModel>();

        // Act
        sut.ChooseHelloCommand.Execute(null);
        await WaitUntilAsync(() => !string.IsNullOrWhiteSpace(sut.StatusMessage));

        // Assert
        sut.StatusMessage.Should().Be(expectedMessage);
        authMock.Verify(x => x.ConfigureHelloAsync(), Times.Once);
        authMock.Verify(x => x.TryUnlockWithHelloAsync(), Times.Never);
    }

    [Fact]
    public async Task ChooseHelloCommand_ConfigurationSucceedsButUnlockFails_ShowsVerificationFailure()
    {
        // Arrange
        var authorizationState = new AuthorizationState();
        var authMock = FreezeMock<IAuthorizationService>();
        authMock.SetupGet(x => x.State).Returns(authorizationState);
        authMock.Setup(x => x.ConfigureHelloAsync()).ReturnsAsync(AuthorizationResult.Success);
        authMock.Setup(x => x.TryUnlockWithHelloAsync()).ReturnsAsync(AuthorizationResult.InvalidCredentials);
        var sut = CreateWithFixture<UnlockViewModel>();

        // Act
        sut.ChooseHelloCommand.Execute(null);
        await WaitUntilAsync(() => !string.IsNullOrWhiteSpace(sut.StatusMessage));

        // Assert
        sut.StatusMessage.Should().Be("Hello verification failed. Try again or use Password if configured.");
        authMock.Verify(x => x.ConfigureHelloAsync(), Times.Once);
        authMock.Verify(x => x.TryUnlockWithHelloAsync(), Times.Once);
    }

    [Fact]
    public async Task ChooseHelloCommand_ConfigurationAndUnlockSucceed_ClearsStatusMessageAndCallsServices()
    {
        // Arrange
        var authorizationState = new AuthorizationState();
        var authMock = FreezeMock<IAuthorizationService>();
        authMock.SetupGet(x => x.State).Returns(authorizationState);
        authMock.Setup(x => x.ConfigureHelloAsync()).ReturnsAsync(AuthorizationResult.Success);
        authMock.Setup(x => x.TryUnlockWithHelloAsync()).ReturnsAsync(AuthorizationResult.Success);
        var sut = CreateWithFixture<UnlockViewModel>();
        sut.StatusMessage = "old status";

        // Act
        sut.ChooseHelloCommand.Execute(null);
        await WaitUntilAsync(() => authMock.Invocations.Count(i => i.Method.Name == nameof(IAuthorizationService.TryUnlockWithHelloAsync)) == 1);

        // Assert
        sut.StatusMessage.Should().BeNull();
        authMock.Verify(x => x.ConfigureHelloAsync(), Times.Once);
        authMock.Verify(x => x.TryUnlockWithHelloAsync(), Times.Once);
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

        // Act
        var sut = CreateWithAutoMocker<UnlockViewModel>();

        // Assert
        sut.IsConfigured.Should().BeTrue();
        sut.CurrentGate.Should().BeSameAs(sut.HelloUnlockVM);
        sut.ConfiguredGate.Should().Be(AuthorizationGateKind.Hello);
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
}
