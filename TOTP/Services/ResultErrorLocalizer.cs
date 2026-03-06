using TOTP.Core.Common;
using TOTP.Resources;

namespace TOTP.Services;

internal static class ResultErrorLocalizer
{
    public static string ToUserMessage(AppErrorCode code, string? context)
    {
        return code switch
        {
            AppErrorCode.OtpStorageReadFailed => UI.err_OtpStorageReadFailed,
            AppErrorCode.OtpStorageAccessDenied => UI.err_OtpStorageAccessDenied,
            AppErrorCode.OtpStorageDecryptFailed => UI.err_OtpStorageDecryptFailed,
            AppErrorCode.OtpStorageWriteFailed => UI.err_OtpStorageWriteFailed,
            AppErrorCode.OtpStorageEncryptionFailed => UI.err_OtpStorageEncryptionFailed,
            AppErrorCode.OtpStorageBackupFailed => UI.err_OtpStorageBackupFailed,
            AppErrorCode.OtpCreateFailed => Format(UI.err_OtpCreateFailed, context),
            AppErrorCode.OtpUpdateFailed => Format(UI.err_OtpUpdateFailed, context),
            AppErrorCode.OtpDeleteFailed => Format(UI.err_OtpDeleteFailed, context),
            AppErrorCode.AppSettingsLoadFailed => UI.err_AppSettingsLoadFailed,
            AppErrorCode.AppSettingsLoadAccessDenied => UI.err_AppSettingsLoadAccessDenied,
            AppErrorCode.AppSettingsDecryptFailed => UI.err_AppSettingsDecryptFailed,
            AppErrorCode.AppSettingsDeserializeFailed => UI.err_AppSettingsDeserializeFailed,
            AppErrorCode.AppSettingsSaveFailed => UI.err_AppSettingsSaveFailed,
            AppErrorCode.AppSettingsSaveAccessDenied => UI.err_AppSettingsSaveAccessDenied,
            AppErrorCode.AppSettingsEncryptFailed => UI.err_AppSettingsEncryptFailed,
            AppErrorCode.ExportFileWriteFailed => UI.err_ExportFileWriteFailed,
            AppErrorCode.ExportFileAccessDenied => UI.err_ExportFileAccessDenied,
            AppErrorCode.ExportEncryptionFailed => UI.err_ExportEncryptionFailed,
            AppErrorCode.ExportUnknownFailed => UI.err_ExportUnknownFailed,
            AppErrorCode.ImportFileNotFound => UI.err_ImportFileNotFound,
            AppErrorCode.ImportInvalidFile => UI.err_ImportInvalidFile,
            AppErrorCode.ImportWrongPasswordOrTampered => UI.err_ImportWrongPasswordOrTampered,
            AppErrorCode.ImportInvalidPayload => UI.err_ImportInvalidPayload,
            AppErrorCode.ImportUnknownFailed => UI.err_ImportUnknownFailed,
            AppErrorCode.TokensLoadFailed => UI.err_TokensLoadFailed,
            AppErrorCode.TokensCreateFailed => UI.err_TokensCreateFailed,
            AppErrorCode.TokensUpdateFailed => UI.err_TokensUpdateFailed,
            AppErrorCode.TokensDeleteFailed => UI.err_TokensDeleteFailed,
            AppErrorCode.SettingsServiceLoadFailed => UI.err_SettingsServiceLoadFailed,
            AppErrorCode.SettingsServiceSaveFailed => UI.err_SettingsServiceSaveFailed,
            _ => UI.err_Unknown
        };
    }

    private static string Format(string message, string? context)
    {
        return string.IsNullOrWhiteSpace(context) ? message : string.Format(message, context);
    }
}
