using System.Threading.Tasks;
using TOTP.Core.Enums;
using TOTP.Core.Models;
using TOTP.Services.Interfaces;

namespace TOTP.Services;

public sealed class SettingsTransferWorkflowService(
    ITokenTransferWorkflowService tokenTransferWorkflowService,
    ITokensCollectionContext accountsCollectionContext) : ISettingsTransferWorkflowService
{
    public Task ExportAsync(bool exportEncrypt, ExportFileFormat selectedExportFormat)
        => tokenTransferWorkflowService.ExportOtpsAsync(exportEncrypt, selectedExportFormat);

    public Task ImportAsync(ImportConflictStrategy selectedImportConflictStrategy)
        => tokenTransferWorkflowService.ImportOtpsAsync(selectedImportConflictStrategy, accountsCollectionContext.AllOtps);
}
