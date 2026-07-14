using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TOTP.ViewModels;

namespace TOTP.Services.Interfaces;

public enum QrAccountImportChangeKind
{
    None,
    Added,
    Updated
}

public sealed record QrAccountImportResult(QrAccountImportChangeKind ChangeKind, Guid? AccountId = null);

public interface IQrAccountImportWorkflow
{
    Task<QrAccountImportResult> ImportAsync(
        string decodedOtpAuthUri,
        ObservableCollection<OtpViewModel> accounts);
}
