using System.Threading.Tasks;
using TOTP.Core.Enums;

namespace TOTP.Services.Interfaces;

public interface ISettingsPersistenceService
{
    SettingsGeneralSnapshot ReadCurrentGeneralSettings();
    SettingsGeneralSnapshot CreateDefaultGeneralSettings();
    Task<SettingsPersistenceResult> SaveGeneralSettingsAsync(SettingsGeneralSnapshot snapshot);
}

public sealed record SettingsGeneralSnapshot(
    AppLogLevel SelectedLogLevel,
    bool LockOnSessionLock,
    bool LockOnMinimize,
    bool LockOnIdleTimeout,
    int IdleTimeoutMinutes,
    bool ClearClipboardEnabled,
    int ClearClipboardSeconds,
    double QrPreviewScaleFactor,
    bool ExportEncrypt,
    bool OpenExportFileAfterExportBeforeEncrypt,
    bool HideSecretsByDefault);

public sealed record SettingsPersistenceResult(
    bool IsSuccess,
    string? ErrorMessage = null,
    bool LogSwitchStateChanged = false);
