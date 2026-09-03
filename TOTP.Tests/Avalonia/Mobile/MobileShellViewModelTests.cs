using System.Globalization;
using Avalonia.Media;
using FluentResults;
using Moq;
using TOTP.Avalonia.Mobile.Localization;
using TOTP.Avalonia.Mobile.Platform;
using TOTP.Avalonia.Mobile.Presentation;
using TOTP.Core.Models;
using TOTP.Core.Security;
using TOTP.Core.Security.Interfaces;
using TOTP.Core.Security.Models;
using TOTP.Core.Services.Interfaces;
using TOTP.Core.Services.Models;

namespace TOTP.Tests.Avalonia.Mobile;

public sealed class MobileShellViewModelTests
{
    private const string ValidSecret = "JBSWY3DPEHPK3PXP";

    [Fact]
    public async Task InitializeAsync_WhenNoEnvelopeExists_ShowsPasswordSetup()
    {
        var context = CreateContext(isConfigured: false);

        await context.Sut.InitializeAsync();

        Assert.True(context.Sut.IsSetupVisible);
        Assert.False(context.Sut.IsUnlockVisible);
        Assert.Empty(context.Sut.NotificationText);
    }

    [Fact]
    public async Task ConfigureAsync_WhenSuccessful_OpensEmptyEncryptedVault()
    {
        var context = CreateContext(isConfigured: false);
        context.Authorization
            .Setup(value => value.ConfigurePasswordAsync("synthetic password", "synthetic password"))
            .ReturnsAsync(AuthorizationResult.Success);
        await context.Sut.InitializeAsync();
        context.Sut.SetupPassword = "synthetic password";
        context.Sut.SetupConfirmation = "synthetic password";

        await context.Sut.ConfigureAsync();

        Assert.True(context.Sut.IsAccountsVisible);
        Assert.True(context.Sut.HasNoAccounts);
        Assert.Empty(context.Sut.SetupPassword);
        Assert.Empty(context.Sut.SetupConfirmation);
    }

    [Fact]
    public async Task OnEnteredBackground_WhenDeviceIsLocked_LocksAndClearsAccountProjection()
    {
        var account = new Account(Guid.NewGuid(), "Example", ValidSecret, "user@example.test");
        var context = CreateContext(isConfigured: true, [account]);
        context.Authorization
            .Setup(value => value.TryUnlockWithPasswordAsync("synthetic password"))
            .Callback(context.State.Unlock)
            .ReturnsAsync(AuthorizationResult.Success);
        context.AccountTotp
            .Setup(value => value.GenerateAsync(account.ID))
            .ReturnsAsync(Result.Ok(new TotpGenerationResult("123456", 20, 30)));
        await context.Sut.InitializeAsync();
        context.Sut.UnlockPassword = "synthetic password";
        await context.Sut.UnlockAsync();

        context.Sut.OnEnteredBackground(lockImmediately: true);

        context.Authorization.Verify(value => value.Lock(), Times.Once);
        Assert.True(context.Sut.IsUnlockVisible);
        Assert.Empty(context.Sut.Accounts);
        Assert.Empty(context.Sut.SelectedCode);
        Assert.Null(context.Sut.SelectedAccount);
    }

    [Fact]
    public async Task OnReturnedToForeground_WithinGracePeriod_RemainsUnlocked()
    {
        var account = new Account(Guid.NewGuid(), "Example", ValidSecret, "user@example.test");
        var context = CreateContext(isConfigured: true, [account]);
        context.Authorization
            .Setup(value => value.TryUnlockWithPasswordAsync("synthetic password"))
            .Callback(context.State.Unlock)
            .ReturnsAsync(AuthorizationResult.Success);
        context.AccountTotp
            .Setup(value => value.GenerateAsync(account.ID))
            .ReturnsAsync(Result.Ok(new TotpGenerationResult("123456", 20, 30)));
        await context.Sut.InitializeAsync();
        context.Sut.UnlockPassword = "synthetic password";
        await context.Sut.UnlockAsync();

        Assert.Equal("123456", context.Sut.SelectedCode);
        context.Sut.OnEnteredBackground(lockImmediately: false);
        Assert.Empty(context.Sut.SelectedCode);
        context.Time.Advance(TimeSpan.FromSeconds(29));
        context.Sut.OnReturnedToForeground();

        context.Authorization.Verify(value => value.Lock(), Times.Never);
        Assert.True(context.Sut.IsAccountsVisible);
        Assert.Single(context.Sut.Accounts);
    }

