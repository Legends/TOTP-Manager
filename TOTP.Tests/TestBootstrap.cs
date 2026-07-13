using Microsoft.Extensions.Configuration;
using Syncfusion.Licensing;
using System;
using System.Runtime.CompilerServices;

namespace TOTP.Tests;

internal static class TestBootstrap
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        RegisterSyncfusionLicense();
    }

    internal static void RegisterSyncfusionLicense()
    {
        var key = Environment.GetEnvironmentVariable("SYNCFUSION_LICENSE");
        if (string.IsNullOrWhiteSpace(key))
        {
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets(typeof(TestBootstrap).Assembly, optional: true)
                .Build();
            key = configuration["syncfusion"];
        }

        if (string.IsNullOrWhiteSpace(key))
        {
            if (string.Equals(Environment.GetEnvironmentVariable("CI"), "true", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "SYNCFUSION_LICENSE must be configured before running WPF tests in CI.");
            }

            return;
        }

        SyncfusionLicenseProvider.RegisterLicense(key);
    }
}
