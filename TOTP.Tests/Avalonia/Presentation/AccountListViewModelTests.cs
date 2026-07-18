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
        var sut = new AccountListViewModel(manager.Object);

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
        var sut = new AccountListViewModel(manager.Object);

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
        var sut = new AccountListViewModel(manager.Object);
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
        var sut = new AccountListViewModel(manager.Object);

        await sut.LoadAsync();

        Assert.Empty(sut.Accounts);
        Assert.DoesNotContain("sensitive", sut.Message, StringComparison.OrdinalIgnoreCase);
    }
}