    [Fact]
    public async Task OnReturnedToForeground_WhenGracePeriodExpired_LocksAndClearsAccountProjection()
    {
        var account = new Account(Guid.NewGuid(), "Example", ValidSecret, "user@example.test");
        var context = CreateContext(isConfigured: true, [account]);
        context.Authorization
            .Setup(value => value.TryUnlockWithPasswordAsync("synthetic password"))
            .Callback(context.State.Unlock)
            .ReturnsAsync(AuthorizationResult.Success);
        context.AccountTotp
            .Setup(value => value.GenerateAsync(account.ID))
            .ReturnsAsync(Result.Ok(new TotpGenerationResult("123456", 20, 30)));
        await context.Sut.InitializeAsync();
        context.Sut.UnlockPassword = "synthetic password";
        await context.Sut.UnlockAsync();

        context.Sut.OnEnteredBackground(lockImmediately: false);
        context.Time.Advance(TimeSpan.FromSeconds(30));
        context.Sut.OnReturnedToForeground();

        context.Authorization.Verify(value => value.Lock(), Times.Once);
        Assert.True(context.Sut.IsUnlockVisible);
        Assert.Empty(context.Sut.Accounts);
        Assert.Null(context.Sut.SelectedAccount);
    }

    [Fact]
    public async Task OnReturnedToForeground_WhenGracePeriodExpiresWithBiometrics_AutomaticallyUnlocks()
    {
        var account = new Account(Guid.NewGuid(), "Example", ValidSecret, "user@example.test");
        var context = CreateContext(
            isConfigured: true,
            accounts: [account],
            biometricAvailable: true,
            preferredUnlockMethod: TOTP.Core.Enums.PreferredUnlockMethod.PlatformQuickUnlock);
        context.Authorization
            .Setup(value => value.TryUnlockWithPasswordAsync("synthetic password"))
            .Callback(context.State.Unlock)
            .ReturnsAsync(AuthorizationResult.Success);
        context.Authorization
            .Setup(value => value.TryUnlockWithHelloAsync())
            .Callback(context.State.Unlock)
            .ReturnsAsync(AuthorizationResult.Success);
        context.AccountTotp
            .Setup(value => value.GenerateAsync(account.ID))
            .ReturnsAsync(Result.Ok(new TotpGenerationResult("123456", 20, 30)));
        await context.Sut.InitializeAsync();
        context.Sut.UnlockPassword = "synthetic password";
        await context.Sut.UnlockAsync();

        context.Sut.OnEnteredBackground(lockImmediately: false);
        context.Time.Advance(TimeSpan.FromSeconds(30));
        context.Sut.OnReturnedToForeground();

        context.Authorization.Verify(value => value.Lock(), Times.Once);
        context.Authorization.Verify(value => value.TryUnlockWithHelloAsync(), Times.Once);
        Assert.True(context.Sut.IsAccountsVisible);
        Assert.Single(context.Sut.Accounts);
    }

