using System;

namespace TOTP.Security.Models;

public sealed class GlobalProfile
{
    public static readonly TimeSpan DefaultIdleTimeout = TimeSpan.FromMinutes(10);
    public const int DefaultClearClipboardSeconds = 15;

    public AuthorizationProfile Authorization { get; set; } = new();

    public TimeSpan IdleTimeout { get; set; } = DefaultIdleTimeout;

    public bool LockOnSessionLock { get; set; } = true;

    public bool ClearClipboardEnabled { get; set; } = true;

    public int ClearClipboardSeconds { get; set; } = DefaultClearClipboardSeconds;

    public bool ExportIncludeQr { get; set; }

    public bool ExportEncrypt { get; set; } = true;

    public bool HideSecretsByDefault { get; set; } = true;
}
