using Moq;
using TOTP.Avalonia.Desktop.Localization;
using TOTP.Avalonia.Desktop.Platform;
using TOTP.Avalonia.Desktop.Presentation;
using TOTP.Avalonia.Desktop.Presentation.Dialogs;
using TOTP.Core.Enums;
using TOTP.Core.Security;
using TOTP.Core.Security.Interfaces;
using TOTP.Core.Security.Models;
using TOTP.Core.Services.Interfaces;

namespace TOTP.Tests.Avalonia.Presentation;

public sealed class AuthorizationSettingsViewModelTests
{
    [Fact]
    public async Task RefreshAsync_ProjectsPlatformAvailabilityAndCurrentPreference()
    {
        var state = ConfiguredState(PreferredUnlockMethod.PlatformQuickUnlock);
        var authorization = Authorization(state);
        authorization.Setup(value => value.IsHelloAvailableAsync()).ReturnsAsync(true);
        var sut = CreateSut(authorization.Object, Mock.Of<IAvaloniaDialogService>());

        await sut.RefreshAsync();

        Assert.True(sut.IsQuickUnlockAvailable);
        Assert.True(sut.IsQuickUnlockEnabled);
        Assert.Equal(AvaloniaStringKeys.QuickUnlockAvailable, sut.Message);
    }

    [Fact]
    public async Task EnableQuickUnlockAsync_WhenEnrollmentIsRequired_RequiresRecoveryPassword()
    {
        var state = ConfiguredState(PreferredUnlockMethod.Password);
        var authorization = Authorization(state);
        authorization.Setup(value => value.IsHelloAvailableAsync()).ReturnsAsync(true);
        authorization.Setup(value => value.SetGateAsync(AuthorizationGateKind.Hello))
            .ReturnsAsync(AuthorizationResult.PasswordRequired);
        authorization.Setup(value => value.ConfigureHelloAsync("recovery-password"))
            .ReturnsAsync(AuthorizationResult.Success);
        var dialogs = new Mock<IAvaloniaDialogService>();
        dialogs.Setup(value => value.PromptForPasswordAsync(
                It.IsAny<PasswordDialogRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("recovery-password");
        var sut = CreateSut(authorization.Object, dialogs.Object);
        await sut.RefreshAsync();

        await sut.EnableQuickUnlockAsync();

        authorization.Verify(value => value.ConfigureHelloAsync("recovery-password"), Times.Once);
        Assert.True(sut.IsQuickUnlockEnabled);
        Assert.Equal(NotificationSeverity.Success, sut.MessageSeverity);
    }

    [Fact]
    public async Task EnableQuickUnlockAsync_WhenRecoveryPromptIsCancelled_PreservesPasswordPreference()
    {
        var state = ConfiguredState(PreferredUnlockMethod.Password);
        var authorization = Authorization(state);
        authorization.Setup(value => value.IsHelloAvailableAsync()).ReturnsAsync(true);
        authorization.Setup(value => value.SetGateAsync(AuthorizationGateKind.Hello))
            .ReturnsAsync(AuthorizationResult.PasswordRequired);
        var dialogs = new Mock<IAvaloniaDialogService>();
        dialogs.Setup(value => value.PromptForPasswordAsync(
                It.IsAny<PasswordDialogRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);
        var sut = CreateSut(authorization.Object, dialogs.Object);
        await sut.RefreshAsync();

        await sut.EnableQuickUnlockAsync();

        authorization.Verify(value => value.ConfigureHelloAsync(It.IsAny<string>()), Times.Never);
        Assert.False(sut.IsQuickUnlockEnabled);
        Assert.Equal(AvaloniaStringKeys.QuickUnlockEnrollmentCancelled, sut.Message);
    }

    [Fact]
    public async Task UsePasswordAsync_VerifiesMasterPasswordBeforeChangingPreference()
    {
        var state = ConfiguredState(PreferredUnlockMethod.PlatformQuickUnlock);
        var authorization = Authorization(state);
        authorization.Setup(value => value.IsHelloAvailableAsync()).ReturnsAsync(true);
        authorization.Setup(value => value.TryUnlockWithPasswordAsync("master-password"))
            .ReturnsAsync(AuthorizationResult.Success);
        authorization.Setup(value => value.SetGateAsync(AuthorizationGateKind.Password))
            .ReturnsAsync(AuthorizationResult.Success);
        var dialogs = new Mock<IAvaloniaDialogService>();
        dialogs.Setup(value => value.PromptForPasswordAsync(
                It.IsAny<PasswordDialogRequest>(),
                It.IsAny<CancellationToken>()))
            .Returns(async (PasswordDialogRequest request, CancellationToken cancellationToken) =>
            {
                var error = await request.ValidateAsync!("master-password", cancellationToken);
                return error is null ? "master-password" : null;
            });
        var sut = CreateSut(authorization.Object, dialogs.Object);
        await sut.RefreshAsync();

        await sut.UsePasswordAsync();

        authorization.Verify(value => value.TryUnlockWithPasswordAsync("master-password"), Times.Once);
        authorization.Verify(value => value.SetGateAsync(AuthorizationGateKind.Password), Times.Once);
        Assert.False(sut.IsQuickUnlockEnabled);
    }

    private static AuthorizationSettingsViewModel CreateSut(
        IAuthorizationService authorization,
        IAvaloniaDialogService dialogs) =>
        new(authorization, dialogs, Localization());

    private static Mock<IAuthorizationService> Authorization(AuthorizationState state)
    {
        var authorization = new Mock<IAuthorizationService>();
        authorization.SetupGet(value => value.State).Returns(state);
        return authorization;
    }

    private static AuthorizationState ConfiguredState(PreferredUnlockMethod preference)
    {
        var state = new AuthorizationState();
        state.SetConfiguration(true, preference);
        return state;
    }

    private static IAvaloniaLocalizationService Localization()
    {
        var localization = new Mock<IAvaloniaLocalizationService>();
        localization.Setup(value => value.GetString(It.IsAny<string>()))
            .Returns((string key) => key);
        return localization.Object;
    }
}
