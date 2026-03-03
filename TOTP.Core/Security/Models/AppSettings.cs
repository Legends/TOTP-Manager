using TOTP.Core.Enums;
using TOTP.Core.Security.Interfaces;

namespace TOTP.Core.Security.Models;

public sealed class AppSettings : IAppSettings
{
    public static readonly TimeSpan DefaultIdleTimeout = TimeSpan.FromMinutes(10);
    public const int DefaultClearClipboardSeconds = 15;

    public AppLogLevel MinimumLogLevel { get; set; } = AppLogLevel.Information;

    public AuthorizationProfile Authorization { get; set; } = new();

    public TimeSpan IdleTimeout { get; set; } = DefaultIdleTimeout;

    public bool LockOnSessionLock { get; set; } = true;

    public bool ClearClipboardEnabled { get; set; } = true;

    public int ClearClipboardSeconds { get; set; } = DefaultClearClipboardSeconds;

    public bool ExportEncrypt { get; set; } = true;

    public bool HideSecretsByDefault { get; set; } = true;
}
