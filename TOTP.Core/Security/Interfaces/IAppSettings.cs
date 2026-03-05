using TOTP.Core.Enums;
using TOTP.Core.Security.Models;

namespace TOTP.Core.Security.Interfaces;

public interface IAppSettings
{
    AppLogLevel MinimumLogLevel { get; set; }
    AuthorizationProfile Authorization { get; set; }
    TimeSpan IdleTimeout { get; set; }
    bool LockOnSessionLock { get; set; }
    bool LockOnMinimize { get; set; }
    bool ClearClipboardEnabled { get; set; }
    int ClearClipboardSeconds { get; set; }
    double QrPreviewScaleFactor { get; set; }
    bool ExportEncrypt { get; set; }
    bool OpenExportFileAfterExport { get; set; }
    bool HideSecretsByDefault { get; set; }
}
