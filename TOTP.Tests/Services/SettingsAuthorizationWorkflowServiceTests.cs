using FluentResults;
using Moq;
using TOTP.Core.Models;
using TOTP.Core.Security.Interfaces;
using TOTP.Core.Security.Models;
using TOTP.Core.Services.Interfaces;
using TOTP.Resources;
using TOTP.Services;
using TOTP.Services.Interfaces;

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
        Assert.Equal(UI.ui_Settings_Auth_HelloUnsupported, result.ErrorMessage);
        auth.Verify(a => a.ConfigureHelloAsync(), Times.Never);
    }

    [Fact]
    public async Task ApplyAuthorizationGateSelectionAsync_WhenSetGateFails_KeepsGate()
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
        auth.Setup(a => a.SetGateAsync(AuthorizationGateKind.Hello))
            .ReturnsAsync(AuthorizationResult.Failed);

        var sut = new SettingsAuthorizationWorkflowService(auth.Object, settings.Object, pwd.Object);

        var result = await sut.ApplyAuthorizationGateSelectionAsync(
            isHelloSelected: true,
            isHelloAvailable: true);

        Assert.False(result.IsSuccess);
        Assert.Equal(AuthorizationGateKind.Password, appSettings.Authorization.Gate);
        settings.Verify(s => s.SaveAsync(), Times.Never);
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
        Assert.Equal(UI.ui_Settings_Auth_HelloSetupFailed, result.ErrorMessage);
        settings.Verify(s => s.SaveAsync(), Times.Never);
    }

    [Fact]
    public async Task ApplyAuthorizationGateSelectionAsync_SwitchingFromHelloToPassword_WithExistingPassword_VerifiesAndSavesGate()
    {
        var appSettings = new AppSettings
        {
            Authorization = new AuthorizationProfile
            {
                Gate = AuthorizationGateKind.Hello,
                PasswordWrappedDek = [1, 2, 3],
                PasswordSalt = [4, 5, 6]
            }
        };

        var auth = new Mock<IAuthorizationService>();
        var settings = new Mock<ISettingsService>();
        var pwd = new Mock<IPasswordValidationService>();
        var prompt = new Mock<IPasswordPromptService>();
        settings.SetupGet(s => s.Current).Returns(appSettings);
        auth.Setup(a => a.SetGateAsync(AuthorizationGateKind.Password))
            .ReturnsAsync(() =>
            {
                appSettings.Authorization.Gate = AuthorizationGateKind.Password;
                return AuthorizationResult.Success;
            });
        prompt.Setup(p => p.Prompt(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<Func<string, Task<string?>>>()))
            .Returns("old-master-password");

        var sut = new SettingsAuthorizationWorkflowService(auth.Object, settings.Object, pwd.Object, prompt.Object);

        var result = await sut.ApplyAuthorizationGateSelectionAsync(
            isHelloSelected: false,
            isHelloAvailable: true);

        Assert.True(result.IsSuccess);
        Assert.Equal(AuthorizationGateKind.Password, appSettings.Authorization.Gate);
        auth.Verify(a => a.SetGateAsync(AuthorizationGateKind.Password), Times.Once);
        settings.Verify(s => s.SaveAsync(), Times.Never);
    }

    [Fact]
    public async Task ApplyAuthorizationGateSelectionAsync_UsesLatestSettingsServiceCurrent()
    {
        var initialSettings = new AppSettings();
        var loadedSettings = new AppSettings
        {
            Authorization = new AuthorizationProfile
            {
                Gate = AuthorizationGateKind.Hello,
                PasswordWrappedDek = [1, 2, 3],
                PasswordSalt = [4, 5, 6],
                DekNonce = [7, 8, 9]
            }
        };

        var currentSettings = initialSettings;
        var auth = new Mock<IAuthorizationService>();
        var settings = new Mock<ISettingsService>();
        var pwd = new Mock<IPasswordValidationService>();
        var prompt = new Mock<IPasswordPromptService>();
        settings.SetupGet(s => s.Current).Returns(() => currentSettings);
        prompt.Setup(p => p.Prompt(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<Func<string, Task<string?>>>()))
            .Returns("old-master-password");
        auth.Setup(a => a.SetGateAsync(AuthorizationGateKind.Password))
            .ReturnsAsync(() =>
            {
                loadedSettings.Authorization.Gate = AuthorizationGateKind.Password;
                return AuthorizationResult.Success;
            });

        var sut = new SettingsAuthorizationWorkflowService(auth.Object, settings.Object, pwd.Object, prompt.Object);
        currentSettings = loadedSettings;

        var result = await sut.ApplyAuthorizationGateSelectionAsync(
            isHelloSelected: false,
            isHelloAvailable: true);

        Assert.True(result.IsSuccess);
        Assert.Equal(AuthorizationGateKind.Password, loadedSettings.Authorization.Gate);
        prompt.Verify(p => p.Prompt(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            It.IsAny<Func<string, Task<string?>>>()), Times.Once);
        auth.Verify(a => a.SetGateAsync(AuthorizationGateKind.Password), Times.Once);
    }

    [Fact]
    public async Task ApplyAuthorizationGateSelectionAsync_SwitchingFromHelloToPassword_WithoutPasswordSetup_KeepsSelectionForSetupAndDoesNotSave()
    {
        var appSettings = new AppSettings
        {
            Authorization = new AuthorizationProfile
            {
                Gate = AuthorizationGateKind.Hello,
                PasswordWrappedDek = null,
                PasswordSalt = null
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

        Assert.False(result.IsSuccess);
        Assert.False(result.RevertGateSelection);
        Assert.Equal(UI.ui_Settings_Auth_PasswordSetupRequired, result.ErrorMessage);
        Assert.Equal(AuthorizationGateKind.Hello, appSettings.Authorization.Gate);
        settings.Verify(s => s.SaveAsync(), Times.Never);
    }

    [Fact]
    public async Task ChangePasswordAsync_WhenActivatePasswordGate_SetsPasswordGateAfterPasswordChange()
    {
        var appSettings = new AppSettings
        {
            Authorization = new AuthorizationProfile
            {
                Gate = AuthorizationGateKind.Hello,
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
        auth.Setup(a => a.SetGateAsync(AuthorizationGateKind.Password))
            .ReturnsAsync(() =>
            {
                appSettings.Authorization.Gate = AuthorizationGateKind.Password;
                return AuthorizationResult.Success;
            });

        var sut = new SettingsAuthorizationWorkflowService(auth.Object, settings.Object, pwd.Object);

        var result = await sut.ChangePasswordAsync("new-pass", "new-pass", activatePasswordGate: true);

        Assert.True(result.IsSuccess);
        Assert.Equal(AuthorizationGateKind.Password, appSettings.Authorization.Gate);
        auth.Verify(a => a.SetGateAsync(AuthorizationGateKind.Password), Times.Once);
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
