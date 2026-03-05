using Moq;
using System.Collections.ObjectModel;
using TOTP.Core.Models;
using TOTP.Services;
using TOTP.Services.Interfaces;
using TOTP.ViewModels;

namespace TOTP.Tests.Services;

public sealed class SettingsTransferWorkflowServiceTests
{
    [Fact]
    public async Task ExportAsync_ForwardsEncryptionAndFormat()
    {
        var accountTransfer = new Mock<IAccountTransferWorkflowService>();
        var context = new Mock<IAccountsCollectionContext>();
        context.SetupGet(c => c.AllOtps).Returns(new ObservableCollection<OtpViewModel>());

        var sut = new SettingsTransferWorkflowService(accountTransfer.Object, context.Object);

        await sut.ExportAsync(exportEncrypt: true, selectedExportFormat: ExportFileFormat.Totp);

        accountTransfer.Verify(a => a.ExportOtpsAsync(true, ExportFileFormat.Totp), Times.Once);
    }

    [Fact]
    public async Task ImportAsync_ForwardsStrategyAndExactContextCollectionReference()
    {
        var accountTransfer = new Mock<IAccountTransferWorkflowService>();
        var context = new Mock<IAccountsCollectionContext>();
        var otps = new ObservableCollection<OtpViewModel>
        {
            new(Guid.NewGuid(), "GitHub", "AAAA", "john")
        };
        context.SetupGet(c => c.AllOtps).Returns(otps);

        var sut = new SettingsTransferWorkflowService(accountTransfer.Object, context.Object);

        await sut.ImportAsync(ImportConflictStrategy.KeepBoth);

        accountTransfer.Verify(a => a.ImportOtpsAsync(ImportConflictStrategy.KeepBoth, otps), Times.Once);
    }

    [Fact]
    public async Task ImportAsync_WithEmptyCollection_StillCallsWorkflow()
    {
        var accountTransfer = new Mock<IAccountTransferWorkflowService>();
        var context = new Mock<IAccountsCollectionContext>();
        var empty = new ObservableCollection<OtpViewModel>();
        context.SetupGet(c => c.AllOtps).Returns(empty);

        var sut = new SettingsTransferWorkflowService(accountTransfer.Object, context.Object);

        await sut.ImportAsync(ImportConflictStrategy.SkipExisting);

        accountTransfer.Verify(a => a.ImportOtpsAsync(ImportConflictStrategy.SkipExisting, empty), Times.Once);
    }
}
