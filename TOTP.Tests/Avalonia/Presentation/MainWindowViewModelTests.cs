using Moq;
using TOTP.Core.Security.Interfaces;
using TOTP.Core.Services.Interfaces;
using TOTP.Core.Models;
using TOTP.Avalonia.Desktop.Presentation;
using TOTP.Avalonia.Desktop.Platform;
using TOTP.Avalonia.Desktop.Startup;
using Microsoft.Extensions.Logging.Abstractions;
using FluentResults;
using TOTP.Core.Security.Models;
using TOTP.Avalonia.Desktop.Localization;

namespace TOTP.Tests.Avalonia.Presentation;

public sealed class MainWindowViewModelTests
{
    [Theory]
    [InlineData(AvaloniaStartupOutcome.ReadyForPasswordSetup, false, "Create a master password")]
    [InlineData(AvaloniaStartupOutcome.ReadyForUnlock, false, "Enter your master password")]
    [InlineData(AvaloniaStartupOutcome.ReadyForPasswordFallback, false, "QuickUnlockFallback")]
    [InlineData(AvaloniaStartupOutcome.ReadyUnlocked, false, "VaultUnlocked")]
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
        Assert.Equal(
            outcome is AvaloniaStartupOutcome.ReadyForUnlock or AvaloniaStartupOutcome.ReadyForPasswordFallback,
            sut.IsPasswordUnlockVisible);
        Assert.Equal(outcome == AvaloniaStartupOutcome.ReadyForPasswordSetup, sut.IsPasswordSetupVisible);
        Assert.Equal(outcome == AvaloniaStartupOutcome.ReadyUnlocked, sut.IsShellVisible);
        Assert.Equal(
            outcome switch
            {
                AvaloniaStartupOutcome.PreferencesUnavailable => NotificationSeverity.Warning,
                AvaloniaStartupOutcome.UnexpectedFailure => NotificationSeverity.Error,
                AvaloniaStartupOutcome.ReadyForPasswordFallback => NotificationSeverity.Warning,
                AvaloniaStartupOutcome.ReadyUnlocked => NotificationSeverity.Success,
                _ => NotificationSeverity.Information
            },
            sut.StatusSeverity);
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
        Assert.Equal(NotificationSeverity.Error, sut.StatusSeverity);
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
            Mock.Of<IAvaloniaQrImageFactory>(),
            Mock.Of<IAvaloniaDialogService>(),
            CreateLocalization());
        using var sut = new MainWindowViewModel(
            coordinator.Object,
            authorization.Object,
            password,
            CreatePasswordSetup(authorization.Object),
            accounts,
            CreateSettingsPage(),
            CreateAuthorizationSettings(authorization.Object),
            CreateFilePicker(),
            CreateCameraScanner(),
            CreateUpdateCheck(),
            CreateDiagnostics(),
            CreateLocalization());
        await sut.InitializeAsync();

        await sut.LockAsync();

        authorization.Verify(value => value.Lock(), Times.Once);
        Assert.True(sut.IsPasswordUnlockVisible);
        Assert.False(sut.IsAccountListVisible);
        Assert.Contains("locked", sut.StatusText, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(NotificationSeverity.Information, sut.StatusSeverity);
    }

    [Fact]
    public void PrepareForShutdown_LocksOnceAndHidesAuthorizedSurfaces()
    {
        var coordinator = new Mock<IAvaloniaStartupCoordinator>();
        var authorization = new Mock<IAuthorizationService>();
        using var accounts = new AccountListViewModel(
            Mock.Of<IAccountManager>(),
            Mock.Of<IAccountTotpService>(),
            Mock.Of<IAsyncClipboardService>(),
            Mock.Of<IAccountQrCodeService>(),
            Mock.Of<IAvaloniaQrImageFactory>(),
            Mock.Of<IAvaloniaDialogService>(),
            CreateLocalization());
        using var sut = new MainWindowViewModel(
            coordinator.Object,
            authorization.Object,
            new PasswordUnlockViewModel(authorization.Object),
            CreatePasswordSetup(authorization.Object),
            accounts,
            CreateSettingsPage(),
            CreateAuthorizationSettings(authorization.Object),
            CreateFilePicker(),
            CreateCameraScanner(),
            CreateUpdateCheck(),
            CreateDiagnostics(),
            CreateLocalization());

        sut.PrepareForShutdown();
        sut.PrepareForShutdown();

        authorization.Verify(value => value.Lock(), Times.Once);
        Assert.False(sut.IsPasswordUnlockVisible);
        Assert.False(sut.IsAccountListVisible);
        Assert.False(sut.IsSettingsVisible);
        Assert.Contains("closing", sut.StatusText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AuthorizedNavigation_ExposesExactlyOnePage()
    {
        var coordinator = new Mock<IAvaloniaStartupCoordinator>();
        coordinator.Setup(value => value.InitializeAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(AvaloniaStartupOutcome.ReadyForUnlock);
        var authorization = new Mock<IAuthorizationService>();
        authorization.Setup(value => value.TryUnlockWithPasswordAsync("test-password"))
            .ReturnsAsync(AuthorizationResult.Success);
        var manager = new Mock<IAccountManager>();
        manager.Setup(value => value.GetAllOtpEntriesSortedAsync())
            .ReturnsAsync(Result.Ok<IReadOnlyList<Account>>([]));
        var password = new PasswordUnlockViewModel(authorization.Object);
        using var accounts = new AccountListViewModel(
            manager.Object,
            Mock.Of<IAccountTotpService>(),
            Mock.Of<IAsyncClipboardService>(),
            Mock.Of<IAccountQrCodeService>(),
            Mock.Of<IAvaloniaQrImageFactory>(),
            Mock.Of<IAvaloniaDialogService>(),
            CreateLocalization());
        using var sut = new MainWindowViewModel(
            coordinator.Object,
            authorization.Object,
            password,
            CreatePasswordSetup(authorization.Object),
            accounts,
            CreateSettingsPage(),
            CreateAuthorizationSettings(authorization.Object),
            CreateFilePicker(),
            CreateCameraScanner(),
            CreateUpdateCheck(),
            CreateDiagnostics(),
            CreateLocalization());
        await sut.InitializeAsync();
        password.Password = "test-password";

        await password.UnlockAsync();

        Assert.True(sut.IsShellVisible);
        Assert.True(sut.IsAccountListVisible);
        Assert.False(sut.IsToolsVisible);
        Assert.False(sut.IsSettingsVisible);

        await sut.ShowToolsAsync();
        Assert.False(sut.IsAccountListVisible);
        Assert.True(sut.IsToolsVisible);
        Assert.False(sut.IsSettingsVisible);

        await sut.ShowSettingsAsync();
        Assert.False(sut.IsAccountListVisible);
        Assert.False(sut.IsToolsVisible);
        Assert.True(sut.IsSettingsVisible);
    }

    private static MainWindowViewModel CreateSut(IAvaloniaStartupCoordinator coordinator) =>
        new(
            coordinator,
            Mock.Of<IAuthorizationService>(),
            new PasswordUnlockViewModel(Mock.Of<IAuthorizationService>()),
            CreatePasswordSetup(Mock.Of<IAuthorizationService>()),
            new AccountListViewModel(
                Mock.Of<IAccountManager>(),
                Mock.Of<IAccountTotpService>(),
                Mock.Of<IAsyncClipboardService>(),
                Mock.Of<IAccountQrCodeService>(),
                Mock.Of<IAvaloniaQrImageFactory>(),
                Mock.Of<IAvaloniaDialogService>(),
                CreateLocalization()),
            CreateSettingsPage(),
            CreateAuthorizationSettings(Mock.Of<IAuthorizationService>()),
            CreateFilePicker(),
            CreateCameraScanner(),
            CreateUpdateCheck(),
            CreateDiagnostics(),
            CreateLocalization());

    private static SettingsPageViewModel CreateSettingsPage()
    {
        var settings = new Mock<ISettingsService>();
        settings.SetupGet(value => value.Current).Returns(new AppSettings());
        return new SettingsPageViewModel(settings.Object);
    }

    private static AuthorizationSettingsViewModel CreateAuthorizationSettings(
        IAuthorizationService authorization)
    {
        var validation = new Mock<IPasswordValidationService>();
        validation.SetupGet(value => value.MinimumLength).Returns(8);
        return new AuthorizationSettingsViewModel(
            authorization,
            Mock.Of<IAvaloniaDialogService>(),
            CreateLocalization(),
            validation.Object);
    }

    private static NativeFilePickerViewModel CreateFilePicker()
    {
        var validation = new Mock<IPasswordValidationService>();
        validation.SetupGet(value => value.MinimumLength).Returns(8);
        var settings = new Mock<ISettingsService>();
        settings.SetupGet(value => value.Current).Returns(new AppSettings());
        return new NativeFilePickerViewModel(
            Mock.Of<IAvaloniaFilePicker>(),
            Mock.Of<IExportService>(),
            Mock.Of<IAccountManager>(),
            Mock.Of<IAccountImportService>(),
            Mock.Of<IAvaloniaDialogService>(),
            validation.Object,
            Mock.Of<IPlatformFileSecurity>(),
            settings.Object,
            Mock.Of<IPlatformFolderLauncher>());
    }

    private static CameraScannerViewModel CreateCameraScanner() =>
        new(
            Mock.Of<IQrScannerRunner>(),
            Mock.Of<IQrPayloadValidator>(),
            Mock.Of<IAvaloniaQrImageFactory>(),
            Mock.Of<IUiScheduler>(),
            NullLogger<CameraScannerViewModel>.Instance,
            Mock.Of<IQrAccountImportService>(),
            Mock.Of<IAvaloniaDialogService>(),
            CreateLocalization());

    private static UpdateCheckViewModel CreateUpdateCheck() =>
        new(Mock.Of<IPortableUpdateService>(), Mock.Of<IUpdateInstallerLauncher>());

    private static DiagnosticsViewModel CreateDiagnostics() =>
        new(Mock.Of<ISupportDiagnosticsService>());

    private static PasswordSetupViewModel CreatePasswordSetup(IAuthorizationService authorization)
    {
        var validation = new Mock<IPasswordValidationService>();
        validation.SetupGet(value => value.MinimumLength).Returns(8);
        return new PasswordSetupViewModel(authorization, validation.Object, CreateLocalization());
    }

    private static IAvaloniaLocalizationService CreateLocalization()
    {
        var localization = new Mock<IAvaloniaLocalizationService>();
        localization.Setup(value => value.GetString(It.IsAny<string>()))
            .Returns((string key) => key);
        return localization.Object;
    }
}
