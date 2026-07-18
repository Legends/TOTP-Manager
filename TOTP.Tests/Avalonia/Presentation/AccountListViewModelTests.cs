using FluentResults;
using System.Diagnostics;
using Moq;
using Avalonia.Media;
using TOTP.Core.Models;
using TOTP.Core.Security.Models;
using TOTP.Core.Security.Interfaces;
using TOTP.Core.Services.Interfaces;
using TOTP.Avalonia.Desktop.Platform;
using TOTP.Avalonia.Desktop.Presentation;
using TOTP.Avalonia.Desktop.Presentation.Dialogs;
using TOTP.Avalonia.Desktop.Localization;

namespace TOTP.Tests.Avalonia.Presentation;

public sealed class AccountListViewModelTests
{
    private const string ValidSecret = "JBSWY3DPEHPK3PXP";
    [Fact]
    public async Task LoadAsync_WithFiveHundredSyntheticAccounts_ProjectsSecretFreeRows()
    {
        var accounts = Enumerable.Range(1, 500)
            .Select(index => new Account(
                Guid.NewGuid(),
                $"Issuer {index:D3}",
                $"SYNTHETIC-SECRET-{index:D3}",
                $"user{index:D3}@example.test"))
            .ToArray();
        var manager = new Mock<IAccountManager>();
        manager.Setup(value => value.GetAllOtpEntriesSortedAsync())
            .ReturnsAsync(Result.Ok<IReadOnlyList<Account>>(accounts));
        var sut = CreateSut(manager.Object);

        await sut.LoadAsync();

        Assert.Equal(500, sut.Accounts.Count);
        Assert.Equal("Issuer 001", sut.Accounts[0].Issuer);
        Assert.Equal("user500@example.test", sut.Accounts[^1].AccountName);
        Assert.DoesNotContain(
            typeof(AccountListItemViewModel).GetProperties(),
            property => string.Equals(property.Name, "Secret", StringComparison.Ordinal));
        Assert.False(sut.HasMessage);
    }

    [Fact]
    public async Task TenThousandAccountProjectionAndFiltering_RemainsWithinDesktopBudget()
    {
        var accounts = Enumerable.Range(1, 10_000)
            .Select(index => new Account(
                Guid.NewGuid(),
                $"Issuer {index:D5}",
                ValidSecret,
                $"user{index:D5}@example.test"))
            .ToArray();
        var manager = new Mock<IAccountManager>();
        manager.Setup(value => value.GetAllOtpEntriesSortedAsync())
            .ReturnsAsync(Result.Ok<IReadOnlyList<Account>>(accounts));
        var sut = CreateSut(manager.Object);
        var stopwatch = Stopwatch.StartNew();

        await sut.LoadAsync();
        sut.SearchText = "user09999";
        stopwatch.Stop();

        Assert.Single(sut.Accounts);
        Assert.Equal("user09999@example.test", sut.Accounts[0].AccountName);
        Assert.True(
            stopwatch.Elapsed < TimeSpan.FromSeconds(5),
            $"Projection and filtering took {stopwatch.Elapsed}.");
    }

