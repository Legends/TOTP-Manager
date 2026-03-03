using System.Security.Cryptography;
using System.Text.Json;
using TOTP.Core.Common;

namespace TOTP.DAL.Common;

internal static class AppSettingsDalErrorMapper
{
    public static AppError MapLoadError(Exception ex) =>
        ex switch
        {
            UnauthorizedAccessException => new AppError(AppErrorCode.AppSettingsLoadAccessDenied, "Access denied while loading app settings.", ex),
            CryptographicException => new AppError(AppErrorCode.AppSettingsDecryptFailed, "Failed to decrypt app settings.", ex),
            JsonException => new AppError(AppErrorCode.AppSettingsDeserializeFailed, "Failed to deserialize app settings.", ex),
            IOException => new AppError(AppErrorCode.AppSettingsLoadFailed, "I/O error while loading app settings.", ex),
            _ => new AppError(AppErrorCode.AppSettingsLoadFailed, "Unexpected error while loading app settings.", ex)
        };

    public static AppError MapSaveError(Exception ex) =>
        ex switch
        {
            UnauthorizedAccessException => new AppError(AppErrorCode.AppSettingsSaveAccessDenied, "Access denied while saving app settings.", ex),
            CryptographicException => new AppError(AppErrorCode.AppSettingsEncryptFailed, "Failed to encrypt app settings.", ex),
            IOException => new AppError(AppErrorCode.AppSettingsSaveFailed, "I/O error while saving app settings.", ex),
            _ => new AppError(AppErrorCode.AppSettingsSaveFailed, "Unexpected error while saving app settings.", ex)
        };
}
