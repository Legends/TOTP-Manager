using FluentResults;
using Moq;
using TOTP.Core.Models;
using TOTP.Core.Services.Interfaces;
using TOTP.Avalonia.Desktop.Presentation;

namespace TOTP.Tests.Avalonia.Presentation;

public sealed class AccountListViewModelTests
{
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
        using var sut = new AccountListViewModel(Mock.Of<IAccountManager>(), totp.Object)
        {
            SelectedAccount = new AccountListItemViewModel(accountId, "Issuer", "account")
        };

        await sut.GenerateCodeAsync();

        Assert.Equal("123456", sut.GeneratedCode);
        Assert.Contains("30 seconds", sut.CodeMessage, StringComparison.Ordinal);
        totp.Verify(value => value.GenerateAsync(accountId), Times.Once);
    }

    [Fact]
    public async Task GenerateCodeAsync_WhenServiceFails_DoesNotExposeFailureDetail()
    {
        var totp = new Mock<IAccountTotpService>();
        totp.Setup(value => value.GenerateAsync(It.IsAny<Guid>()))
            .ReturnsAsync(Result.Fail<TotpGenerationResult>("SYNTHETIC-SECRET-DETAIL"));
        using var sut = new AccountListViewModel(Mock.Of<IAccountManager>(), totp.Object)
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
        using var sut = new AccountListViewModel(manager.Object, totp.Object);
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

    private static AccountListViewModel CreateSut(IAccountManager manager) =>
        new(manager, Mock.Of<IAccountTotpService>());
}
