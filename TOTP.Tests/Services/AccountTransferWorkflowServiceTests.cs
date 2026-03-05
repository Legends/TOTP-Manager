using FluentResults;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.ObjectModel;
using TOTP.Core.Enums;
using TOTP.Core.Models;
using TOTP.Core.Security.Interfaces;
using TOTP.Core.Services.Interfaces;
using TOTP.Services;
using TOTP.Services.Interfaces;
using TOTP.ViewModels;

namespace TOTP.Tests.Services;

public sealed class AccountTransferWorkflowServiceTests
{
    [Fact]
    public async Task ExportOtpsAsync_WhenSaveDialogCanceled_DoesNothing()
    {
        var fileDialog = new Mock<IFileDialogService>();
        var accounts = new Mock<IAccountsWorkflowService>();
        var export = new Mock<IExportService>();
        var prompt = new Mock<IPasswordPromptService>();
        var message = new Mock<IMessageService>();
        var settings = new Mock<ISettingsService>();
        var logger = new Mock<ILogger<AccountTransferWorkflowService>>();

        fileDialog.Setup(f => f.ShowSaveFileDialog(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns((string?)null);

        var sut = new AccountTransferWorkflowService(
            fileDialog.Object,
            accounts.Object,
            export.Object,
            prompt.Object,
            message.Object,
            settings.Object,
            logger.Object);

        await sut.ExportOtpsAsync(toBeEncrypted: false, ExportFileFormat.Json);

        accounts.Verify(a => a.GetAllEntriesSortedAsync(), Times.Never);
        export.Verify(e => e.ExportToFileAsync(It.IsAny<IEnumerable<OtpEntry>>(), It.IsAny<string>(), It.IsAny<ExportFileFormat>()), Times.Never);
    }

    [Fact]
    public async Task ExportOtpsAsync_WhenEncryptedAndPasswordMissing_ShowsWarning()
    {
        var fileDialog = new Mock<IFileDialogService>();
        var accounts = new Mock<IAccountsWorkflowService>();
        var export = new Mock<IExportService>();
        var prompt = new Mock<IPasswordPromptService>();
        var message = new Mock<IMessageService>();
        var settings = new Mock<ISettingsService>();
        var logger = new Mock<ILogger<AccountTransferWorkflowService>>();

        fileDialog.Setup(f => f.ShowSaveFileDialog(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns("backup.totp");
        accounts.Setup(a => a.GetAllEntriesSortedAsync())
            .ReturnsAsync(Result.Ok(new ObservableCollection<OtpEntry>()));
        prompt.Setup(p => p.PromptForEncryptedExportPassword(It.IsAny<string>()))
            .Returns((string?)null);

        var sut = new AccountTransferWorkflowService(
            fileDialog.Object,
            accounts.Object,
            export.Object,
            prompt.Object,
            message.Object,
            settings.Object,
            logger.Object);

        await sut.ExportOtpsAsync(toBeEncrypted: true, ExportFileFormat.Json);

        message.Verify(m => m.ShowWarning(TOTP.Resources.UI.ui_ExportPasswordRequired), Times.Once);
        export.Verify(e => e.ExportToEncryptedFileAsync(It.IsAny<IEnumerable<OtpEntry>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ExportFileFormat>()), Times.Never);
    }

    [Fact]
    public async Task ImportOtpsAsync_WhenOpenDialogCanceled_DoesNothing()
    {
        var fileDialog = new Mock<IFileDialogService>();
        var accounts = new Mock<IAccountsWorkflowService>();
        var export = new Mock<IExportService>();
        var prompt = new Mock<IPasswordPromptService>();
        var message = new Mock<IMessageService>();
        var settings = new Mock<ISettingsService>();
        var logger = new Mock<ILogger<AccountTransferWorkflowService>>();

        fileDialog.Setup(f => f.ShowOpenFileDialog(It.IsAny<string>(), It.IsAny<string>()))
            .Returns((string?)null);

        var sut = new AccountTransferWorkflowService(
            fileDialog.Object,
            accounts.Object,
            export.Object,
            prompt.Object,
            message.Object,
            settings.Object,
            logger.Object);

        await sut.ImportOtpsAsync(ImportConflictStrategy.SkipExisting, []);

        export.Verify(e => e.ImportFromFileAsync(It.IsAny<string>(), It.IsAny<string?>()), Times.Never);
    }

    [Fact]
    public async Task ImportOtpsAsync_WhenImportFails_ShowsResultError()
    {
        var fileDialog = new Mock<IFileDialogService>();
        var accounts = new Mock<IAccountsWorkflowService>();
        var export = new Mock<IExportService>();
        var prompt = new Mock<IPasswordPromptService>();
        var message = new Mock<IMessageService>();
        var settings = new Mock<ISettingsService>();
        var logger = new Mock<ILogger<AccountTransferWorkflowService>>();

        fileDialog.Setup(f => f.ShowOpenFileDialog(It.IsAny<string>(), It.IsAny<string>()))
            .Returns("import.json");
        export.Setup(e => e.ImportFromFileAsync("import.json", null))
            .ReturnsAsync(Result.Fail<List<OtpEntry>>("import failed"));

        var sut = new AccountTransferWorkflowService(
            fileDialog.Object,
            accounts.Object,
            export.Object,
            prompt.Object,
            message.Object,
            settings.Object,
            logger.Object);

        await sut.ImportOtpsAsync(ImportConflictStrategy.SkipExisting, []);

        message.Verify(m => m.ShowResultError(It.Is<IResultBase>(r => r.IsFailed), null), Times.Once);
    }

    [Fact]
    public async Task ExportOtpsAsync_WhenPlain_CallsExportToFileWithSelectedFormat()
    {
        var fileDialog = new Mock<IFileDialogService>();
        var accounts = new Mock<IAccountsWorkflowService>();
        var export = new Mock<IExportService>();
        var prompt = new Mock<IPasswordPromptService>();
        var message = new Mock<IMessageService>();
        var settings = new Mock<ISettingsService>();
        var logger = new Mock<ILogger<AccountTransferWorkflowService>>();

        var appSettings = new AppSettings { OpenExportFileAfterExport = false };
        settings.SetupGet(s => s.Current).Returns(appSettings);
        fileDialog.Setup(f => f.ShowSaveFileDialog(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns("backup.json");
        accounts.Setup(a => a.GetAllEntriesSortedAsync())
            .ReturnsAsync(Result.Ok(new ObservableCollection<OtpEntry>
            {
                new(Guid.NewGuid(), "GitHub", "JBSWY3DPEHPK3PXP", "john")
            }));
        export.Setup(e => e.ExportToFileAsync(It.IsAny<IEnumerable<OtpEntry>>(), "backup.json", ExportFileFormat.Json))
            .ReturnsAsync(Result.Ok());

        var sut = new AccountTransferWorkflowService(
            fileDialog.Object,
            accounts.Object,
            export.Object,
            prompt.Object,
            message.Object,
            settings.Object,
            logger.Object);

        await sut.ExportOtpsAsync(toBeEncrypted: false, ExportFileFormat.Json);

        export.Verify(e => e.ExportToFileAsync(It.IsAny<IEnumerable<OtpEntry>>(), "backup.json", ExportFileFormat.Json), Times.Once);
        message.Verify(m => m.ShowSuccess(TOTP.Resources.UI.ui_Settings_Export_Success, 1), Times.Once);
    }

    [Fact]
    public async Task ExportOtpsAsync_WhenEncryptedWithPassword_CallsEncryptedExport()
    {
        var fileDialog = new Mock<IFileDialogService>();
        var accounts = new Mock<IAccountsWorkflowService>();
        var export = new Mock<IExportService>();
        var prompt = new Mock<IPasswordPromptService>();
        var message = new Mock<IMessageService>();
        var settings = new Mock<ISettingsService>();
        var logger = new Mock<ILogger<AccountTransferWorkflowService>>();

        settings.SetupGet(s => s.Current).Returns(new AppSettings());
        fileDialog.Setup(f => f.ShowSaveFileDialog(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns("backup.totp");
        accounts.Setup(a => a.GetAllEntriesSortedAsync())
            .ReturnsAsync(Result.Ok(new ObservableCollection<OtpEntry>
            {
                new(Guid.NewGuid(), "GitHub", "JBSWY3DPEHPK3PXP", "john")
            }));
        prompt.Setup(p => p.PromptForEncryptedExportPassword(It.IsAny<string>())).Returns("secret");
        export.Setup(e => e.ExportToEncryptedFileAsync(It.IsAny<IEnumerable<OtpEntry>>(), "secret", "backup.totp", ExportFileFormat.Json))
            .ReturnsAsync(Result.Ok());

        var sut = new AccountTransferWorkflowService(
            fileDialog.Object,
            accounts.Object,
            export.Object,
            prompt.Object,
            message.Object,
            settings.Object,
            logger.Object);

        await sut.ExportOtpsAsync(toBeEncrypted: true, ExportFileFormat.Csv);

        export.Verify(e => e.ExportToEncryptedFileAsync(It.IsAny<IEnumerable<OtpEntry>>(), "secret", "backup.totp", ExportFileFormat.Json), Times.Once);
    }

    [Fact]
    public async Task ImportOtpsAsync_WhenNoImportedEntries_ShowsInfo()
    {
        var fileDialog = new Mock<IFileDialogService>();
        var accounts = new Mock<IAccountsWorkflowService>();
        var export = new Mock<IExportService>();
        var prompt = new Mock<IPasswordPromptService>();
        var message = new Mock<IMessageService>();
        var settings = new Mock<ISettingsService>();
        var logger = new Mock<ILogger<AccountTransferWorkflowService>>();

        fileDialog.Setup(f => f.ShowOpenFileDialog(It.IsAny<string>(), It.IsAny<string>()))
            .Returns("import.json");
        export.Setup(e => e.ImportFromFileAsync("import.json", null))
            .ReturnsAsync(Result.Ok(new List<OtpEntry>()));

        var sut = new AccountTransferWorkflowService(
            fileDialog.Object,
            accounts.Object,
            export.Object,
            prompt.Object,
            message.Object,
            settings.Object,
            logger.Object);

        await sut.ImportOtpsAsync(ImportConflictStrategy.SkipExisting, []);

        message.Verify(m => m.ShowInfo(TOTP.Resources.UI.ui_Import_NoEntriesFound, null), Times.Once);
    }

    [Fact]
    public async Task ImportOtpsAsync_WhenSkipExisting_DoesNotAddDuplicate()
    {
        var existingId = Guid.NewGuid();
        var allOtps = new ObservableCollection<OtpViewModel>
        {
            new(existingId, "GitHub", "AAAABBBBCCCCDDDD", "john")
        };

        var fileDialog = new Mock<IFileDialogService>();
        var accounts = new Mock<IAccountsWorkflowService>();
        var export = new Mock<IExportService>();
        var prompt = new Mock<IPasswordPromptService>();
        var message = new Mock<IMessageService>();
        var settings = new Mock<ISettingsService>();
        var logger = new Mock<ILogger<AccountTransferWorkflowService>>();

        fileDialog.Setup(f => f.ShowOpenFileDialog(It.IsAny<string>(), It.IsAny<string>()))
            .Returns("import.json");
        export.Setup(e => e.ImportFromFileAsync("import.json", null))
            .ReturnsAsync(Result.Ok(new List<OtpEntry>
            {
                new(Guid.NewGuid(), "GitHub", "ZZZZYYYYXXXXWWWW", "john")
            }));
        message.Setup(m => m.ConfirmInfo(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(true);

        var sut = new AccountTransferWorkflowService(
            fileDialog.Object,
            accounts.Object,
            export.Object,
            prompt.Object,
            message.Object,
            settings.Object,
            logger.Object);

        await sut.ImportOtpsAsync(ImportConflictStrategy.SkipExisting, allOtps);

        Assert.Single(allOtps);
        accounts.Verify(a => a.AddAsync(It.IsAny<OtpViewModel>()), Times.Never);
        accounts.Verify(a => a.UpdateAsync(It.IsAny<OtpViewModel?>(), It.IsAny<OtpViewModel>()), Times.Never);
    }

    [Fact]
    public async Task ImportOtpsAsync_WhenPreviewCanceled_DoesNotMutateCollection()
    {
        var allOtps = new ObservableCollection<OtpViewModel>();

        var fileDialog = new Mock<IFileDialogService>();
        var accounts = new Mock<IAccountsWorkflowService>();
        var export = new Mock<IExportService>();
        var prompt = new Mock<IPasswordPromptService>();
        var message = new Mock<IMessageService>();
        var settings = new Mock<ISettingsService>();
        var logger = new Mock<ILogger<AccountTransferWorkflowService>>();

        fileDialog.Setup(f => f.ShowOpenFileDialog(It.IsAny<string>(), It.IsAny<string>()))
            .Returns("import.json");
        export.Setup(e => e.ImportFromFileAsync("import.json", null))
            .ReturnsAsync(Result.Ok(new List<OtpEntry> { new(Guid.NewGuid(), "GitHub", "AAAA", "john") }));
        message.Setup(m => m.ConfirmInfo(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(false);

        var sut = new AccountTransferWorkflowService(
            fileDialog.Object, accounts.Object, export.Object, prompt.Object, message.Object, settings.Object, logger.Object);

        await sut.ImportOtpsAsync(ImportConflictStrategy.SkipExisting, allOtps);

        Assert.Empty(allOtps);
        accounts.Verify(a => a.AddAsync(It.IsAny<OtpViewModel>()), Times.Never);
        accounts.Verify(a => a.UpdateAsync(It.IsAny<OtpViewModel?>(), It.IsAny<OtpViewModel>()), Times.Never);
    }

    [Fact]
    public async Task ImportOtpsAsync_WhenReplaceExisting_UpdatesExisting()
    {
        var existingId = Guid.NewGuid();
        var allOtps = new ObservableCollection<OtpViewModel>
        {
            new(existingId, "GitHub", "OLDSECRET", "john")
        };

        var fileDialog = new Mock<IFileDialogService>();
        var accounts = new Mock<IAccountsWorkflowService>();
        var export = new Mock<IExportService>();
        var prompt = new Mock<IPasswordPromptService>();
        var message = new Mock<IMessageService>();
        var settings = new Mock<ISettingsService>();
        var logger = new Mock<ILogger<AccountTransferWorkflowService>>();

        fileDialog.Setup(f => f.ShowOpenFileDialog(It.IsAny<string>(), It.IsAny<string>()))
            .Returns("import.json");
        export.Setup(e => e.ImportFromFileAsync("import.json", null))
            .ReturnsAsync(Result.Ok(new List<OtpEntry>
            {
                new(Guid.NewGuid(), "GitHub", "NEWSECRET", "john")
            }));
        message.Setup(m => m.ConfirmInfo(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        accounts.Setup(a => a.UpdateAsync(It.IsAny<OtpViewModel>(), It.IsAny<OtpViewModel>())).ReturnsAsync(Result.Ok());

        var sut = new AccountTransferWorkflowService(
            fileDialog.Object, accounts.Object, export.Object, prompt.Object, message.Object, settings.Object, logger.Object);

        await sut.ImportOtpsAsync(ImportConflictStrategy.ReplaceExisting, allOtps);

        Assert.Single(allOtps);
        Assert.Equal("NEWSECRET", allOtps[0].Secret);
        accounts.Verify(a => a.UpdateAsync(It.IsAny<OtpViewModel>(), It.IsAny<OtpViewModel>()), Times.Once);
    }

    [Fact]
    public async Task ImportOtpsAsync_WhenKeepBoth_AddsRenamedDuplicate()
    {
        var allOtps = new ObservableCollection<OtpViewModel>
        {
            new(Guid.NewGuid(), "GitHub", "OLDSECRET", "john")
        };

        var fileDialog = new Mock<IFileDialogService>();
        var accounts = new Mock<IAccountsWorkflowService>();
        var export = new Mock<IExportService>();
        var prompt = new Mock<IPasswordPromptService>();
        var message = new Mock<IMessageService>();
        var settings = new Mock<ISettingsService>();
        var logger = new Mock<ILogger<AccountTransferWorkflowService>>();

        fileDialog.Setup(f => f.ShowOpenFileDialog(It.IsAny<string>(), It.IsAny<string>()))
            .Returns("import.json");
        export.Setup(e => e.ImportFromFileAsync("import.json", null))
            .ReturnsAsync(Result.Ok(new List<OtpEntry>
            {
                new(Guid.NewGuid(), "GitHub", "NEWSECRET", "john")
            }));
        message.Setup(m => m.ConfirmInfo(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        accounts.Setup(a => a.AddAsync(It.IsAny<OtpViewModel>())).ReturnsAsync(Result.Ok());

        var sut = new AccountTransferWorkflowService(
            fileDialog.Object, accounts.Object, export.Object, prompt.Object, message.Object, settings.Object, logger.Object);

        await sut.ImportOtpsAsync(ImportConflictStrategy.KeepBoth, allOtps);

        Assert.Equal(2, allOtps.Count);
        Assert.Contains(allOtps, o => o.Issuer != null && o.Issuer.StartsWith("GitHub (imported)", StringComparison.OrdinalIgnoreCase));
        accounts.Verify(a => a.AddAsync(It.IsAny<OtpViewModel>()), Times.Once);
    }

    [Fact]
    public async Task ImportOtpsAsync_WhenEncryptedPromptCanceled_DoesNotImport()
    {
        var allOtps = new ObservableCollection<OtpViewModel>();
        var fileDialog = new Mock<IFileDialogService>();
        var accounts = new Mock<IAccountsWorkflowService>();
        var export = new Mock<IExportService>();
        var prompt = new Mock<IPasswordPromptService>();
        var message = new Mock<IMessageService>();
        var settings = new Mock<ISettingsService>();
        var logger = new Mock<ILogger<AccountTransferWorkflowService>>();

        fileDialog.Setup(f => f.ShowOpenFileDialog(It.IsAny<string>(), It.IsAny<string>()))
            .Returns("import.totp");
        prompt.Setup(p => p.Prompt(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            It.IsAny<Func<string, Task<string?>>>()))
            .Returns((string?)null);

        var sut = new AccountTransferWorkflowService(
            fileDialog.Object, accounts.Object, export.Object, prompt.Object, message.Object, settings.Object, logger.Object);

        await sut.ImportOtpsAsync(ImportConflictStrategy.SkipExisting, allOtps);

        export.Verify(e => e.ImportFromFileAsync(It.IsAny<string>(), It.IsAny<string?>()), Times.Never);
        Assert.Empty(allOtps);
    }

    [Fact]
    public async Task ExportOtpsAsync_WhenAccountsLoadFails_ShowsResultError()
    {
        var fileDialog = new Mock<IFileDialogService>();
        var accounts = new Mock<IAccountsWorkflowService>();
        var export = new Mock<IExportService>();
        var prompt = new Mock<IPasswordPromptService>();
        var message = new Mock<IMessageService>();
        var settings = new Mock<ISettingsService>();
        var logger = new Mock<ILogger<AccountTransferWorkflowService>>();

        settings.SetupGet(s => s.Current).Returns(new AppSettings());
        fileDialog.Setup(f => f.ShowSaveFileDialog(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns("backup.json");
        accounts.Setup(a => a.GetAllEntriesSortedAsync())
            .ReturnsAsync(Result.Fail<ObservableCollection<OtpEntry>>("load failed"));

        var sut = new AccountTransferWorkflowService(
            fileDialog.Object, accounts.Object, export.Object, prompt.Object, message.Object, settings.Object, logger.Object);

        await sut.ExportOtpsAsync(false, ExportFileFormat.Json);

        message.Verify(m => m.ShowResultError(It.Is<IResultBase>(r => r.IsFailed), null), Times.Once);
        export.Verify(e => e.ExportToFileAsync(It.IsAny<IEnumerable<OtpEntry>>(), It.IsAny<string>(), It.IsAny<ExportFileFormat>()), Times.Never);
    }

    [Fact]
    public async Task ExportOtpsAsync_WhenExportFails_ShowsResultError()
    {
        var fileDialog = new Mock<IFileDialogService>();
        var accounts = new Mock<IAccountsWorkflowService>();
        var export = new Mock<IExportService>();
        var prompt = new Mock<IPasswordPromptService>();
        var message = new Mock<IMessageService>();
        var settings = new Mock<ISettingsService>();
        var logger = new Mock<ILogger<AccountTransferWorkflowService>>();

        settings.SetupGet(s => s.Current).Returns(new AppSettings());
        fileDialog.Setup(f => f.ShowSaveFileDialog(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns("backup.json");
        accounts.Setup(a => a.GetAllEntriesSortedAsync())
            .ReturnsAsync(Result.Ok(new ObservableCollection<OtpEntry> { new(Guid.NewGuid(), "GitHub", "AAAA", "john") }));
        export.Setup(e => e.ExportToFileAsync(It.IsAny<IEnumerable<OtpEntry>>(), "backup.json", ExportFileFormat.Json))
            .ReturnsAsync(Result.Fail("export failed"));

        var sut = new AccountTransferWorkflowService(
            fileDialog.Object, accounts.Object, export.Object, prompt.Object, message.Object, settings.Object, logger.Object);

        await sut.ExportOtpsAsync(false, ExportFileFormat.Json);

        message.Verify(m => m.ShowResultError(It.Is<IResultBase>(r => r.IsFailed), null), Times.Once);
    }

    [Fact]
    public async Task ImportOtpsAsync_WhenTotpAndPasswordProvided_UsesPassword()
    {
        var allOtps = new ObservableCollection<OtpViewModel>();
        var fileDialog = new Mock<IFileDialogService>();
        var accounts = new Mock<IAccountsWorkflowService>();
        var export = new Mock<IExportService>();
        var prompt = new Mock<IPasswordPromptService>();
        var message = new Mock<IMessageService>();
        var settings = new Mock<ISettingsService>();
        var logger = new Mock<ILogger<AccountTransferWorkflowService>>();

        fileDialog.Setup(f => f.ShowOpenFileDialog(It.IsAny<string>(), It.IsAny<string>()))
            .Returns("import.totp");
        prompt.Setup(p => p.Prompt(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<Func<string, Task<string?>>>()))
            .Returns("pw123");
        export.Setup(e => e.ImportFromFileAsync("import.totp", "pw123"))
            .ReturnsAsync(Result.Ok(new List<OtpEntry>()));

        var sut = new AccountTransferWorkflowService(
            fileDialog.Object, accounts.Object, export.Object, prompt.Object, message.Object, settings.Object, logger.Object);

        await sut.ImportOtpsAsync(ImportConflictStrategy.SkipExisting, allOtps);

        export.Verify(e => e.ImportFromFileAsync("import.totp", "pw123"), Times.Once);
    }

    [Fact]
    public async Task ImportOtpsAsync_WhenReplaceExistingUpdateFails_DoesNotChangeOriginal()
    {
        var existingId = Guid.NewGuid();
        var allOtps = new ObservableCollection<OtpViewModel>
        {
            new(existingId, "GitHub", "OLDSECRET", "john")
        };

        var fileDialog = new Mock<IFileDialogService>();
        var accounts = new Mock<IAccountsWorkflowService>();
        var export = new Mock<IExportService>();
        var prompt = new Mock<IPasswordPromptService>();
        var message = new Mock<IMessageService>();
        var settings = new Mock<ISettingsService>();
        var logger = new Mock<ILogger<AccountTransferWorkflowService>>();

        fileDialog.Setup(f => f.ShowOpenFileDialog(It.IsAny<string>(), It.IsAny<string>()))
            .Returns("import.json");
        export.Setup(e => e.ImportFromFileAsync("import.json", null))
            .ReturnsAsync(Result.Ok(new List<OtpEntry> { new(Guid.NewGuid(), "GitHub", "NEWSECRET", "john") }));
        message.Setup(m => m.ConfirmInfo(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        accounts.Setup(a => a.UpdateAsync(It.IsAny<OtpViewModel>(), It.IsAny<OtpViewModel>())).ReturnsAsync(Result.Fail("update failed"));

        var sut = new AccountTransferWorkflowService(
            fileDialog.Object, accounts.Object, export.Object, prompt.Object, message.Object, settings.Object, logger.Object);

        await sut.ImportOtpsAsync(ImportConflictStrategy.ReplaceExisting, allOtps);

        Assert.Single(allOtps);
        Assert.Equal("OLDSECRET", allOtps[0].Secret);
    }

    [Fact]
    public async Task ImportOtpsAsync_WhenKeepBothWithCollision_AppendsIncrementedSuffix()
    {
        var allOtps = new ObservableCollection<OtpViewModel>
        {
            new(Guid.NewGuid(), "GitHub", "OLD1", "john"),
            new(Guid.NewGuid(), "GitHub (imported)", "OLD2", "john")
        };

        var fileDialog = new Mock<IFileDialogService>();
        var accounts = new Mock<IAccountsWorkflowService>();
        var export = new Mock<IExportService>();
        var prompt = new Mock<IPasswordPromptService>();
        var message = new Mock<IMessageService>();
        var settings = new Mock<ISettingsService>();
        var logger = new Mock<ILogger<AccountTransferWorkflowService>>();

        fileDialog.Setup(f => f.ShowOpenFileDialog(It.IsAny<string>(), It.IsAny<string>()))
            .Returns("import.json");
        export.Setup(e => e.ImportFromFileAsync("import.json", null))
            .ReturnsAsync(Result.Ok(new List<OtpEntry> { new(Guid.NewGuid(), "GitHub", "NEWSECRET", "john") }));
        message.Setup(m => m.ConfirmInfo(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        accounts.Setup(a => a.AddAsync(It.IsAny<OtpViewModel>())).ReturnsAsync(Result.Ok());

        var sut = new AccountTransferWorkflowService(
            fileDialog.Object, accounts.Object, export.Object, prompt.Object, message.Object, settings.Object, logger.Object);

        await sut.ImportOtpsAsync(ImportConflictStrategy.KeepBoth, allOtps);

        Assert.Contains(allOtps, o => string.Equals(o.Issuer, "GitHub (imported 2)", StringComparison.OrdinalIgnoreCase));
    }
}
