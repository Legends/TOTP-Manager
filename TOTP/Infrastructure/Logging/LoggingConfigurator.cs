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
        // Use the static switch directly since DI isn't ready
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.ControlledBy(LogSwitchService.SharedSwitch) // Access the static member
            .WriteTo.Async(a => a.File(StringsConstants.RootLogPath))
            .CreateBootstrapLogger();
    }

    /// <summary>
    /// Configures the specified logger using settings from the provided host context and service provider.
    /// the order of methods matters !!!
    /// </summary>
    /// <remarks>This method applies configuration from the host context to the logger, including minimum log
    /// level, output destinations, and file logging options. It is typically called during application startup to ensure
    /// logging is consistent with the application's configuration and environment.</remarks>
    /// <param name="context">The host builder context containing configuration and environment information used to set up the logger.</param>
    /// <param name="services">The service provider used to resolve dependencies required for logger configuration.</param>
    /// <param name="config">The logger configuration instance to be updated with settings from the host context.</param>
    public static void ConfigureWithHostContext(HostBuilderContext context, IServiceProvider services, LoggerConfiguration config)
    {
        config
            .ReadFrom.Configuration(context.Configuration) // 1. Load overrides like "Microsoft: Warning"
            .MinimumLevel.ControlledBy(LogSwitchService.SharedSwitch) // 2. FORCE the global level to use our switch
            .WriteTo.Console()
            .WriteTo.Async(a => a.File(StringsConstants.AppLogPath, rollingInterval: RollingInterval.Day));
    }
}