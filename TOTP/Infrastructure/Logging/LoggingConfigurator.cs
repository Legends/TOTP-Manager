using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using TOTP.Core.Services;
using TOTP.Helper;

namespace TOTP.Infrastructure.Logging;

/// <summary>
/// Provides methods and properties for configuring application logging, including support for command-line log level
/// overrides and integration with host-based logging setups.
/// </summary>
/// <remarks>This static class enables early and final logger configuration for applications that use Serilog. It
/// supports command-line overrides for log levels and ensures consistent logging behavior throughout the application's
/// lifecycle. Use this class to initialize logging both before and after the host is fully constructed.</remarks>
public static class LoggingConfigurator
{
    // Allows other services to check if the user is forcing a specific log level
    public static LogEventLevel? ManualOverrideLevel { get; private set; }
    
    private static readonly Dictionary<string, LogEventLevel> _levelMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { "-v", LogEventLevel.Verbose }, { "--verbose", LogEventLevel.Verbose },
        { "-d", LogEventLevel.Debug },   { "--debug", LogEventLevel.Debug },
        { "-i", LogEventLevel.Information }, { "--info", LogEventLevel.Information },
        { "-w", LogEventLevel.Warning }, { "--warn", LogEventLevel.Warning },
        { "-e", LogEventLevel.Error },   { "--error", LogEventLevel.Error },
        { "-f", LogEventLevel.Fatal },   { "--fatal", LogEventLevel.Fatal }
    };

    public static LogEventLevel? GetLevelFromArgs(string[] args)
    {
        if (args == null || args.Length == 0) return null;

        // Find the first argument that exists in our map
        foreach (var arg in args)
        {
            if (_levelMap.TryGetValue(arg, out var level))
            {
                return level;
            }
        }

        return null;
    }
    public static void SetupEarlyLogger(string[] args)
    {
        foreach (var arg in args)
        {
            if (_levelMap.TryGetValue(arg, out var level))
            {
                ManualOverrideLevel = level;
                LogSwitchService.SharedSwitch.MinimumLevel = level;
                break;
            }
        }

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.ControlledBy(LogSwitchService.SharedSwitch)
            .WriteTo.Async(a => a.File(StringsConstants.AppLogPath))
            .CreateBootstrapLogger();

        if (ManualOverrideLevel.HasValue)
        {
            Log.Write(ManualOverrideLevel.Value, "Early Logger initialized with CLI override: {Level}", ManualOverrideLevel);
        }
    }

    /// <summary>
    /// This is the method your BootLoader is looking for. 
    /// It configures the final logger when the Host is ready.
    /// </summary>
    public static void ConfigureWithHostContext(HostBuilderContext context, IServiceProvider services, LoggerConfiguration config)
    {
        config
            .ReadFrom.Configuration(context.Configuration) // 1. Load basic settings
            .MinimumLevel.ControlledBy(LogSwitchService.SharedSwitch) // 2. Re-attach the master switch
            .WriteTo.Console()
            .WriteTo.Async(a => a.File(StringsConstants.AppLogPath, rollingInterval: RollingInterval.Day));

        if (ManualOverrideLevel.HasValue)
        {
            Log.Write(ManualOverrideLevel.Value, "Host logger configured. Maintaining CLI override: {Level}", ManualOverrideLevel);
        }
    }
}