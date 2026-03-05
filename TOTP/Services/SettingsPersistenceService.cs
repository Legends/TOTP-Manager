using System;
using System.Linq;
using System.Threading.Tasks;
using TOTP.Core.Enums;
using TOTP.Core.Models;
using TOTP.Core.Security.Interfaces;
using TOTP.Core.Services.Interfaces;
using TOTP.Services.Interfaces;

namespace TOTP.Services;

public sealed class SettingsPersistenceService(
    ISettingsService settingsService,
    ILogSwitchService logSwitchService,
    IQrPreviewService qrPreviewService) : ISettingsPersistenceService
{
    private const double MinQrPreviewScale = 1.0;
    private const double MaxQrPreviewScale = 6.0;

    private IAppSettings Settings => settingsService.Current;

    public SettingsGeneralSnapshot ReadCurrentGeneralSettings()
    {
        var selectedLogLevel = logSwitchService.IsCliOverrideActive
            ? logSwitchService.GetLevel()
            : Settings.MinimumLogLevel;

        var qrScale = Math.Clamp(
            Settings.QrPreviewScaleFactor > 0 ? Settings.QrPreviewScaleFactor : AppSettings.DefaultQrPreviewScaleFactor,
            MinQrPreviewScale,
            MaxQrPreviewScale);

        return new SettingsGeneralSnapshot(
            selectedLogLevel,
            Settings.LockOnSessionLock,
            Settings.LockOnMinimize,
            Settings.IdleTimeout > TimeSpan.Zero,
            (int)Math.Max(1, Settings.IdleTimeout.TotalMinutes),
            Settings.ClearClipboardEnabled,
            Settings.ClearClipboardSeconds > 0 ? Settings.ClearClipboardSeconds : AppSettings.DefaultClearClipboardSeconds,
            qrScale,
            Settings.ExportEncrypt,
            Settings.OpenExportFileAfterExport,
            Settings.HideSecretsByDefault);
    }

    public SettingsGeneralSnapshot CreateDefaultGeneralSettings()
        => new(
            AppLogLevel.Information,
            true,
            true,
            true,
            (int)AppSettings.DefaultIdleTimeout.TotalMinutes,
            true,
            AppSettings.DefaultClearClipboardSeconds,
            AppSettings.DefaultQrPreviewScaleFactor,
            true,
            true,
            true);

    public async Task<SettingsPersistenceResult> SaveGeneralSettingsAsync(SettingsGeneralSnapshot snapshot)
    {
        Settings.MinimumLogLevel = snapshot.SelectedLogLevel;
        Settings.LockOnSessionLock = snapshot.LockOnSessionLock;
        Settings.LockOnMinimize = snapshot.LockOnMinimize;
        Settings.IdleTimeout = snapshot.LockOnIdleTimeout
            ? TimeSpan.FromMinutes(Math.Max(1, snapshot.IdleTimeoutMinutes))
            : TimeSpan.Zero;
        Settings.ClearClipboardEnabled = snapshot.ClearClipboardEnabled;
        Settings.ClearClipboardSeconds = snapshot.ClearClipboardSeconds;
        Settings.QrPreviewScaleFactor = Math.Clamp(
            snapshot.QrPreviewScaleFactor > 0 ? snapshot.QrPreviewScaleFactor : AppSettings.DefaultQrPreviewScaleFactor,
            MinQrPreviewScale,
            MaxQrPreviewScale);
        Settings.ExportEncrypt = snapshot.ExportEncrypt;
        Settings.OpenExportFileAfterExport = snapshot.OpenExportFileAfterExportBeforeEncrypt;
        Settings.HideSecretsByDefault = snapshot.HideSecretsByDefault;

        var saveResult = await settingsService.SaveAsync();
        if (saveResult.IsFailed)
        {
            return new SettingsPersistenceResult(false, string.Join("; ", saveResult.Errors.Select(e => e.Message)));
        }

        var logSwitchStateChanged = false;
        if (snapshot.SelectedLogLevel != logSwitchService.GetLevel())
        {
            logSwitchService.SetLevel(snapshot.SelectedLogLevel);
            logSwitchService.IsCliOverrideActive = false;
            logSwitchStateChanged = true;
        }

        qrPreviewService.PreviewScaleFactor = Settings.QrPreviewScaleFactor;
        return new SettingsPersistenceResult(true, LogSwitchStateChanged: logSwitchStateChanged);
    }
}