    [Fact]
    public async Task LoadAsync_WhenManagerFails_ClearsRowsAndShowsRecoverableMessage()
    {
        var manager = new Mock<IAccountManager>();
        manager.Setup(value => value.GetAllOtpEntriesSortedAsync())
            .ReturnsAsync(Result.Fail<IReadOnlyList<Account>>("synthetic failure"));
        var sut = CreateSut(manager.Object);

        await sut.LoadAsync();

        Assert.Empty(sut.Accounts);
        Assert.Contains("not changed", sut.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SearchText_FiltersIssuerAndAccountNameCaseInsensitively()
    {
        IReadOnlyList<Account> accounts =
        [
            new(Guid.NewGuid(), "GitHub", "SECRET-A", "alice@example.test"),
            new(Guid.NewGuid(), "Microsoft", "SECRET-B", "bob@example.test"),
            new(Guid.NewGuid(), "Example", "SECRET-C", "github-user@example.test")
        ];
        var manager = new Mock<IAccountManager>();
        manager.Setup(value => value.GetAllOtpEntriesSortedAsync())
            .ReturnsAsync(Result.Ok(accounts));
        var sut = CreateSut(manager.Object);
        await sut.LoadAsync();

        sut.SearchText = "GITHUB";

        Assert.Equal(2, sut.Accounts.Count);
        Assert.Contains(sut.Accounts, account => account.Issuer == "GitHub");
        Assert.Contains(sut.Accounts, account => account.AccountName == "github-user@example.test");

        sut.SearchText = "  bob  ";
        Assert.Single(sut.Accounts);
        Assert.Equal("Microsoft", sut.Accounts[0].Issuer);

        sut.SearchText = string.Empty;
        Assert.Equal(3, sut.Accounts.Count);
    }

    [Fact]
    public async Task LoadAsync_WhenBoundaryThrows_DoesNotExposeExceptionText()
    {
        var manager = new Mock<IAccountManager>();
        manager.Setup(value => value.GetAllOtpEntriesSortedAsync())
            .ThrowsAsync(new InvalidOperationException("sensitive synthetic detail"));
        var sut = CreateSut(manager.Object);

        await sut.LoadAsync();

        Assert.Empty(sut.Accounts);
        Assert.DoesNotContain("sensitive", sut.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GenerateCodeAsync_UsesSelectedIdAndProjectsExpiringCode()
    {
        var accountId = Guid.NewGuid();
        var totp = new Mock<IAccountTotpService>();
        totp.Setup(value => value.GenerateAsync(accountId))
            .ReturnsAsync(Result.Ok(new TotpGenerationResult("123456", 30, 30)));
        using var sut = new AccountListViewModel(
            Mock.Of<IAccountManager>(),
            totp.Object,
            Mock.Of<IAsyncClipboardService>(),
            Mock.Of<IAccountQrCodeService>(),
            Mock.Of<IAvaloniaQrImageFactory>(),
            Mock.Of<IAvaloniaDialogService>(),
            Localization())
        {
            SelectedAccount = new AccountListItemViewModel(accountId, "Issuer", "account")
        };

        await sut.GenerateCodeAsync();

        Assert.Equal("123456", sut.GeneratedCode);
        Assert.Equal(AvaloniaStringKeys.CodeAutoRefreshReady, sut.CodeMessage);
        totp.Verify(value => value.GenerateAsync(accountId), Times.Once);
    }

    [Fact]
    public async Task GenerateCodeAsync_WhenServiceFails_DoesNotExposeFailureDetail()
    {
        var totp = new Mock<IAccountTotpService>();
        totp.Setup(value => value.GenerateAsync(It.IsAny<Guid>()))
            .ReturnsAsync(Result.Fail<TotpGenerationResult>("SYNTHETIC-SECRET-DETAIL"));
        using var sut = new AccountListViewModel(
            Mock.Of<IAccountManager>(),
            totp.Object,
            Mock.Of<IAsyncClipboardService>(),
            Mock.Of<IAccountQrCodeService>(),
            Mock.Of<IAvaloniaQrImageFactory>(),
            Mock.Of<IAvaloniaDialogService>(),
            Localization())
        {
            SelectedAccount = new AccountListItemViewModel(Guid.NewGuid(), "Issuer", "account")
        };

        await sut.GenerateCodeAsync();

        Assert.Empty(sut.GeneratedCode);
        Assert.DoesNotContain("SECRET", sut.CodeMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Clear_RemovesRowsSelectionSearchAndGeneratedCode()
    {
        var id = Guid.NewGuid();
        var manager = new Mock<IAccountManager>();
        manager.Setup(value => value.GetAllOtpEntriesSortedAsync())
            .ReturnsAsync(Result.Ok<IReadOnlyList<Account>>(
                [new Account(id, "Issuer", "SECRET", "account")]));
        var totp = new Mock<IAccountTotpService>();
        totp.Setup(value => value.GenerateAsync(id))
            .ReturnsAsync(Result.Ok(new TotpGenerationResult("123456", 30, 30)));
        using var sut = new AccountListViewModel(
            manager.Object,
            totp.Object,
            Mock.Of<IAsyncClipboardService>(),
            Mock.Of<IAccountQrCodeService>(),
            Mock.Of<IAvaloniaQrImageFactory>(),
            Mock.Of<IAvaloniaDialogService>(),
            Localization());
        await sut.LoadAsync();
        sut.SelectedAccount = sut.Accounts[0];
        sut.SearchText = "Issuer";
        await sut.GenerateCodeAsync();

        sut.Clear();

        Assert.Empty(sut.Accounts);
        Assert.Null(sut.SelectedAccount);
        Assert.Empty(sut.SearchText);
        Assert.Empty(sut.GeneratedCode);
        Assert.Empty(sut.CodeMessage);
    }

    [Fact]
    public async Task ClearSensitiveOutput_PreservesRowsAndSelectionButRemovesCode()
    {
        var id = Guid.NewGuid();
        var manager = new Mock<IAccountManager>();
        manager.Setup(value => value.GetAllOtpEntriesSortedAsync())
            .ReturnsAsync(Result.Ok<IReadOnlyList<Account>>(
                [new Account(id, "Issuer", "SECRET", "account")]));
        var totp = new Mock<IAccountTotpService>();
        totp.Setup(value => value.GenerateAsync(id))
            .ReturnsAsync(Result.Ok(new TotpGenerationResult("123456", 30, 30)));
        using var sut = new AccountListViewModel(
            manager.Object,
            totp.Object,
            Mock.Of<IAsyncClipboardService>(),
            Mock.Of<IAccountQrCodeService>(),
            Mock.Of<IAvaloniaQrImageFactory>(),
            Mock.Of<IAvaloniaDialogService>(),
            Localization());
        await sut.LoadAsync();
        sut.SelectedAccount = sut.Accounts[0];
        await sut.GenerateCodeAsync();

        sut.ClearSensitiveOutput();

        Assert.Single(sut.Accounts);
        Assert.NotNull(sut.SelectedAccount);
        Assert.Empty(sut.GeneratedCode);
        Assert.Empty(sut.CodeMessage);
    }

    [Fact]
    public async Task CopyCodeAsync_UsesRemainingLifetimeForConditionalClear()
    {
        var id = Guid.NewGuid();
        var totp = new Mock<IAccountTotpService>();
        totp.Setup(value => value.GenerateAsync(id))
            .ReturnsAsync(Result.Ok(new TotpGenerationResult("123456", 18, 30)));
        var clipboard = new Mock<IAsyncClipboardService>();
        clipboard.Setup(value => value.CopyAndScheduleClearAsync(
                "123456",
                TimeSpan.FromSeconds(18),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());
        using var sut = new AccountListViewModel(
            Mock.Of<IAccountManager>(),
            totp.Object,
            clipboard.Object,
            Mock.Of<IAccountQrCodeService>(),
            Mock.Of<IAvaloniaQrImageFactory>(),
            Mock.Of<IAvaloniaDialogService>(),
            Localization())
        {
            SelectedAccount = new AccountListItemViewModel(id, "Issuer", "account")
        };
        await sut.GenerateCodeAsync();

        await sut.CopyCodeAsync();

        clipboard.Verify(value => value.CopyAndScheduleClearAsync(
            "123456",
            TimeSpan.FromSeconds(18),
            It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal(AvaloniaStringKeys.CodeCopiedWithClear, sut.CodeMessage);
    }

    [Fact]
    public async Task GenerateQrAsync_ProjectsAndClearsSecretBearingBitmap()
    {
        var id = Guid.NewGuid();
        var png = new byte[] { 137, 80, 78, 71 };
        var image = new Mock<IImage>();
        var lifetime = new Mock<IDisposable>();
        var imageFactory = new Mock<IAvaloniaQrImageFactory>();
        imageFactory.Setup(value => value.Create(It.IsAny<ReadOnlyMemory<byte>>()))
            .Returns(new AvaloniaQrImageHandle(image.Object, lifetime.Object));
        var qr = new Mock<IAccountQrCodeService>();
        qr.Setup(value => value.GenerateAsync(id))
            .ReturnsAsync(() => Result.Ok(SensitiveBuffer.CopyFrom(png)));
        using var sut = new AccountListViewModel(
            Mock.Of<IAccountManager>(),
            Mock.Of<IAccountTotpService>(),
            Mock.Of<IAsyncClipboardService>(),
            qr.Object,
            imageFactory.Object,
            Mock.Of<IAvaloniaDialogService>(),
            Localization())
        {
            SelectedAccount = new AccountListItemViewModel(id, "Issuer", "account")
        };

        await sut.GenerateQrAsync();

        Assert.True(sut.HasQrImage);
        sut.Clear();
        Assert.False(sut.HasQrImage);
        lifetime.Verify(value => value.Dispose(), Times.Once);
    }

    [Fact]
    public async Task SaveAccountAsync_CreatesNormalizedAccountAfterClearingBoundSecret()
    {
        var manager = new Mock<IAccountManager>();
        Account? created = null;
        manager.SetupSequence(value => value.GetAllOtpEntriesSortedAsync())
            .ReturnsAsync(Result.Ok<IReadOnlyList<Account>>([]))
            .ReturnsAsync(() => Result.Ok<IReadOnlyList<Account>>(
                created is null ? [] : [created]));
        AccountListViewModel? sut = null;
        manager.Setup(value => value.AddNewAsync(It.IsAny<Account>()))
            .ReturnsAsync((Account account) =>
            {
                Assert.Equal(string.Empty, sut!.EditorSecret);
                created = account;
                return Result.Ok();
            });
        sut = CreateSut(manager.Object);
        await sut.BeginAddAsync();
        sut.EditorIssuer = "  GitHub  ";
        sut.EditorAccountName = " alice@example.test ";
        sut.EditorSecret = "JBSW Y3DP-EHPK3PXP";

        await sut.SaveAccountAsync();

        Assert.NotNull(created);
        Assert.Equal("GitHub", created.Issuer);
        Assert.Equal("alice@example.test", created.AccountName);
        Assert.Equal(ValidSecret, created.Secret);
        Assert.False(sut.IsEditorVisible);
        Assert.Equal(AvaloniaStringKeys.AccountSaved, sut.Message);
    }

    [Fact]
    public async Task SaveAccountAsync_RejectsDuplicateIssuerAndAccountWithoutWriting()
    {
        var manager = new Mock<IAccountManager>();
        manager.Setup(value => value.GetAllOtpEntriesSortedAsync())
            .ReturnsAsync(Result.Ok<IReadOnlyList<Account>>(
                [new Account(Guid.NewGuid(), "GitHub", ValidSecret, "alice@example.test")]));
        var sut = CreateSut(manager.Object);
        await sut.BeginAddAsync();
        sut.EditorIssuer = "github";
        sut.EditorAccountName = "ALICE@example.test";
        sut.EditorSecret = ValidSecret;

        await sut.SaveAccountAsync();

        manager.Verify(value => value.AddNewAsync(It.IsAny<Account>()), Times.Never);
        Assert.Equal(AvaloniaStringKeys.AccountDuplicate, sut.EditorMessage);
        Assert.Equal(string.Empty, sut.EditorSecret);
    }

    [Fact]
    public async Task BeginEditAndSave_UsesSelectedIdentityAndClearsSecretAtBoundaries()
    {
        var id = Guid.NewGuid();
        var original = new Account(id, "GitHub", ValidSecret, "alice@example.test");
        var manager = new Mock<IAccountManager>();
        manager.Setup(value => value.GetAllOtpEntriesSortedAsync())
            .ReturnsAsync(Result.Ok<IReadOnlyList<Account>>([original]));
        AccountListViewModel? sut = null;
        manager.Setup(value => value.UpdateAsync(original, It.IsAny<Account>()))
            .ReturnsAsync((Account _, Account updated) =>
            {
                Assert.Equal(string.Empty, sut!.EditorSecret);
                Assert.Equal(id, updated.ID);
                return Result.Ok();
            });
        sut = CreateSut(manager.Object);
        await sut.LoadAsync();
        sut.SelectedAccount = sut.Accounts[0];

        await sut.BeginEditAsync();
        Assert.Equal(ValidSecret, sut.EditorSecret);
        sut.EditorIssuer = "GitHub Enterprise";
        await sut.SaveAccountAsync();

        manager.Verify(value => value.UpdateAsync(
            original,
            It.Is<Account>(account => account.Issuer == "GitHub Enterprise")), Times.Once);
        Assert.False(sut.IsEditorVisible);
        Assert.Equal(string.Empty, sut.EditorSecret);
    }

    [Fact]
    public async Task DeleteAccountAsync_DeletesOnlyAfterOwnedConfirmation()
    {
        var id = Guid.NewGuid();
        var account = new Account(id, "GitHub", ValidSecret, "alice@example.test");
        var manager = new Mock<IAccountManager>();
        manager.SetupSequence(value => value.GetAllOtpEntriesSortedAsync())
            .ReturnsAsync(Result.Ok<IReadOnlyList<Account>>([account]))
            .ReturnsAsync(Result.Ok<IReadOnlyList<Account>>([account]))
            .ReturnsAsync(Result.Ok<IReadOnlyList<Account>>([]));
        manager.Setup(value => value.DeleteAsync(account)).ReturnsAsync(Result.Ok());
        var dialogs = new Mock<IAvaloniaDialogService>();
        dialogs.Setup(value => value.ConfirmAsync(
                It.IsAny<ConfirmationDialogRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var sut = CreateSut(manager.Object, dialogs.Object);
        await sut.LoadAsync();
        sut.SelectedAccount = sut.Accounts[0];

        await sut.DeleteAccountAsync();

        manager.Verify(value => value.DeleteAsync(account), Times.Once);
        Assert.Empty(sut.Accounts);
        Assert.Equal(AvaloniaStringKeys.AccountDeleted, sut.Message);
    }

    [Fact]
    public async Task ClearSensitiveOutput_DiscardsUncommittedEditorSecret()
    {
        var sut = CreateSut(Mock.Of<IAccountManager>());
        await sut.BeginAddAsync();
        sut.EditorSecret = ValidSecret;

        sut.ClearSensitiveOutput();

        Assert.False(sut.IsEditorVisible);
        Assert.Equal(string.Empty, sut.EditorSecret);
    }

    [Fact]
    public async Task CountdownAsync_RefreshesCodeAtPeriodBoundary()
    {
        var id = Guid.NewGuid();
        var totp = new Mock<IAccountTotpService>();
        totp.SetupSequence(value => value.GenerateAsync(id))
            .ReturnsAsync(Result.Ok(new TotpGenerationResult("111111", 1, 30)))
            .ReturnsAsync(Result.Ok(new TotpGenerationResult("222222", 30, 30)));
        using var sut = new AccountListViewModel(
            Mock.Of<IAccountManager>(),
            totp.Object,
            Mock.Of<IAsyncClipboardService>(),
            Mock.Of<IAccountQrCodeService>(),
            Mock.Of<IAvaloniaQrImageFactory>(),
            Mock.Of<IAvaloniaDialogService>(),
            Localization(),
            TimeSpan.FromMilliseconds(10))
        {
            SelectedAccount = new AccountListItemViewModel(id, "Issuer", "account")
        };

        await sut.GenerateCodeAsync();
        await WaitUntilAsync(() => sut.GeneratedCode == "222222");

        Assert.InRange(sut.RemainingSeconds, 1, 30);
        Assert.Equal(30, sut.PeriodSeconds);
        Assert.Equal(AvaloniaStringKeys.CodeRefreshed, sut.CodeMessage);
        totp.Verify(value => value.GenerateAsync(id), Times.Exactly(2));
    }

    [Fact]
    public async Task CopyCodeAsync_WhenClipboardPolicyIsDisabled_DoesNotCopy()
    {
        var id = Guid.NewGuid();
        var totp = new Mock<IAccountTotpService>();
        totp.Setup(value => value.GenerateAsync(id))
            .ReturnsAsync(Result.Ok(new TotpGenerationResult("123456", 30, 30)));
        var clipboard = new Mock<IAsyncClipboardService>();
        var settings = new Mock<ISettingsService>();
        settings.SetupGet(value => value.Current).Returns(new AppSettings
        {
            ClearClipboardEnabled = false
        });
        using var sut = new AccountListViewModel(
            Mock.Of<IAccountManager>(),
            totp.Object,
            clipboard.Object,
            Mock.Of<IAccountQrCodeService>(),
            Mock.Of<IAvaloniaQrImageFactory>(),
            Mock.Of<IAvaloniaDialogService>(),
            Localization(),
            settingsService: settings.Object)
        {
            SelectedAccount = new AccountListItemViewModel(id, "Issuer", "account")
        };
        await sut.GenerateCodeAsync();

        await sut.CopyCodeAsync();

        clipboard.Verify(value => value.CopyAndScheduleClearAsync(
            It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Never);
        Assert.Equal(AvaloniaStringKeys.ClipboardCopyDisabled, sut.CodeMessage);
    }

    private static AccountListViewModel CreateSut(
        IAccountManager manager,
        IAvaloniaDialogService? dialogs = null) =>
        new(
            manager,
            Mock.Of<IAccountTotpService>(),
            Mock.Of<IAsyncClipboardService>(),
            Mock.Of<IAccountQrCodeService>(),
            Mock.Of<IAvaloniaQrImageFactory>(),
            dialogs ?? Mock.Of<IAvaloniaDialogService>(),
            Localization());

    private static IAvaloniaLocalizationService Localization()
    {
        var localization = new Mock<IAvaloniaLocalizationService>();
        localization.Setup(value => value.GetString(It.IsAny<string>()))
            .Returns((string key) => key);
        return localization.Object;
    }

    private static async Task WaitUntilAsync(Func<bool> predicate)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        while (!predicate())
            await Task.Delay(10, timeout.Token);
    }
}
