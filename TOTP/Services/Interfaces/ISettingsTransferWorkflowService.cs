using System.Threading.Tasks;
using TOTP.Core.Enums;
using TOTP.Core.Models;

namespace TOTP.Services.Interfaces;

public interface ISettingsTransferWorkflowService
{
    Task ExportAsync(bool exportEncrypt, ExportFileFormat selectedExportFormat);
    Task ImportAsync(ImportConflictStrategy selectedImportConflictStrategy);
}
