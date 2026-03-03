namespace TOTP.Core.Common;

public enum AppErrorCode
{
    Unknown = 0,

    // OTP storage and account operations
    OtpStorageReadFailed,
    OtpStorageAccessDenied,
    OtpStorageDecryptFailed,
    OtpStorageWriteFailed,
    OtpStorageEncryptionFailed,
    OtpStorageBackupFailed,
    OtpCreateFailed,
    OtpUpdateFailed,
    OtpDeleteFailed,

    // App settings
    AppSettingsLoadFailed,
    AppSettingsLoadAccessDenied,
    AppSettingsDecryptFailed,
    AppSettingsDeserializeFailed,
    AppSettingsSaveFailed,
    AppSettingsSaveAccessDenied,
    AppSettingsEncryptFailed,

    // Encrypted import/export
    ExportFileWriteFailed,
    ExportFileAccessDenied,
    ExportEncryptionFailed,
    ExportUnknownFailed,
    ImportFileNotFound,
    ImportInvalidFile,
    ImportWrongPasswordOrTampered,
    ImportInvalidPayload,
    ImportUnknownFailed,

    // Workflow/service boundaries
    AccountsLoadFailed,
    AccountsCreateFailed,
    AccountsUpdateFailed,
    AccountsDeleteFailed,
    SettingsServiceLoadFailed,
    SettingsServiceSaveFailed
}
