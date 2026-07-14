using FluentResults;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Collections.ObjectModel;
using TOTP.Core.Enums;
using TOTP.Services;
using TOTP.Services.Interfaces;
using TOTP.ViewModels;

namespace TOTP.Tests.Services;

public sealed class QrAccountImportWorkflowTests
{
    private const string ValidUri =
        "otpauth://totp/GitHub:alice?secret=JBSWY3DPEHPK3PXP&issuer=GitHub";

    [Fact]
    public async Task ImportAsync_NewAccount_PersistsAndAddsToCollection()
    {
        var accountsWorkflow = new Mock<IAccountsWorkflowService>();
        accountsWorkflow
            .Setup(x => x.ValidateForCreate(It.IsAny<OtpViewModel>(), It.IsAny<IEnumerable<OtpViewModel>>()))
            .Returns([]);
        accountsWorkflow.Setup(x => x.AddAsync(It.IsAny<OtpViewModel>())).ReturnsAsync(Result.Ok());
        var accounts = new ObservableCollection<OtpViewModel>();
        var sut = CreateSut(accountsWorkflow, new Mock<IMessageService>());

        var result = await sut.ImportAsync(ValidUri, accounts);

        Assert.Equal(QrAccountImportChangeKind.Added, result.ChangeKind);
        var added = Assert.Single(accounts);
        Assert.Equal("GitHub", added.Issuer);
        Assert.Equal("alice", added.AccountName);
        accountsWorkflow.Verify(x => x.AddAsync(added), Times.Once);
    }

    [Fact]
    public async Task ImportAsync_EquivalentExistingAccount_DoesNotPersistDuplicate()
    {
        var accountsWorkflow = new Mock<IAccountsWorkflowService>();
        var messages = new Mock<IMessageService>();
        var existing = new OtpViewModel(
            Guid.NewGuid(), "GitHub", "JBSW-Y3DP EHPK3PXP", "alice");
        var accounts = new ObservableCollection<OtpViewModel> { existing };
        var sut = CreateSut(accountsWorkflow, messages);

        var result = await sut.ImportAsync(ValidUri, accounts);

        Assert.Equal(QrAccountImportChangeKind.None, result.ChangeKind);
        Assert.Equal(existing.ID, result.AccountId);
        Assert.Single(accounts);
        accountsWorkflow.Verify(x => x.AddAsync(It.IsAny<OtpViewModel>()), Times.Never);
        messages.Verify(x => x.ShowInfo(It.IsAny<string>(), It.IsAny<int?>()), Times.Once);
    }

    [Fact]
    public async Task ImportAsync_InvalidUri_ShowsRecoverableError()
    {
        var accountsWorkflow = new Mock<IAccountsWorkflowService>();
        var messages = new Mock<IMessageService>();
        var sut = CreateSut(accountsWorkflow, messages);

        var result = await sut.ImportAsync(
            "https://example.test/not-an-otp-uri",
            new ObservableCollection<OtpViewModel>());

        Assert.Equal(QrAccountImportChangeKind.None, result.ChangeKind);
        messages.Verify(x => x.ShowError(It.IsAny<string>()), Times.Once);
        accountsWorkflow.Verify(x => x.AddAsync(It.IsAny<OtpViewModel>()), Times.Never);
    }

    private static QrAccountImportWorkflow CreateSut(
        Mock<IAccountsWorkflowService> accountsWorkflow,
        Mock<IMessageService> messages)
        => new(
            accountsWorkflow.Object,
            messages.Object,
            NullLogger<QrAccountImportWorkflow>.Instance);
}
