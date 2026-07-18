using Moq;
using TOTP.Core.Security.Interfaces;
using TOTP.Core.Services.Interfaces;
using TOTP.Core.Models;
using TOTP.Avalonia.Desktop.Presentation;
using TOTP.Avalonia.Desktop.Platform;
using TOTP.Avalonia.Desktop.Startup;
using Microsoft.Extensions.Logging.Abstractions;

namespace TOTP.Tests.Avalonia.Presentation;

public sealed class MainWindowViewModelTests
{
    [Theory]
    [InlineData(AvaloniaStartupOutcome.ReadyForPasswordSetup, false, "Create a master password")]
    [InlineData(AvaloniaStartupOutcome.ReadyForUnlock, false, "Enter your master password")]
    [InlineData(AvaloniaStartupOutcome.PreferencesUnavailable, true, "preferences could not be loaded")]
    [InlineData(AvaloniaStartupOutcome.UnexpectedFailure, true, "could not start safely")]
    public async Task InitializeAsync_ProjectsSafeRecoverableState(
        AvaloniaStartupOutcome outcome,
        bool canRetry,
        string expectedText)
    {
        var coordinator = new Mock<IAvaloniaStartupCoordinator>();
        coordinator.Setup(value => value.InitializeAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(outcome);
        using var sut = CreateSut(coordinator.Object);

        await sut.InitializeAsync();

        Assert.False(sut.IsBusy);
        Assert.Equal(canRetry, sut.CanRetry);
        Assert.Contains(expectedText, sut.StatusText, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(outcome == AvaloniaStartupOutcome.ReadyForUnlock, sut.IsPasswordUnlockVisible);
    }

    [Fact]
    public async Task InitializeAsync_WhenCoordinatorContractThrows_RemainsRecoverable()
    {
        var coordinator = new Mock<IAvaloniaStartupCoordinator>();
        coordinator.Setup(value => value.InitializeAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("sensitive synthetic detail"));
        using var sut = CreateSut(coordinator.Object);

        await sut.InitializeAsync();

        Assert.True(sut.CanRetry);
        Assert.DoesNotContain("sensitive", sut.StatusText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LockAsync_LocksAuthorizationAndReturnsToPasswordGate()
    {
        var coordinator = new Mock<IAvaloniaStartupCoordinator>();
        coordinator.Setup(value => value.InitializeAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(AvaloniaStartupOutcome.ReadyForUnlock);
        var authorization = new Mock<IAuthorizationService>();
        var password = new PasswordUnlockViewModel(authorization.Object);
        using var accounts = new AccountListViewModel(
            Mock.Of<IAccountManager>(),
            Mock.Of<IAccountTotpService>(),
            Mock.Of<IAsyncClipboardService>(),
            Mock.Of<IAccountQrCodeService>(),
            Mock.Of<IAvaloniaQrImageFactory>());
        using var sut = new MainWindowViewModel(
            coordinator.Object,
            authorization.Object,
            password,
            accounts,
            CreateSettingsPage(),
            CreateFilePicker(),
            CreateCameraScanner(),
            CreateUpdateCheck());
        await sut.InitializeAsync();

        await sut.LockAsync();

        authorization.Verify(value => value.Lock(), Times.Once);
        Assert.True(sut.IsPasswordUnlockVisible);
        Assert.False(sut.IsAccountListVisible);
        Assert.Contains("locked", sut.StatusText, StringComparison.OrdinalIgnoreCase);
    }

    private static MainWindowViewModel CreateSut(IAvaloniaStartupCoordinator coordinator) =>
        new(
            coordinator,
            Mock.Of<IAuthorizationService>(),
            new PasswordUnlockViewModel(Mock.Of<IAuthorizationService>()),
            new AccountListViewModel(
                Mock.Of<IAccountManager>(),
                Mock.Of<IAccountTotpService>(),
                Mock.Of<IAsyncClipboardService>(),
                Mock.Of<IAccountQrCodeService>(),
                Mock.Of<IAvaloniaQrImageFactory>()),
            CreateSettingsPage(),
            CreateFilePicker(),
            CreateCameraScanner(),
            CreateUpdateCheck());

    private static SettingsPageViewModel CreateSettingsPage()
    {
        var settings = new Mock<ISettingsService>();
        settings.SetupGet(value => value.Current).Returns(new AppSettings());
        return new SettingsPageViewModel(settings.Object);
    }

    private static NativeFilePickerViewModel CreateFilePicker() =>
        new(Mock.Of<IAvaloniaFilePicker>());

    private static CameraScannerViewModel CreateCameraScanner() =>
        new(
            Mock.Of<IQrScannerRunner>(),
            Mock.Of<IQrPayloadValidator>(),
            Mock.Of<IAvaloniaQrImageFactory>(),
            Mock.Of<IUiScheduler>(),
            NullLogger<CameraScannerViewModel>.Instance);

    private static UpdateCheckViewModel CreateUpdateCheck() =>
        new(Mock.Of<ISignedAppcastVerifier>());
}
