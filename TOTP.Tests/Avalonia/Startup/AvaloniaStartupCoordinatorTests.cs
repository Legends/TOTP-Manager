using FluentResults;
using Microsoft.Extensions.Logging;
using Moq;
using TOTP.Core.Security;
using TOTP.Core.Security.Interfaces;
using TOTP.Avalonia.Desktop.Startup;

namespace TOTP.Tests.Avalonia.Startup;

public sealed class AvaloniaStartupCoordinatorTests
{
    [Fact]
    public async Task InitializeAsync_WhenAuthorizationIsConfigured_IsReadyForUnlock()
    {
        var state = new AuthorizationState();
        state.SetConfiguration(true, TOTP.Core.Enums.PreferredUnlockMethod.Password);
        var (sut, settings, authorization) = CreateSut(state);

        var outcome = await sut.InitializeAsync(TestContext.Current.CancellationToken);

        Assert.Equal(AvaloniaStartupOutcome.ReadyForUnlock, outcome);
        settings.Verify(service => service.LoadAsync(), Times.Once);
        authorization.Verify(service => service.InitializeAsync(), Times.Once);
    }

    [Fact]
    public async Task InitializeAsync_WhenAuthorizationIsNotConfigured_IsReadyForPasswordSetup()
    {
        var (sut, _, _) = CreateSut(new AuthorizationState());

        var outcome = await sut.InitializeAsync(TestContext.Current.CancellationToken);

        Assert.Equal(AvaloniaStartupOutcome.ReadyForPasswordSetup, outcome);
    }

    [Fact]
    public async Task InitializeAsync_WhenPreferencesFail_DoesNotInitializeAuthorization()
    {
        var (sut, settings, authorization) = CreateSut(new AuthorizationState());
        settings.Setup(service => service.LoadAsync())
            .ReturnsAsync(Result.Fail<IAppSettings>("synthetic preference failure"));

        var outcome = await sut.InitializeAsync(TestContext.Current.CancellationToken);

        Assert.Equal(AvaloniaStartupOutcome.PreferencesUnavailable, outcome);
        authorization.Verify(service => service.InitializeAsync(), Times.Never);
    }

    [Fact]
    public async Task InitializeAsync_WhenBoundaryThrows_ReturnsSanitizedFailure()
    {
        var (sut, settings, _) = CreateSut(new AuthorizationState());
        settings.Setup(service => service.LoadAsync())
            .ThrowsAsync(new InvalidOperationException("sensitive synthetic detail"));

        var outcome = await sut.InitializeAsync(TestContext.Current.CancellationToken);

        Assert.Equal(AvaloniaStartupOutcome.UnexpectedFailure, outcome);
    }

    private static (
        AvaloniaStartupCoordinator Sut,
        Mock<ISettingsService> Settings,
        Mock<IAuthorizationService> Authorization) CreateSut(AuthorizationState state)
    {
        var settings = new Mock<ISettingsService>();
        settings.Setup(service => service.LoadAsync())
            .ReturnsAsync(Result.Ok(Mock.Of<IAppSettings>()));

        var authorization = new Mock<IAuthorizationService>();
        authorization.SetupGet(service => service.State).Returns(state);

        return (
            new AvaloniaStartupCoordinator(
                settings.Object,
                authorization.Object,
                Mock.Of<ILogger<AvaloniaStartupCoordinator>>()),
            settings,
            authorization);
    }
}
