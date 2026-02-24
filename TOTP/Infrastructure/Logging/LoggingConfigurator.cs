using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using TOTP.Core.Services;
using TOTP.Helper;

namespace TOTP.Infrastructure.Logging;

public static class LoggingConfigurator
{
    public static void SetupEarlyLogger()
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Async(a => a.File(StringsConstants.RootLogPath))
            .CreateLogger();
    }

    public static void ConfigureWithHostContext(HostBuilderContext context, IServiceProvider services, LoggerConfiguration config)
    {
        var loggingService = services.GetRequiredService<ILoggingService>();

        config
            .MinimumLevel.ControlledBy(loggingService.ControlSwitch)
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.Async(a => a.File(StringsConstants.AppLogPath, rollingInterval: RollingInterval.Day));
    }
}