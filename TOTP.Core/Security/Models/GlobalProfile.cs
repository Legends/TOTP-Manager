using Serilog.Events;
using System;
using TOTP.Core.Enums;

namespace TOTP.Security.Models;

public sealed class GlobalProfile
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
