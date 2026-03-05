using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TOTP.Core.Enums;
using TOTP.Core.Models;
using TOTP.ViewModels;

namespace TOTP.Services.Interfaces;

public interface IAccountTransferWorkflowService
{
    Task ExportSecretsToFileAsync();
    Task ExportOtpsAsync(bool toBeEncrypted, ExportFileFormat format);
    Task ImportOtpsAsync(ImportConflictStrategy strategy, ObservableCollection<OtpViewModel> allOtps);
}
