using System;

namespace TOTP.Security.Models;

public sealed class GlobalProfile
{
    public static readonly TimeSpan DefaultIdleTimeout = TimeSpan.FromMinutes(10);

    public AuthorizationProfile Authorization { get; set; } = new();

    public TimeSpan IdleTimeout { get; set; } = DefaultIdleTimeout;
}
