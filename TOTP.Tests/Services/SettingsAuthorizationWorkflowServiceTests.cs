using Moq;
using TOTP.Core.Security;
using TOTP.Core.Security.Interfaces;
using TOTP.Core.Security.Models;
using TOTP.Core.Services.Interfaces;
using TOTP.Resources;
using TOTP.Services;

namespace TOTP.Tests.Services;

public sealed class SettingsAuthorizationWorkflowServiceTests
{
    [Fact]
    public async Task ApplyAuthorizationSettingsAsync_WhenHelloIsUnavailable_ReturnsErrorWithoutPrompting()
    {
        var dependencies = new Dependencies();
        var sut = dependencies.CreateSut();

        var result = await sut.ApplyAuthorizationSettingsAsync(
            isHelloSelected: true,
            isHelloAvailable: false,
            newPassword: string.Empty,
            confirmPassword: string.Empty);

        Assert.False(result.IsSuccess);
        Assert.Equal(UI.ui_Settings_Auth_HelloUnsupported, result.ErrorMessage);
        dependencies.Prompt.VerifyNoOtherCalls();
        dependencies.Authorization.Verify(value => value.ConfigureHelloAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ApplyAuthorizationGateSelectionAsync_WhenGateIsUnchanged_DoesNothing()
    {
        var dependencies = new Dependencies();
        dependencies.State.SetConfiguration(true, TOTP.Core.Enums.PreferredUnlockMethod.Password);
        var sut = dependencies.CreateSut();

        var result = await sut.ApplyAuthorizationGateSelectionAsync(
            isHelloSelected: false,
            isHelloAvailable: true);

        Assert.True(result.IsSuccess);
        dependencies.Authorization.Verify(value => value.SetGateAsync(
            It.IsAny<AuthorizationGateKind>()), Times.Never);
        dependencies.Prompt.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ApplyAuthorizationGateSelectionAsync_WhenQuickUnlockExists_SelectsItWithoutPasswordPrompt()
    {
        var dependencies = new Dependencies();
        dependencies.State.SetConfiguration(true, TOTP.Core.Enums.PreferredUnlockMethod.Password);
        dependencies.Authorization.Setup(value => value.SetGateAsync(AuthorizationGateKind.Hello))
            .ReturnsAsync(AuthorizationResult.Success);
        var sut = dependencies.CreateSut();

        var result = await sut.ApplyAuthorizationGateSelectionAsync(
            isHelloSelected: true,
            isHelloAvailable: true);

        Assert.True(result.IsSuccess);
        dependencies.Prompt.VerifyNoOtherCalls();
        dependencies.Authorization.Verify(value => value.ConfigureHelloAsync(
            It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ApplyAuthorizationGateSelectionAsync_WhenEnrollmentIsNeeded_PromptsAndPassesRecoveryPassword()
    {
        var dependencies = new Dependencies();
        dependencies.State.SetConfiguration(true, TOTP.Core.Enums.PreferredUnlockMethod.Password);
        dependencies.Authorization.Setup(value => value.SetGateAsync(AuthorizationGateKind.Hello))
            .ReturnsAsync(AuthorizationResult.PasswordRequired);
        dependencies.SetupPrompt("recovery-password");
        dependencies.Authorization.Setup(value => value.ConfigureHelloAsync("recovery-password"))
            .ReturnsAsync(AuthorizationResult.Success);
        var sut = dependencies.CreateSut();

        var result = await sut.ApplyAuthorizationGateSelectionAsync(
            isHelloSelected: true,
            isHelloAvailable: true);

        Assert.True(result.IsSuccess);
        dependencies.Authorization.Verify(value => value.ConfigureHelloAsync("recovery-password"), Times.Once);
    }

    [Fact]
    public async Task ApplyAuthorizationGateSelectionAsync_WhenEnrollmentPromptIsCancelled_FailsClosed()
    {
        var dependencies = new Dependencies();
        dependencies.State.SetConfiguration(true, TOTP.Core.Enums.PreferredUnlockMethod.Password);
        dependencies.Authorization.Setup(value => value.SetGateAsync(AuthorizationGateKind.Hello))
            .ReturnsAsync(AuthorizationResult.PasswordRequired);
        dependencies.SetupPrompt(null);
        var sut = dependencies.CreateSut();

        var result = await sut.ApplyAuthorizationGateSelectionAsync(
            isHelloSelected: true,
            isHelloAvailable: true);

        Assert.False(result.IsSuccess);
        Assert.Equal(UI.ui_Password_VerificationFailed, result.ErrorMessage);
        dependencies.Authorization.Verify(value => value.ConfigureHelloAsync(
            It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ApplyAuthorizationGateSelectionAsync_SwitchingToPassword_VerifiesCurrentPassword()
    {
        var dependencies = new Dependencies();
        dependencies.State.SetConfiguration(true, TOTP.Core.Enums.PreferredUnlockMethod.PlatformQuickUnlock);
        dependencies.Authorization.Setup(value => value.TryUnlockWithPasswordAsync("recovery-password"))
            .ReturnsAsync(AuthorizationResult.Success);
        dependencies.Authorization.Setup(value => value.SetGateAsync(AuthorizationGateKind.Password))
            .ReturnsAsync(AuthorizationResult.Success);
        dependencies.Prompt.Setup(value => value.Prompt(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<Func<string, Task<string?>>?>()))
            .Returns<string, string, string?, string?, Func<string, Task<string?>>?>(
                (_, _, _, _, validate) =>
                    validate!("recovery-password").GetAwaiter().GetResult() is null
                        ? "recovery-password"
                        : null);
        var sut = dependencies.CreateSut();

        var result = await sut.ApplyAuthorizationGateSelectionAsync(
            isHelloSelected: false,
            isHelloAvailable: true);

        Assert.True(result.IsSuccess);
        dependencies.Authorization.Verify(value => value.TryUnlockWithPasswordAsync("recovery-password"), Times.Once);
        dependencies.Authorization.Verify(value => value.SetGateAsync(AuthorizationGateKind.Password), Times.Once);
    }

    [Fact]
    public async Task ChangePasswordAsync_WhenValidationFails_ReturnsFieldErrorsWithoutPrompting()
    {
        var dependencies = new Dependencies(validNewPassword: false);
        var sut = dependencies.CreateSut();

        var result = await sut.ChangePasswordAsync("new", "different");

        Assert.False(result.IsSuccess);
        Assert.Equal("password error", result.NewPasswordError);
        Assert.Equal("confirmation error", result.ConfirmPasswordError);
        dependencies.Prompt.VerifyNoOtherCalls();
        dependencies.Authorization.Verify(value => value.ChangePasswordAsync(
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ChangePasswordAsync_PromptsAndPassesCurrentRecoveryPassword()
    {
        var dependencies = new Dependencies();
        dependencies.SetupPrompt("current-password");
        dependencies.Authorization.Setup(value => value.ChangePasswordAsync(
                "current-password",
                "new-password"))
            .ReturnsAsync(AuthorizationResult.Success);
        var sut = dependencies.CreateSut();

        var result = await sut.ChangePasswordAsync("new-password", "new-password");

        Assert.True(result.IsSuccess);
        Assert.True(result.ClearPasswordInputs);
        dependencies.Authorization.Verify(value => value.ChangePasswordAsync(
            "current-password",
            "new-password"), Times.Once);
    }

    [Fact]
    public async Task ChangePasswordAsync_WhenCurrentPasswordPromptIsCancelled_DoesNotChangePassword()
    {
        var dependencies = new Dependencies();
        dependencies.SetupPrompt(null);
        var sut = dependencies.CreateSut();

        var result = await sut.ChangePasswordAsync("new-password", "new-password");

        Assert.False(result.IsSuccess);
        Assert.Equal(UI.ui_Password_VerificationFailed, result.ErrorMessage);
        dependencies.Authorization.Verify(value => value.ChangePasswordAsync(
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ChangePasswordAsync_WhenPasswordGateRequested_ActivatesItAfterReplacement()
    {
        var dependencies = new Dependencies();
        dependencies.SetupPrompt("current-password");
        dependencies.Authorization.Setup(value => value.ChangePasswordAsync(
                "current-password",
                "new-password"))
            .ReturnsAsync(AuthorizationResult.Success);
        dependencies.Authorization.Setup(value => value.SetGateAsync(AuthorizationGateKind.Password))
            .ReturnsAsync(AuthorizationResult.Success);
        var sut = dependencies.CreateSut();

        var result = await sut.ChangePasswordAsync(
            "new-password",
            "new-password",
            activatePasswordGate: true);

        Assert.True(result.IsSuccess);
        dependencies.Authorization.Verify(value => value.SetGateAsync(AuthorizationGateKind.Password), Times.Once);
    }

    private sealed class Dependencies
    {
        public AuthorizationState State { get; } = new();
        public Mock<IAuthorizationService> Authorization { get; } = new();
        public Mock<IPasswordValidationService> Validation { get; } = new();
        public Mock<IPasswordPromptService> Prompt { get; } = new();

        public Dependencies(bool validNewPassword = true)
        {
            Authorization.SetupGet(value => value.State).Returns(State);
            Validation.Setup(value => value.ValidateNewWithConfirmation(
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns(validNewPassword
                    ? new PasswordValidationResult()
                    : new PasswordValidationResult
                    {
                        PasswordError = "password error",
                        ConfirmPasswordError = "confirmation error"
                    });
        }

        public SettingsAuthorizationWorkflowService CreateSut() =>
            new(Authorization.Object, Validation.Object, Prompt.Object);

        public void SetupPrompt(string? result)
        {
            Prompt.Setup(value => value.Prompt(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<Func<string, Task<string?>>?>()))
                .Returns(result);
        }
    }
}
