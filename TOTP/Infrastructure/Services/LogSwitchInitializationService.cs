using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TOTP.Core.Services;
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
                var profile = await profileStore.LoadAsync();
                if (profile != null)
                {
                var level = profile?.MinimumLogLevel ?? LogEventLevel.Information;
                logSwitch.SetLevel(level);
                logger.LogInformation("Global profile loaded and log level set to {Level}", profile.MinimumLogLevel);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to initialize app settings from profile.");
            }
        }
    }