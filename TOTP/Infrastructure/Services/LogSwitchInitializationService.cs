using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TOTP.Core.Services;
using TOTP.Infrastructure.Logging;
using TOTP.Security.Interfaces;

namespace TOTP.Infrastructure.Services;
 
     
    public sealed class LogSwitchInitializationService(
        IGlobalProfileStore profileStore,
        ILogSwitchService logSwitch,
        ILogger<LogSwitchInitializationService> logger)
        : BackgroundService
    {
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            // If a CLI flag was used, we ignore the profile's log level entirely.
            if (LoggingConfigurator.ManualOverrideLevel.HasValue)
            {
                var level = LoggingConfigurator.ManualOverrideLevel.Value;

                // We manually write to the static Log class to ensure it bypasses 
                // any specific category filters if they existed.
                Log.Write(level, "CLI Override active. Level: {Level}", level);
            }
            else
            {
                var profile = await profileStore.LoadAsync();
                if (profile != null)
                {
                    var level = profile.MinimumLogLevel;
                    logSwitch.SetLevel(level);
                    //logger.LogDebug("Log level synchronized with Global Profile: {Level}", level);
                    Log.Write(LogEventLevel.Fatal, "LogSwitchInitializationService.cs: Log level synchronized with Global Profile: {Level}", level);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to synchronize log level from profile.");
        }
    }
}