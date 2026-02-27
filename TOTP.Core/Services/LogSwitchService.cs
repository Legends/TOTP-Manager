using Serilog.Core;
using Serilog.Events;
using TOTP.Core.Services.Interfaces;

namespace TOTP.Core.Services;

/*
 *  Level	    Value	Usage
    ----------------------------------------------------------------
    Verbose	    0	    Everything. Absolute "firehose" of data.
    Debug	    1	    Internal control flow and state changes.
    Information	2	    General app flow (e.g., "Service Started").
    Warning	    3	    Handled errors or suspicious behavior.
    Error	    4	    Crashes/Exceptions that affect a feature.
    Fatal	    5	    Total app failure. App must shut down.
 */

/// <summary>
/// Provides centralized control over the application's logging level, allowing dynamic adjustment of log verbosity at
/// runtime.
/// </summary>
/// <remarks>LogSwitchService exposes a shared logging level switch that can be used to coordinate log filtering
/// across multiple components. Changing the logging level affects which log events are emitted, ranging from verbose
/// diagnostic information to critical errors. This service is intended for scenarios where consistent log level
/// management is required throughout the application.</remarks>
public class LogSwitchService : ILogSwitchService
{
    // This is what the LoggingConfigurator is looking for
    public static LoggingLevelSwitch SharedSwitch { get; private set; } = new LoggingLevelSwitch(LogEventLevel.Information);

    public bool IsCliOverrideActive
    {
        get;
        set;
    }

    // Implement the interface property by returning the static one
    public LoggingLevelSwitch ControlSwitch => SharedSwitch;

    public LogSwitchService(LogEventLevel initialLevel, bool isCliOverride)
    {
        IsCliOverrideActive = isCliOverride;
        SharedSwitch.MinimumLevel = initialLevel;
    }

    public void SetLevel(LogEventLevel level)
    {
        SharedSwitch.MinimumLevel = level;
    }

    public LogEventLevel GetLevel() => SharedSwitch.MinimumLevel;
}