    [Fact]
    public async Task UnlockAsync_ProjectsNoSecretIntoMobileAccountRows()
    {
        var account = new Account(Guid.NewGuid(), "Example", ValidSecret, "user@example.test");
        var context = CreateContext(isConfigured: true, [account]);
        context.Authorization
            .Setup(value => value.TryUnlockWithPasswordAsync(It.IsAny<string>()))
            .ReturnsAsync(AuthorizationResult.Success);
        context.AccountTotp
            .Setup(value => value.GenerateAsync(account.ID))
            .ReturnsAsync(Result.Ok(new TotpGenerationResult("123456", 20, 30)));
        await context.Sut.InitializeAsync();
        context.Sut.UnlockPassword = "synthetic password";

        await context.Sut.UnlockAsync();

        var projected = Assert.Single(context.Sut.Accounts);
        Assert.Equal(account.ID, projected.Id);
        Assert.DoesNotContain(
            typeof(MobileAccountItem).GetProperties(),
            property => property.Name.Contains("Secret", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task InitializeAsync_WithConfiguredBiometrics_OffersBiometricUnlockAndPasswordFallback()
    {
        var context = CreateContext(
            isConfigured: true,
            biometricAvailable: true,
            preferredUnlockMethod: TOTP.Core.Enums.PreferredUnlockMethod.PlatformQuickUnlock);

        await context.Sut.InitializeAsync();

        Assert.True(context.Sut.IsBiometricUnlockVisible);
        Assert.True(context.Sut.IsUnlockVisible);
        Assert.False(context.Sut.UnlockCommand.CanExecute(null));
        Assert.True(context.Sut.BiometricUnlockCommand.CanExecute(null));
    }

    [Fact]
    public async Task OnReturnedToForeground_WithConfiguredBiometrics_AutomaticallyPromptsAndUnlocks()
    {
        var context = CreateContext(
            isConfigured: true,
            biometricAvailable: true,
            preferredUnlockMethod: TOTP.Core.Enums.PreferredUnlockMethod.PlatformQuickUnlock);
        context.Authorization
            .Setup(value => value.TryUnlockWithHelloAsync())
            .Callback(context.State.Unlock)
            .ReturnsAsync(AuthorizationResult.Success);

        context.Sut.OnReturnedToForeground();
        await context.Sut.InitializeAsync();

        Assert.True(context.Sut.IsAccountsVisible);
        context.Authorization.Verify(value => value.TryUnlockWithHelloAsync(), Times.Once);
    }

    [Fact]
    public async Task OnReturnedToForeground_WhenAutomaticBiometricPromptIsCancelled_KeepsPasswordFallback()
    {
        var context = CreateContext(
            isConfigured: true,
            biometricAvailable: true,
            preferredUnlockMethod: TOTP.Core.Enums.PreferredUnlockMethod.PlatformQuickUnlock);
        context.Authorization
            .Setup(value => value.TryUnlockWithHelloAsync())
            .ReturnsAsync(AuthorizationResult.Cancelled);

        context.Sut.OnReturnedToForeground();
        await context.Sut.InitializeAsync();

        Assert.True(context.Sut.IsUnlockVisible);
        Assert.True(context.Sut.IsBiometricUnlockVisible);
        Assert.Empty(context.Sut.NotificationText);
        context.Authorization.Verify(value => value.TryUnlockWithHelloAsync(), Times.Once);
    }

    [Fact]
    public async Task BiometricUnlockAsync_WhenSuccessful_OpensEncryptedAccounts()
    {
        var account = new Account(Guid.NewGuid(), "Example", ValidSecret, "user@example.test");
        var context = CreateContext(
            isConfigured: true,
            accounts: [account],
            biometricAvailable: true,
            preferredUnlockMethod: TOTP.Core.Enums.PreferredUnlockMethod.PlatformQuickUnlock);
        context.Authorization
            .Setup(value => value.TryUnlockWithHelloAsync())
            .Callback(context.State.Unlock)
            .ReturnsAsync(AuthorizationResult.Success);
        context.AccountTotp
            .Setup(value => value.GenerateAsync(account.ID))
            .ReturnsAsync(Result.Ok(new TotpGenerationResult("123456", 20, 30)));
        await context.Sut.InitializeAsync();

        await context.Sut.BiometricUnlockAsync();

        Assert.True(context.Sut.IsAccountsVisible);
        Assert.Single(context.Sut.Accounts);
        context.Authorization.Verify(value => value.TryUnlockWithHelloAsync(), Times.Once);
    }

    [Fact]
    public async Task EnableBiometricAsync_RequiresRecoveryPasswordAndClearsIt()
    {
        var context = CreateContext(isConfigured: true, biometricAvailable: true);
        context.Authorization
            .Setup(value => value.TryUnlockWithPasswordAsync("synthetic password"))
            .Callback(context.State.Unlock)
            .ReturnsAsync(AuthorizationResult.Success);
        context.Authorization
            .Setup(value => value.ConfigureHelloAsync("recovery-password"))
            .Callback(() => context.State.SetConfiguration(
                true,
                TOTP.Core.Enums.PreferredUnlockMethod.PlatformQuickUnlock))
            .ReturnsAsync(AuthorizationResult.Success);
        await context.Sut.InitializeAsync();
        context.Sut.UnlockPassword = "synthetic password";
        await context.Sut.UnlockAsync();
        await context.Sut.ShowSettingsAsync();
        await context.Sut.BeginBiometricEnrollmentAsync();
        context.Sut.BiometricRecoveryPassword = "recovery-password";

        await context.Sut.EnableBiometricAsync();

        Assert.True(context.Sut.IsBiometricEnabled);
        Assert.False(context.Sut.IsBiometricEnrollmentVisible);
        Assert.Empty(context.Sut.BiometricRecoveryPassword);
        Assert.Equal(
            context.Strings.Get(MobileStringKeys.BiometricEnabled),
            context.Sut.NotificationText);
    }

    [Fact]
    public async Task ShowSettingsAsync_MovesBiometricEnrollmentOutOfAccountList()
    {
        var context = CreateContext(isConfigured: true, biometricAvailable: true);
        context.Authorization
            .Setup(value => value.TryUnlockWithPasswordAsync("synthetic password"))
            .ReturnsAsync(AuthorizationResult.Success);
        await context.Sut.InitializeAsync();
        context.Sut.UnlockPassword = "synthetic password";
        await context.Sut.UnlockAsync();

        Assert.True(context.Sut.IsAccountListVisible);
        Assert.False(context.Sut.IsBiometricSetupAvailable);

        await context.Sut.ShowSettingsAsync();

        Assert.True(context.Sut.IsSettingsVisible);
        Assert.False(context.Sut.IsAccountListVisible);
        Assert.True(context.Sut.IsBiometricSetupAvailable);
        Assert.True(context.Sut.ShowAccountsCommand.CanExecute(null));
        Assert.False(context.Sut.ShowSettingsCommand.CanExecute(null));
    }

    [Fact]
    public async Task ShowAccountsAsync_ClearsUnsubmittedSettingsPasswords()
    {
        var context = CreateContext(isConfigured: true, biometricAvailable: true);
        context.Authorization
            .Setup(value => value.TryUnlockWithPasswordAsync("synthetic password"))
            .Callback(context.State.Unlock)
            .ReturnsAsync(AuthorizationResult.Success);
        await context.Sut.InitializeAsync();
        context.Sut.UnlockPassword = "synthetic password";
        await context.Sut.UnlockAsync();
        await context.Sut.ShowSettingsAsync();
        await context.Sut.BeginBiometricEnrollmentAsync();
        context.Sut.BiometricRecoveryPassword = "recovery-password";
        context.Sut.BackupPassword = "backup-password";
        context.Sut.BackupPasswordConfirmation = "backup-password";
        context.Sut.ImportPassword = "backup-password";

        await context.Sut.ShowAccountsAsync();

        Assert.True(context.Sut.IsAccountListVisible);
        Assert.Empty(context.Sut.BiometricRecoveryPassword);
        Assert.Empty(context.Sut.BackupPassword);
        Assert.Empty(context.Sut.BackupPasswordConfirmation);
        Assert.Empty(context.Sut.ImportPassword);
    }

    [Fact]
    public async Task SearchText_FiltersByIssuerAndAccountNameWithoutChangingEmptyVaultState()
    {
        var work = new Account(Guid.NewGuid(), "Microsoft", ValidSecret, "work@example.test");
        var privateAccount = new Account(Guid.NewGuid(), "GitHub", ValidSecret, "private");
        var context = CreateContext(isConfigured: true, [work, privateAccount]);
        context.Authorization
            .Setup(value => value.TryUnlockWithPasswordAsync("synthetic password"))
            .ReturnsAsync(AuthorizationResult.Success);
        context.AccountTotp
            .Setup(value => value.GenerateAsync(It.IsAny<Guid>()))
            .ReturnsAsync(Result.Ok(new TotpGenerationResult("123456", 20, 30)));
        await context.Sut.InitializeAsync();
        context.Sut.UnlockPassword = "synthetic password";
        await context.Sut.UnlockAsync();

        context.Sut.SearchText = "PRIVATE";

        var match = Assert.Single(context.Sut.Accounts);
        Assert.Equal(privateAccount.ID, match.Id);
        Assert.False(context.Sut.HasNoAccounts);
        Assert.False(context.Sut.HasNoSearchResults);

        context.Sut.SearchText = "does-not-exist";

        Assert.Empty(context.Sut.Accounts);
        Assert.False(context.Sut.HasNoAccounts);
        Assert.True(context.Sut.HasNoSearchResults);
    }

    [Fact]
    public async Task ScanQrAsync_WhenScannerIsUnavailable_OffersManualFallback()
    {
        var context = CreateContext(isConfigured: true);
        context.Authorization
            .Setup(value => value.TryUnlockWithPasswordAsync("synthetic password"))
            .Callback(context.State.Unlock)
            .ReturnsAsync(AuthorizationResult.Success);
        context.QrScanner.Setup(value => value.ScanAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(MobileQrScanResult.Unavailable);
        await context.Sut.InitializeAsync();
        context.Sut.UnlockPassword = "synthetic password";
        await context.Sut.UnlockAsync();

        await context.Sut.ScanQrAsync();

        Assert.Equal(
            context.Strings.Get(MobileStringKeys.QrScannerUnavailable),
            context.Sut.NotificationText);
        context.QrImport.Verify(value => value.ImportAsync(
            It.IsAny<string>(),
            It.IsAny<Func<QrAccountConflict, CancellationToken, Task<QrAccountConflictDecision>>>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ScanQrAsync_WhenImportSucceeds_ShowsLocalizedResult()
    {
        const string payload =
            "otpauth://totp/Example:user?secret=JBSWY3DPEHPK3PXP&issuer=Example";
        var importedId = Guid.NewGuid();
        var context = CreateContext(isConfigured: true);
        context.Authorization
            .Setup(value => value.TryUnlockWithPasswordAsync("synthetic password"))
            .Callback(context.State.Unlock)
            .ReturnsAsync(AuthorizationResult.Success);
        context.QrScanner.Setup(value => value.ScanAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(MobileQrScanResult.Successful(payload));
        context.QrImport.Setup(value => value.ImportAsync(
                payload,
                It.IsAny<Func<QrAccountConflict, CancellationToken, Task<QrAccountConflictDecision>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(new QrAccountImportOutcome(
                QrAccountImportStatus.Added,
                importedId,
                "Example",
                "user")));
        await context.Sut.InitializeAsync();
        context.Sut.UnlockPassword = "synthetic password";
        await context.Sut.UnlockAsync();

        await context.Sut.ScanQrAsync();

        Assert.Equal(
            context.Strings.Get(MobileStringKeys.QrAccountAdded),
            context.Sut.NotificationText);
    }

    [Fact]
    public async Task ScanQrAsync_WhenAccountConflicts_UsesLocalizedExplicitDecision()
    {
        const string payload =
            "otpauth://totp/Example:user?secret=JBSWY3DPEHPK3PXP&issuer=Example";
        var importedId = Guid.NewGuid();
        var context = CreateContext(isConfigured: true, cultureName: "de");
        context.Authorization
            .Setup(value => value.TryUnlockWithPasswordAsync("synthetic password"))
            .Callback(context.State.Unlock)
            .ReturnsAsync(AuthorizationResult.Success);
        context.QrScanner.Setup(value => value.ScanAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(MobileQrScanResult.Successful(payload));
        var conflictRequested = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);
        context.QrImport.Setup(value => value.ImportAsync(
                payload,
                It.IsAny<Func<QrAccountConflict, CancellationToken, Task<QrAccountConflictDecision>>>(),
                It.IsAny<CancellationToken>()))
            .Returns(async (
                string _,
                Func<QrAccountConflict, CancellationToken, Task<QrAccountConflictDecision>> resolve,
                CancellationToken cancellationToken) =>
            {
                conflictRequested.TrySetResult();
                var decision = await resolve(
                    new QrAccountConflict("Example", "user"),
                    cancellationToken);
                Assert.Equal(QrAccountConflictDecision.UpdateExisting, decision);
                return Result.Ok(new QrAccountImportOutcome(
                    QrAccountImportStatus.Updated,
                    importedId,
                    "Example",
                    "user"));
            });
        await context.Sut.InitializeAsync();
        context.Sut.UnlockPassword = "synthetic password";
        await context.Sut.UnlockAsync();

        var scanTask = context.Sut.ScanQrAsync();
        await conflictRequested.Task.WaitAsync(
            TimeSpan.FromSeconds(2),
            global::Xunit.TestContext.Current.CancellationToken);

        Assert.True(context.Sut.IsQrConflictVisible);
        Assert.Equal(
            string.Format(
                context.Strings.Get(MobileStringKeys.QrConflictPrompt),
                "Example: user"),
            context.Sut.QrConflictPrompt);
        Assert.DoesNotContain("Choose how", context.Sut.QrConflictPrompt, StringComparison.Ordinal);

        await context.Sut.ResolveQrConflictAsync(QrAccountConflictDecision.UpdateExisting);
        await scanTask.WaitAsync(
            TimeSpan.FromSeconds(2),
            global::Xunit.TestContext.Current.CancellationToken);

        Assert.False(context.Sut.IsQrConflictVisible);
        Assert.Equal(
            context.Strings.Get(MobileStringKeys.QrAccountUpdated),
            context.Sut.NotificationText);
    }

    [Fact]
    public async Task LockAsync_DuringQrScan_CancelsScanAndClearsUnlockedState()
    {
        var account = new Account(Guid.NewGuid(), "Example", ValidSecret, "user");
        var context = CreateContext(isConfigured: true, [account]);
        context.Authorization
            .Setup(value => value.TryUnlockWithPasswordAsync("synthetic password"))
            .Callback(context.State.Unlock)
            .ReturnsAsync(AuthorizationResult.Success);
        context.AccountTotp
            .Setup(value => value.GenerateAsync(account.ID))
            .ReturnsAsync(Result.Ok(new TotpGenerationResult("123456", 20, 30)));
        var scanStarted = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);
        context.QrScanner
            .Setup(value => value.ScanAsync(It.IsAny<CancellationToken>()))
            .Returns(async (CancellationToken cancellationToken) =>
            {
                scanStarted.TrySetResult();
                await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
                return MobileQrScanResult.Cancelled;
            });
        await context.Sut.InitializeAsync();
        context.Sut.UnlockPassword = "synthetic password";
        await context.Sut.UnlockAsync();

        var scanTask = context.Sut.ScanQrAsync();
        await scanStarted.Task.WaitAsync(
            TimeSpan.FromSeconds(2),
            global::Xunit.TestContext.Current.CancellationToken);

        Assert.True(context.Sut.IsBusy);
        Assert.True(context.Sut.LockCommand.CanExecute(null));

        await context.Sut.LockAsync();
        await scanTask.WaitAsync(
            TimeSpan.FromSeconds(2),
            global::Xunit.TestContext.Current.CancellationToken);

        context.Authorization.Verify(value => value.Lock(), Times.Once);
        Assert.True(context.Sut.IsUnlockVisible);
        Assert.False(context.Sut.IsBusy);
        Assert.Empty(context.Sut.Accounts);
        Assert.Empty(context.Sut.SelectedCode);
        Assert.Null(context.Sut.SelectedAccount);
    }

    [Fact]
    public async Task ShowQrAsync_ClearsSensitivePngAndDisposesImageWhenBackgrounded()
    {
        var account = new Account(Guid.NewGuid(), "Example", ValidSecret, "user");
        var context = CreateContext(isConfigured: true, [account]);
        context.Authorization
            .Setup(value => value.TryUnlockWithPasswordAsync("synthetic password"))
            .Callback(context.State.Unlock)
            .ReturnsAsync(AuthorizationResult.Success);
        context.AccountTotp
            .Setup(value => value.GenerateAsync(account.ID))
            .ReturnsAsync(Result.Ok(new TotpGenerationResult("123456", 20, 30)));
        var sensitivePng = SensitiveBuffer.CopyFrom([1, 2, 3]);
        context.AccountQrCode.Setup(value => value.GenerateAsync(account.ID))
            .ReturnsAsync(Result.Ok(sensitivePng));
        var imageLifetime = new Mock<IDisposable>();
        context.QrImageFactory.Setup(value => value.Create(It.IsAny<ReadOnlyMemory<byte>>()))
            .Returns(new MobileQrImageHandle(Mock.Of<IImage>(), imageLifetime.Object));
        await context.Sut.InitializeAsync();
        context.Sut.UnlockPassword = "synthetic password";
        await context.Sut.UnlockAsync();

        await context.Sut.ShowQrAsync();

        Assert.True(context.Sut.HasQrImage);
        Assert.Throws<ObjectDisposedException>(() => _ = sensitivePng.Memory);

        context.Sut.OnEnteredBackground(lockImmediately: false);

        Assert.False(context.Sut.HasQrImage);
        imageLifetime.Verify(value => value.Dispose(), Times.Once);
    }

    [Fact]
    public async Task ExportBackupAsync_UsesOnlyEncryptedExportAndClearsPasswordInputs()
    {
        var account = new Account(Guid.NewGuid(), "Example", ValidSecret, "user");
        var context = CreateContext(isConfigured: true, [account]);
        context.Authorization
            .Setup(value => value.TryUnlockWithPasswordAsync("synthetic password"))
            .Callback(context.State.Unlock)
            .ReturnsAsync(AuthorizationResult.Success);
        context.Documents.Setup(value => value.CreateEncryptedBackupAsync(
                It.Is<string>(name => name.EndsWith(".totp", StringComparison.Ordinal)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MobileWritableDocument(
                new MemoryStream(),
                _ => Task.CompletedTask));
        context.ExportService.Setup(value => value.ExportToEncryptedStreamAsync(
                It.IsAny<IEnumerable<Account>>(),
                "backup-password",
                It.IsAny<Stream>(),
                ExportFileFormat.Json,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());
        await context.Sut.InitializeAsync();
        context.Sut.UnlockPassword = "synthetic password";
        await context.Sut.UnlockAsync();
        await context.Sut.ShowSettingsAsync();
        context.Sut.BackupPassword = "backup-password";
        context.Sut.BackupPasswordConfirmation = "backup-password";

        await context.Sut.ExportBackupAsync();

        Assert.Empty(context.Sut.BackupPassword);
        Assert.Empty(context.Sut.BackupPasswordConfirmation);
        Assert.Equal(
            context.Strings.Get(MobileStringKeys.BackupExported),
            context.Sut.NotificationText);
        context.ExportService.Verify(value => value.ExportToStreamAsync(
            It.IsAny<IEnumerable<Account>>(),
            It.IsAny<Stream>(),
            It.IsAny<ExportFileFormat>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExportBackupAsync_WhenEncryptionFails_DiscardsIncompleteDocument()
    {
        var account = new Account(Guid.NewGuid(), "Example", ValidSecret, "user");
        var context = CreateContext(isConfigured: true, [account]);
        context.Authorization
            .Setup(value => value.TryUnlockWithPasswordAsync("synthetic password"))
            .Callback(context.State.Unlock)
            .ReturnsAsync(AuthorizationResult.Success);
        var stream = new MemoryStream();
        var discarded = false;
        var streamWasClosedBeforeDiscard = false;
        context.Documents.Setup(value => value.CreateEncryptedBackupAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MobileWritableDocument(
                stream,
                _ =>
                {
                    discarded = true;
                    streamWasClosedBeforeDiscard = !stream.CanWrite;
                    return Task.CompletedTask;
                }));
        context.ExportService.Setup(value => value.ExportToEncryptedStreamAsync(
                It.IsAny<IEnumerable<Account>>(),
                "backup-password",
                It.IsAny<Stream>(),
                ExportFileFormat.Json,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail("synthetic encryption failure"));
        await context.Sut.InitializeAsync();
        context.Sut.UnlockPassword = "synthetic password";
        await context.Sut.UnlockAsync();
        await context.Sut.ShowSettingsAsync();
        context.Sut.BackupPassword = "backup-password";
        context.Sut.BackupPasswordConfirmation = "backup-password";

        await context.Sut.ExportBackupAsync();

        Assert.True(discarded);
        Assert.True(streamWasClosedBeforeDiscard);
        Assert.Equal(
            context.Strings.Get(MobileStringKeys.BackupExportFailed),
            context.Sut.NotificationText);
    }

    [Fact]
    public async Task ImportBackupAsync_RequiresExplicitConfirmationAndSkipsExistingAccounts()
    {
        var importedAccount = new Account(Guid.NewGuid(), "Example", ValidSecret, "user");
        var context = CreateContext(isConfigured: true, cultureName: "de");
        context.Authorization
            .Setup(value => value.TryUnlockWithPasswordAsync("synthetic password"))
            .Callback(context.State.Unlock)
            .ReturnsAsync(AuthorizationResult.Success);
        context.Documents.Setup(value => value.OpenEncryptedBackupAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MobileReadableDocument(new MemoryStream([1, 2, 3])));
        context.ExportService.Setup(value => value.ImportFromEncryptedStreamAsync(
                "backup-password",
                It.IsAny<Stream>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(new List<Account> { importedAccount }));
        context.AccountImport.Setup(value => value.ImportAsync(
                It.IsAny<IReadOnlyList<Account>>(),
                ImportConflictStrategy.SkipExisting,
                It.IsAny<Func<AccountImportPreview, CancellationToken, Task<bool>>>(),
                It.IsAny<CancellationToken>()))
            .Returns(async (
                IReadOnlyList<Account> _,
                ImportConflictStrategy _,
                Func<AccountImportPreview, CancellationToken, Task<bool>> confirm,
                CancellationToken cancellationToken) =>
            {
                var accepted = await confirm(
                    new AccountImportPreview(1, 1, ImportConflictStrategy.SkipExisting),
                    cancellationToken);
                return Result.Ok(accepted
                    ? new AccountImportOutcome(AccountImportStatus.Completed, Added: 0, Skipped: 1)
                    : new AccountImportOutcome(AccountImportStatus.Cancelled));
            });
        await context.Sut.InitializeAsync();
        context.Sut.UnlockPassword = "synthetic password";
        await context.Sut.UnlockAsync();
        await context.Sut.ShowSettingsAsync();
        context.Sut.ImportPassword = "backup-password";

        var importTask = context.Sut.ImportBackupAsync();
        for (var attempt = 0;
             attempt < 20 && !context.Sut.IsImportConfirmationVisible;
             attempt++)
        {
            await Task.Yield();
        }

        Assert.True(context.Sut.IsImportConfirmationVisible);
        Assert.Equal(
            string.Format(context.Strings.Get(MobileStringKeys.ImportConfirmation), 1, 1),
            context.Sut.ImportConfirmationText);

        await context.Sut.ResolveImportConfirmationAsync(true);
        await importTask;

        Assert.False(context.Sut.IsImportConfirmationVisible);
        Assert.Empty(context.Sut.ImportPassword);
        Assert.Equal(
            string.Format(context.Strings.Get(MobileStringKeys.BackupImported), 0, 1),
            context.Sut.NotificationText);
    }

    [Fact]
    public async Task SaveAccountAsync_WithInvalidSecret_DoesNotPersistAccount()
    {
        var context = CreateContext(isConfigured: false);
        context.Authorization
            .Setup(value => value.ConfigurePasswordAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(AuthorizationResult.Success);
        await ConfigureAndBeginAddAsync(context);
        context.Sut.EditorIssuer = "Example";
        context.Sut.EditorSecret = "not-valid-*";

        await context.Sut.SaveAccountAsync();

        context.AccountManager.Verify(value => value.AddNewAsync(It.IsAny<Account>()), Times.Never);
        Assert.Equal(
            context.Strings.Get(MobileStringKeys.SecretInvalid),
            context.Sut.NotificationText);
        Assert.Empty(context.Sut.EditorSecret);
    }

    [Fact]
    public async Task SaveAccountAsync_WithValidInput_PersistsOnlyNormalizedAccount()
    {
        var context = CreateContext(isConfigured: false);
        context.Authorization
            .Setup(value => value.ConfigurePasswordAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(AuthorizationResult.Success);
        Account? persisted = null;
        context.AccountManager
            .Setup(value => value.AddNewAsync(It.IsAny<Account>()))
            .Callback<Account>(account => persisted = account)
            .ReturnsAsync(Result.Ok());
        await ConfigureAndBeginAddAsync(context);
        context.Sut.EditorIssuer = "  Example  ";
        context.Sut.EditorAccountName = " user@example.test ";
        context.Sut.EditorSecret = "JBSW Y3DP EHPK 3PXP";

        await context.Sut.SaveAccountAsync();

        Assert.NotNull(persisted);
        Assert.Equal("Example", persisted.Issuer);
        Assert.Equal("user@example.test", persisted.AccountName);
        Assert.Equal(ValidSecret, persisted.Secret);
        Assert.Empty(context.Sut.EditorSecret);
        Assert.False(context.Sut.IsEditorVisible);
    }

    private static async Task ConfigureAndBeginAddAsync(TestContext context)
    {
        await context.Sut.InitializeAsync();
        context.Sut.SetupPassword = "synthetic password";
        context.Sut.SetupConfirmation = "synthetic password";
        await context.Sut.ConfigureAsync();
        await context.Sut.BeginAddAsync();
    }

    private static TestContext CreateContext(
        bool isConfigured,
        IReadOnlyList<Account>? accounts = null,
        bool biometricAvailable = false,
        TOTP.Core.Enums.PreferredUnlockMethod preferredUnlockMethod =
            TOTP.Core.Enums.PreferredUnlockMethod.Password,
        string cultureName = "en")
    {
        accounts ??= [];
        var state = new AuthorizationState();
        state.SetConfiguration(isConfigured, preferredUnlockMethod);
        var authorization = new Mock<IAuthorizationService>();
        authorization.SetupGet(value => value.State).Returns(state);
        authorization.Setup(value => value.InitializeAsync()).Returns(Task.CompletedTask);
        authorization.Setup(value => value.IsHelloAvailableAsync())
            .ReturnsAsync(biometricAvailable);

        var passwordValidation = new Mock<IPasswordValidationService>();
        passwordValidation.SetupGet(value => value.MinimumLength).Returns(8);

        var accountManager = new Mock<IAccountManager>();
        accountManager.Setup(value => value.GetAllOtpEntriesSortedAsync())
            .ReturnsAsync(() => Result.Ok(accounts));

        var accountTotp = new Mock<IAccountTotpService>();
        var clipboard = new Mock<IAsyncClipboardService>();
        clipboard.SetupGet(value => value.Capabilities)
            .Returns(ClipboardCapabilities.WriteText | ClipboardCapabilities.ConditionalClear);
        var qrScanner = new Mock<IMobileQrScanner>();
        var qrImport = new Mock<IQrAccountImportService>();
        var accountQrCode = new Mock<IAccountQrCodeService>();
        var qrImageFactory = new Mock<IMobileQrImageFactory>();
        var documents = new Mock<IMobileDocumentService>();
        var exportService = new Mock<IExportService>();
        var accountImport = new Mock<IAccountImportService>();

        var settingsValue = new AppSettings();
        var settings = new Mock<ISettingsService>();
        settings.SetupGet(value => value.Current).Returns(settingsValue);
        settings.Setup(value => value.LoadAsync())
            .ReturnsAsync(Result.Ok<IAppSettings>(settingsValue));

        var paths = new Mock<IPlatformApplicationPaths>();
        paths.SetupGet(value => value.AuthorizationEnvelopeFilePath)
            .Returns(Path.Combine(Path.GetTempPath(), $"missing-{Guid.NewGuid():N}.bin"));

        var strings = new MobileStringCatalog(CultureInfo.GetCultureInfo(cultureName));
        var time = new ManualTimeProvider();
        var sut = new MobileShellViewModel(
            authorization.Object,
            passwordValidation.Object,
            accountManager.Object,
            accountTotp.Object,
            clipboard.Object,
            qrScanner.Object,
            qrImport.Object,
            accountQrCode.Object,
            qrImageFactory.Object,
            documents.Object,
            exportService.Object,
            accountImport.Object,
            settings.Object,
            paths.Object,
            strings,
            time);
        return new TestContext(
            sut,
            state,
            authorization,
            accountManager,
            accountTotp,
            qrScanner,
            qrImport,
            accountQrCode,
            qrImageFactory,
            documents,
            exportService,
            accountImport,
            strings,
            time);
    }

    private sealed record TestContext(
        MobileShellViewModel Sut,
        AuthorizationState State,
        Mock<IAuthorizationService> Authorization,
        Mock<IAccountManager> AccountManager,
        Mock<IAccountTotpService> AccountTotp,
        Mock<IMobileQrScanner> QrScanner,
        Mock<IQrAccountImportService> QrImport,
        Mock<IAccountQrCodeService> AccountQrCode,
        Mock<IMobileQrImageFactory> QrImageFactory,
        Mock<IMobileDocumentService> Documents,
        Mock<IExportService> ExportService,
        Mock<IAccountImportService> AccountImport,
        MobileStringCatalog Strings,
        ManualTimeProvider Time);

    private sealed class ManualTimeProvider : TimeProvider
    {
        private long _timestamp;

        public override long GetTimestamp() => _timestamp;
        public override long TimestampFrequency => TimeSpan.TicksPerSecond;

        public void Advance(TimeSpan duration) => _timestamp += duration.Ticks;
    }
}
