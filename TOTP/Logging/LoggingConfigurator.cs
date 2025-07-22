using Microsoft.Extensions.Hosting;
using Serilog;
using System;

namespace TOTP.Manager.Logging;

public static class LoggingConfigurator
{
    public static void SetupEarlyLogger() =>
        Log.Logger = new LoggerConfiguration()
            .WriteTo.File("Logs/app-root-start.log")
            .CreateLogger();

    public static void ConfigureWithHostContext(HostBuilderContext context, IServiceProvider services, LoggerConfiguration config) =>
        config
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File("Logs/app.log", rollingInterval: RollingInterval.Day);
}
