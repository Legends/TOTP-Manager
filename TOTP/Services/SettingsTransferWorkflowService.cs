using System.Threading.Tasks;
using TOTP.Core.Enums;
using TOTP.Core.Models;
using TOTP.Services.Interfaces;

namespace TOTP.Services;

public sealed class SettingsTransferWorkflowService(
    IAccountTransferWorkflowService accountTransferWorkflowService,
    IAccountsCollectionContext accountsCollectionContext) : ISettingsTransferWorkflowService
{
    public Task ExportAsync(bool exportEncrypt, ExportFileFormat selectedExportFormat)
        => accountTransferWorkflowService.ExportOtpsAsync(exportEncrypt, selectedExportFormat);

    public Task ImportAsync(ImportConflictStrategy selectedImportConflictStrategy)
        => accountTransferWorkflowService.ImportOtpsAsync(selectedImportConflictStrategy, accountsCollectionContext.AllOtps);
}
