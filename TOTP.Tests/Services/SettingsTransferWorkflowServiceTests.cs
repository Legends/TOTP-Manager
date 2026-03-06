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
        var tokenTransfer = new Mock<IAccountTransferWorkflowService>();
        var context = new Mock<IAccountsCollectionContext>();
        context.SetupGet(c => c.AllOtps).Returns(new ObservableCollection<OtpViewModel>());

        var sut = new SettingsTransferWorkflowService(tokenTransfer.Object, context.Object);

        await sut.ExportAsync(exportEncrypt: true, selectedExportFormat: ExportFileFormat.Totp);

        tokenTransfer.Verify(a => a.ExportOtpsAsync(true, ExportFileFormat.Totp), Times.Once);
    }

    [Fact]
    public async Task ImportAsync_ForwardsStrategyAndExactContextCollectionReference()
    {
        var tokenTransfer = new Mock<IAccountTransferWorkflowService>();
        var context = new Mock<IAccountsCollectionContext>();
        var otps = new ObservableCollection<OtpViewModel>
        {
            new(Guid.NewGuid(), "GitHub", "AAAA", "john")
        };
        context.SetupGet(c => c.AllOtps).Returns(otps);

        var sut = new SettingsTransferWorkflowService(tokenTransfer.Object, context.Object);

        await sut.ImportAsync(ImportConflictStrategy.KeepBoth);

        tokenTransfer.Verify(a => a.ImportOtpsAsync(ImportConflictStrategy.KeepBoth, otps), Times.Once);
    }

    [Fact]
    public async Task ImportAsync_WithEmptyCollection_StillCallsWorkflow()
    {
        var tokenTransfer = new Mock<IAccountTransferWorkflowService>();
        var context = new Mock<IAccountsCollectionContext>();
        var empty = new ObservableCollection<OtpViewModel>();
        context.SetupGet(c => c.AllOtps).Returns(empty);

        var sut = new SettingsTransferWorkflowService(tokenTransfer.Object, context.Object);

        await sut.ImportAsync(ImportConflictStrategy.SkipExisting);

        tokenTransfer.Verify(a => a.ImportOtpsAsync(ImportConflictStrategy.SkipExisting, empty), Times.Once);
    }

    [Fact]
    public async Task ImportAsync_UsesCurrentCollectionInstanceAtCallTime()
    {
        var tokenTransfer = new Mock<IAccountTransferWorkflowService>();
        var context = new Mock<IAccountsCollectionContext>();
        var first = new ObservableCollection<OtpViewModel>();
        var second = new ObservableCollection<OtpViewModel>();
        context.SetupSequence(c => c.AllOtps)
            .Returns(first)
            .Returns(second);

        var sut = new SettingsTransferWorkflowService(tokenTransfer.Object, context.Object);

        await sut.ImportAsync(ImportConflictStrategy.SkipExisting);
        await sut.ImportAsync(ImportConflictStrategy.SkipExisting);

        tokenTransfer.Verify(a => a.ImportOtpsAsync(ImportConflictStrategy.SkipExisting, It.Is<ObservableCollection<OtpViewModel>>(o => ReferenceEquals(o, first))), Times.Once);
        tokenTransfer.Verify(a => a.ImportOtpsAsync(ImportConflictStrategy.SkipExisting, It.Is<ObservableCollection<OtpViewModel>>(o => ReferenceEquals(o, second))), Times.Once);
    }
}
