using FluentResults;
using Moq;
using TOTP.Core.Models;
using TOTP.Core.Security.Interfaces;
using TOTP.Core.Security.Models;
using TOTP.Services;

namespace TOTP.Tests.Services;

public sealed class SettingsAuthorizationWorkflowServiceTests
{
    [Fact]
    public async Task ApplyAuthorizationSettingsAsync_WhenHelloNotAvailable_ReturnsError()
    {
        var appSettings = new AppSettings
        {
            Authorization = new AuthorizationProfile
            {
                Gate = AuthorizationGateKind.Password,
                HelloWrappedDek = null,
                HelloKeyId = null
            }
        };

        var auth = new Mock<IAuthorizationService>();
        var settings = new Mock<ISettingsService>();
        var pwd = new Mock<IPasswordValidationService>();
        settings.SetupGet(s => s.Current).Returns(appSettings);

        var sut = new SettingsAuthorizationWorkflowService(auth.Object, settings.Object, pwd.Object);

        var result = await sut.ApplyAuthorizationSettingsAsync(
            isHelloSelected: true,
            isHelloAvailable: false,
            newPassword: string.Empty,
            confirmPassword: string.Empty);

        Assert.False(result.IsSuccess);
        Assert.Equal("Windows Hello is not supported on this device.", result.ErrorMessage);
        auth.Verify(a => a.ConfigureHelloAsync(), Times.Never);
    }

    [Fact]
    public async Task ApplyAuthorizationGateSelectionAsync_WhenSaveFails_RollsBackGate()
    {
        var appSettings = new AppSettings
        {
            Authorization = new AuthorizationProfile
            {
                Gate = AuthorizationGateKind.Password,
                HelloWrappedDek = [1, 2, 3],
                HelloKeyId = "key-id"
            }
        };

        var auth = new Mock<IAuthorizationService>();
        var settings = new Mock<ISettingsService>();
        var pwd = new Mock<IPasswordValidationService>();
        settings.SetupGet(s => s.Current).Returns(appSettings);
        settings.Setup(s => s.SaveAsync()).ReturnsAsync(Result.Fail("save failed"));

        var sut = new SettingsAuthorizationWorkflowService(auth.Object, settings.Object, pwd.Object);

        var result = await sut.ApplyAuthorizationGateSelectionAsync(
            isHelloSelected: true,
            isHelloAvailable: true);

        Assert.False(result.IsSuccess);
        Assert.Equal(AuthorizationGateKind.Password, appSettings.Authorization.Gate);
    }

