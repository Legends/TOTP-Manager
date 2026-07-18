using FluentResults;
using Moq;
using TOTP.Core.Models;
using TOTP.Core.Services.Interfaces;
using TOTP.Core.Services.Models;
using TOTP.Infrastructure.Services;

namespace TOTP.Tests.Infrastructure.Services;

public sealed class QrAccountImportServiceTests
{
    private const string Payload =
        "otpauth://totp/Example:alice?secret=JBSWY3DPEHPK3PXP&issuer=Example";

    [Fact]
    public async Task ImportAsync_WhenIdentityIsNew_AddsNormalizedAccountWithoutConflictPrompt()
    {
        var accounts = Manager([]);
        Account? added = null;
        accounts.Setup(value => value.AddNewAsync(It.IsAny<Account>()))
            .ReturnsAsync((Account account) =>
            {
                added = account;
                return Result.Ok();
            });
        var resolver = new Mock<
            Func<QrAccountConflict, CancellationToken, Task<QrAccountConflictDecision>>>();
        var sut = new QrAccountImportService(accounts.Object);

        var result = await sut.ImportAsync(
            Payload,
            resolver.Object,
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(QrAccountImportStatus.Added, result.Value.Status);
        Assert.Equal("Example", added!.Issuer);
        Assert.Equal("alice", added.AccountName);
        Assert.Equal("JBSWY3DPEHPK3PXP", added.Secret);
        resolver.Verify(value => value(It.IsAny<QrAccountConflict>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ImportAsync_WhenExactAccountExists_DoesNotWriteOrPrompt()
    {
        var existing = Existing("JBSWY3DPEHPK3PXP");
        var accounts = Manager([existing]);
        var resolver = new Mock<
            Func<QrAccountConflict, CancellationToken, Task<QrAccountConflictDecision>>>();
        var sut = new QrAccountImportService(accounts.Object);

        var result = await sut.ImportAsync(
            Payload,
            resolver.Object,
            TestContext.Current.CancellationToken);

        Assert.Equal(QrAccountImportStatus.DuplicateUnchanged, result.Value.Status);
        accounts.Verify(value => value.AddNewAsync(It.IsAny<Account>()), Times.Never);
        accounts.Verify(value => value.UpdateAsync(It.IsAny<Account>(), It.IsAny<Account>()), Times.Never);
        resolver.Verify(value => value(It.IsAny<QrAccountConflict>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(QrAccountConflictDecision.UpdateExisting, QrAccountImportStatus.Updated)]
    [InlineData(QrAccountConflictDecision.KeepBoth, QrAccountImportStatus.KeptBoth)]
    [InlineData(QrAccountConflictDecision.Cancel, QrAccountImportStatus.Cancelled)]
    public async Task ImportAsync_WhenIdentityConflicts_AppliesExplicitDecision(
        QrAccountConflictDecision decision,
        QrAccountImportStatus expectedStatus)
    {
        var existing = Existing("KRUGS4ZANFZSAYJA");
        var accounts = Manager([existing]);
        accounts.Setup(value => value.UpdateAsync(existing, It.IsAny<Account>()))
            .ReturnsAsync(Result.Ok());
        accounts.Setup(value => value.AddNewAsync(It.IsAny<Account>()))
            .ReturnsAsync(Result.Ok());
        var sut = new QrAccountImportService(accounts.Object);

        var result = await sut.ImportAsync(
            Payload,
            (conflict, _) =>
            {
                Assert.Equal("Example", conflict.Issuer);
                Assert.Equal("alice", conflict.AccountName);
                return Task.FromResult(decision);
            },
            TestContext.Current.CancellationToken);

        Assert.Equal(expectedStatus, result.Value.Status);
        accounts.Verify(
            value => value.UpdateAsync(existing, It.Is<Account>(account => account.ID == existing.ID)),
            decision == QrAccountConflictDecision.UpdateExisting ? Times.Once : Times.Never);
        accounts.Verify(
            value => value.AddNewAsync(It.IsAny<Account>()),
            decision == QrAccountConflictDecision.KeepBoth ? Times.Once : Times.Never);
    }

    [Fact]
    public async Task ImportAsync_WhenPayloadIsInvalid_FailsBeforeAccountAccess()
    {
        var accounts = new Mock<IAccountManager>();
        var sut = new QrAccountImportService(accounts.Object);

        var result = await sut.ImportAsync(
            "otpauth://totp/Example:alice?secret=NOT-BASE32",
            (_, _) => Task.FromResult(QrAccountConflictDecision.Cancel),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsFailed);
        accounts.Verify(value => value.GetAllOtpEntriesSortedAsync(), Times.Never);
    }

    [Fact]
    public async Task ImportAsync_WhenParametersAreUnsupported_FailsBeforeAccountAccess()
    {
        var accounts = new Mock<IAccountManager>();
        var sut = new QrAccountImportService(accounts.Object);

        var result = await sut.ImportAsync(
            Payload + "&digits=8",
            (_, _) => Task.FromResult(QrAccountConflictDecision.Cancel),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsFailed);
        accounts.Verify(value => value.GetAllOtpEntriesSortedAsync(), Times.Never);
    }

    private static Mock<IAccountManager> Manager(IReadOnlyList<Account> existing)
    {
        var accounts = new Mock<IAccountManager>();
        accounts.Setup(value => value.GetAllOtpEntriesSortedAsync())
            .ReturnsAsync(Result.Ok(existing));
        return accounts;
    }

    private static Account Existing(string secret) =>
        new(Guid.NewGuid(), "Example", secret, "alice");
}
