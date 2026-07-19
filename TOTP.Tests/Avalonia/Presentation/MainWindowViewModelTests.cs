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
using TOTP.Core.Security;
using TOTP.Core.Enums;
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
    public async Task LockAsync_WhenPasswordPreferred_ReturnsToPasswordGate()
    {
        var coordinator = new Mock<IAvaloniaStartupCoordinator>();
        coordinator.Setup(value => value.InitializeAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(AvaloniaStartupOutcome.ReadyUnlocked);
        var authorization = new Mock<IAuthorizationService>();
        var state = CreateAuthorizationState(PreferredUnlockMethod.Password);
        authorization.SetupGet(value => value.State).Returns(state);
        authorization.Setup(value => value.Lock()).Callback(state.Lock);
        using var sut = CreateSut(coordinator.Object, authorization.Object);
        await sut.InitializeAsync();

        await sut.LockAsync();

        authorization.Verify(value => value.Lock(), Times.Once);
        Assert.True(sut.IsPasswordUnlockVisible);
        Assert.False(sut.IsQuickUnlockVisible);
        Assert.False(sut.IsAccountListVisible);
        Assert.Contains("locked", sut.StatusText, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(NotificationSeverity.Information, sut.StatusSeverity);
    }

    [Fact]
    public async Task LockAsync_WhenQuickUnlockPreferred_ReturnsToQuickUnlockGate()
    {
        var coordinator = new Mock<IAvaloniaStartupCoordinator>();
        coordinator.Setup(value => value.InitializeAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(AvaloniaStartupOutcome.ReadyUnlocked);
        var authorization = new Mock<IAuthorizationService>();
        var state = CreateAuthorizationState(PreferredUnlockMethod.PlatformQuickUnlock);
        authorization.SetupGet(value => value.State).Returns(state);
        authorization.Setup(value => value.Lock()).Callback(state.Lock);
        using var sut = CreateSut(coordinator.Object, authorization.Object);
        await sut.InitializeAsync();

        await sut.LockAsync();

        Assert.True(sut.IsQuickUnlockVisible);
        Assert.False(sut.IsPasswordUnlockVisible);
        Assert.Equal(PreferredUnlockMethod.PlatformQuickUnlock, state.PreferredUnlockMethod);
    }

    [Fact]
    public async Task TryQuickUnlockAsync_WhenAuthorized_ReentersShell()
    {
        var coordinator = new Mock<IAvaloniaStartupCoordinator>();
        coordinator.Setup(value => value.InitializeAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(AvaloniaStartupOutcome.ReadyUnlocked);
        var authorization = new Mock<IAuthorizationService>();
        var state = CreateAuthorizationState(PreferredUnlockMethod.PlatformQuickUnlock);
        authorization.SetupGet(value => value.State).Returns(state);
        authorization.Setup(value => value.Lock()).Callback(state.Lock);
        authorization.Setup(value => value.TryUnlockWithHelloAsync(It.IsAny<CancellationToken>()))
            .Returns<CancellationToken>(_ =>
            {
                state.Unlock();
                return Task.FromResult(AuthorizationResult.Success);
            });
        using var sut = CreateSut(coordinator.Object, authorization.Object);
        await sut.InitializeAsync();
        await sut.LockAsync();

        await sut.TryQuickUnlockAsync();

        Assert.True(sut.IsShellVisible);
        Assert.True(sut.IsAccountListVisible);
        Assert.False(sut.IsQuickUnlockVisible);
        Assert.False(sut.IsPasswordUnlockVisible);
    }

    [Fact]
    public async Task TryQuickUnlockAsync_WhenCancelled_RemainsAtQuickUnlockGate()
    {
        var coordinator = new Mock<IAvaloniaStartupCoordinator>();
        coordinator.Setup(value => value.InitializeAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(AvaloniaStartupOutcome.ReadyUnlocked);
        var authorization = new Mock<IAuthorizationService>();
        var state = CreateAuthorizationState(PreferredUnlockMethod.PlatformQuickUnlock);
        authorization.SetupGet(value => value.State).Returns(state);
        authorization.Setup(value => value.Lock()).Callback(state.Lock);
        authorization.Setup(value => value.TryUnlockWithHelloAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(AuthorizationResult.Cancelled);
        using var sut = CreateSut(coordinator.Object, authorization.Object);
        await sut.InitializeAsync();
        await sut.LockAsync();

        await sut.TryQuickUnlockAsync();

        Assert.True(sut.IsQuickUnlockVisible);
        Assert.False(sut.IsPasswordUnlockVisible);
        Assert.Contains(AvaloniaStringKeys.QuickUnlockCancelled, sut.QuickUnlockMessage);
    }

    [Fact]
    public async Task TryQuickUnlockAsync_WhenPasswordIsRequired_ShowsRecoveryGate()
    {
        var coordinator = new Mock<IAvaloniaStartupCoordinator>();
        coordinator.Setup(value => value.InitializeAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(AvaloniaStartupOutcome.ReadyUnlocked);
        var authorization = new Mock<IAuthorizationService>();
        var state = CreateAuthorizationState(PreferredUnlockMethod.PlatformQuickUnlock);
        authorization.SetupGet(value => value.State).Returns(state);
        authorization.Setup(value => value.Lock()).Callback(state.Lock);
        authorization.Setup(value => value.TryUnlockWithHelloAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(AuthorizationResult.PasswordRequired);
        using var sut = CreateSut(coordinator.Object, authorization.Object);
        await sut.InitializeAsync();
        await sut.LockAsync();

        await sut.TryQuickUnlockAsync();

        Assert.False(sut.IsQuickUnlockVisible);
        Assert.True(sut.IsPasswordUnlockVisible);
        Assert.False(sut.IsShellVisible);
    }

    [Fact]
    public async Task UsePasswordFallbackAsync_DoesNotChangeQuickUnlockPreference()
    {
        var coordinator = new Mock<IAvaloniaStartupCoordinator>();
        coordinator.Setup(value => value.InitializeAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(AvaloniaStartupOutcome.ReadyUnlocked);
        var authorization = new Mock<IAuthorizationService>();
        var state = CreateAuthorizationState(PreferredUnlockMethod.PlatformQuickUnlock);
        authorization.SetupGet(value => value.State).Returns(state);
        authorization.Setup(value => value.Lock()).Callback(state.Lock);
        using var sut = CreateSut(coordinator.Object, authorization.Object);
        await sut.InitializeAsync();
        await sut.LockAsync();

        await sut.UsePasswordFallbackAsync();

        Assert.False(sut.IsQuickUnlockVisible);
        Assert.True(sut.IsPasswordUnlockVisible);
        Assert.Equal(PreferredUnlockMethod.PlatformQuickUnlock, state.PreferredUnlockMethod);
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
        Assert.True(sut.CloseSettingsCommand.CanExecute(null));
        Assert.False(sut.LockCommand.CanExecute(null));
        Assert.False(sut.ToggleSearchCommand.CanExecute(null));
        Assert.False(sut.BeginAddAccountCommand.CanExecute(null));

        await sut.CloseSettingsAsync();
        Assert.False(sut.IsAccountListVisible);
        Assert.True(sut.IsToolsVisible);
        Assert.False(sut.IsSettingsVisible);
        Assert.True(sut.LockCommand.CanExecute(null));
    }

    [Fact]
    public async Task ToolbarSearch_TogglesClearsAndReturnsToAccounts()
    {
        var coordinator = new Mock<IAvaloniaStartupCoordinator>();
        coordinator.Setup(value => value.InitializeAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(AvaloniaStartupOutcome.ReadyUnlocked);
        using var sut = CreateSut(coordinator.Object);
        await sut.InitializeAsync();

        await sut.ToggleSearchAsync();
        Assert.True(sut.IsSearchVisible);
        sut.AccountList.SearchText = "github";

        await sut.ToggleSearchAsync();
        Assert.False(sut.IsSearchVisible);
        Assert.Empty(sut.AccountList.SearchText);

        await sut.ShowToolsAsync();
        await sut.ToggleSearchAsync();
        Assert.True(sut.IsAccountListVisible);
        Assert.False(sut.IsToolsVisible);
        Assert.True(sut.IsSearchVisible);
    }

    [Fact]
    public async Task HandleWindowMinimizedAsync_WhenPolicyEnabled_LocksAuthorizedShell()
    {
        var coordinator = new Mock<IAvaloniaStartupCoordinator>();
        coordinator.Setup(value => value.InitializeAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(AvaloniaStartupOutcome.ReadyUnlocked);
        var authorization = new Mock<IAuthorizationService>();
        var settings = new Mock<ISettingsService>();
        settings.SetupGet(value => value.Current).Returns(new AppSettings
        {
            LockOnMinimize = true
        });
        using var sut = new MainWindowViewModel(
            coordinator.Object,
            authorization.Object,
            new PasswordUnlockViewModel(authorization.Object),
            CreatePasswordSetup(authorization.Object),
            new AccountListViewModel(
                Mock.Of<IAccountManager>(),
                Mock.Of<IAccountTotpService>(),
                Mock.Of<IAsyncClipboardService>(),
                Mock.Of<IAccountQrCodeService>(),
                Mock.Of<IAvaloniaQrImageFactory>(),
                Mock.Of<IAvaloniaDialogService>(),
                CreateLocalization()),
            CreateSettingsPage(),
            CreateAuthorizationSettings(authorization.Object),
            CreateFilePicker(),
            CreateCameraScanner(),
            CreateUpdateCheck(),
            CreateDiagnostics(),
            CreateLocalization(),
            settings.Object);
        await sut.InitializeAsync();

        await sut.HandleWindowMinimizedAsync();

        authorization.Verify(value => value.Lock(), Times.Once);
        Assert.True(sut.IsPasswordUnlockVisible);
        Assert.False(sut.IsShellVisible);
    }

    private static MainWindowViewModel CreateSut(IAvaloniaStartupCoordinator coordinator) =>
        CreateSut(coordinator, Mock.Of<IAuthorizationService>());

    private static MainWindowViewModel CreateSut(
        IAvaloniaStartupCoordinator coordinator,
        IAuthorizationService authorization) =>
        new(
            coordinator,
            authorization,
            new PasswordUnlockViewModel(authorization),
            CreatePasswordSetup(authorization),
            new AccountListViewModel(
                Mock.Of<IAccountManager>(),
                Mock.Of<IAccountTotpService>(),
                Mock.Of<IAsyncClipboardService>(),
                Mock.Of<IAccountQrCodeService>(),
                Mock.Of<IAvaloniaQrImageFactory>(),
                Mock.Of<IAvaloniaDialogService>(),
                CreateLocalization()),
            CreateSettingsPage(),
            CreateAuthorizationSettings(authorization),
            CreateFilePicker(),
            CreateCameraScanner(),
            CreateUpdateCheck(),
            CreateDiagnostics(),
            CreateLocalization());

    private static AuthorizationState CreateAuthorizationState(PreferredUnlockMethod preference)
    {
        var state = new AuthorizationState();
        state.SetConfiguration(true, preference);
        state.Unlock();
        return state;
    }

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