    [Fact]
    public async Task ChangePasswordAsync_WhenValidationFails_ReturnsFieldErrors()
    {
        var appSettings = new AppSettings();
        var auth = new Mock<IAuthorizationService>();
        var settings = new Mock<ISettingsService>();
        var pwd = new Mock<IPasswordValidationService>();
        settings.SetupGet(s => s.Current).Returns(appSettings);
        pwd.Setup(p => p.ValidateNewWithConfirmation(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Returns(new PasswordValidationResult
            {
                PasswordError = "pwd error",
                ConfirmPasswordError = "confirm error"
            });

        var sut = new SettingsAuthorizationWorkflowService(auth.Object, settings.Object, pwd.Object);

        var result = await sut.ChangePasswordAsync("abc", "def");

        Assert.False(result.IsSuccess);
        Assert.Equal("pwd error", result.NewPasswordError);
        Assert.Equal("confirm error", result.ConfirmPasswordError);
        auth.Verify(a => a.ChangePasswordAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ApplyAuthorizationSettingsAsync_WithValidPassword_ChangesPassword()
    {
        var appSettings = new AppSettings
        {
            Authorization = new AuthorizationProfile
            {
                Gate = AuthorizationGateKind.Password,
                HelloWrappedDek = [1, 2, 3],
                HelloKeyId = "key-id"
            }
        };

        var auth = new Mock<IAuthorizationService>();
        var settings = new Mock<ISettingsService>();
        var pwd = new Mock<IPasswordValidationService>();
        settings.SetupGet(s => s.Current).Returns(appSettings);
        pwd.Setup(p => p.ValidateNewWithConfirmation(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Returns(new PasswordValidationResult());
        auth.Setup(a => a.ChangePasswordAsync(string.Empty, "new-pass"))
            .ReturnsAsync(AuthorizationResult.Success);

        var sut = new SettingsAuthorizationWorkflowService(auth.Object, settings.Object, pwd.Object);

        var result = await sut.ApplyAuthorizationSettingsAsync(
            isHelloSelected: false,
            isHelloAvailable: true,
            newPassword: "new-pass",
            confirmPassword: "new-pass");

        Assert.True(result.IsSuccess);
        Assert.True(result.ClearPasswordInputs);
        auth.Verify(a => a.ChangePasswordAsync(string.Empty, "new-pass"), Times.Once);
    }

    [Fact]
    public async Task ApplyAuthorizationGateSelectionAsync_WhenGateUnchanged_DoesNotSave()
    {
        var appSettings = new AppSettings
        {
            Authorization = new AuthorizationProfile
            {
                Gate = AuthorizationGateKind.Password
            }
        };

        var auth = new Mock<IAuthorizationService>();
        var settings = new Mock<ISettingsService>();
        var pwd = new Mock<IPasswordValidationService>();
        settings.SetupGet(s => s.Current).Returns(appSettings);

        var sut = new SettingsAuthorizationWorkflowService(auth.Object, settings.Object, pwd.Object);

        var result = await sut.ApplyAuthorizationGateSelectionAsync(
            isHelloSelected: false,
            isHelloAvailable: true);

        Assert.True(result.IsSuccess);
        settings.Verify(s => s.SaveAsync(), Times.Never);
    }

    [Fact]
    public async Task ApplyAuthorizationGateSelectionAsync_WhenHelloSetupFails_ReturnsError()
    {
        var appSettings = new AppSettings
        {
            Authorization = new AuthorizationProfile
            {
                Gate = AuthorizationGateKind.Password,
                HelloWrappedDek = null,
                HelloKeyId = null
            }
        };

        var auth = new Mock<IAuthorizationService>();
        var settings = new Mock<ISettingsService>();
        var pwd = new Mock<IPasswordValidationService>();
        settings.SetupGet(s => s.Current).Returns(appSettings);
        auth.Setup(a => a.ConfigureHelloAsync()).ReturnsAsync(AuthorizationResult.Failed);

        var sut = new SettingsAuthorizationWorkflowService(auth.Object, settings.Object, pwd.Object);

        var result = await sut.ApplyAuthorizationGateSelectionAsync(
            isHelloSelected: true,
            isHelloAvailable: true);

        Assert.False(result.IsSuccess);
        Assert.Equal("Windows Hello setup failed.", result.ErrorMessage);
        settings.Verify(s => s.SaveAsync(), Times.Never);
    }

    [Fact]
    public async Task ChangePasswordAsync_WhenAuthorizationFails_ReturnsValidationFailed()
    {
        var appSettings = new AppSettings();
        var auth = new Mock<IAuthorizationService>();
        var settings = new Mock<ISettingsService>();
        var pwd = new Mock<IPasswordValidationService>();
        settings.SetupGet(s => s.Current).Returns(appSettings);
        pwd.Setup(p => p.ValidateNewWithConfirmation(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Returns(new PasswordValidationResult());
        auth.Setup(a => a.ChangePasswordAsync(string.Empty, "new-pass"))
            .ReturnsAsync(AuthorizationResult.Failed);

        var sut = new SettingsAuthorizationWorkflowService(auth.Object, settings.Object, pwd.Object);

        var result = await sut.ChangePasswordAsync("new-pass", "new-pass");

        Assert.False(result.IsSuccess);
        Assert.Equal(TOTP.Resources.UI.ui_Password_ValidationFailed, result.ErrorMessage);
    }
}
