using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TOTP.Infrastructure.Services;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using TOTP.Core.Security.Interfaces;
using TOTP.Security.Interfaces;

public sealed class IdleMonitoringBackgroundService : BackgroundService, IActivityHeartbeat
{
    private readonly IAuthorizationService _authService;
    private readonly ISettingsService _settingsService;
    private readonly ILogger<IdleMonitoringBackgroundService> _logger;

    public DateTime LastActivity { get; private set; } = DateTime.UtcNow;

    public IdleMonitoringBackgroundService(
        IAuthorizationService authService,
        ISettingsService settingsService,
        ILogger<IdleMonitoringBackgroundService> logger)
    {
        _authService = authService;
        _settingsService = settingsService;
        _logger = logger;
    }

    public void RecordActivity() => LastActivity = DateTime.UtcNow;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {

        // We DONT reload profile in case user changed timeout in settings
        var profileResult = await _settingsService.LoadAsync();
        if (profileResult.IsFailed)
        {
            _logger.LogWarning("Failed to load settings for idle monitoring. Using default timeout.");
        }

        var timeout = profileResult.IsSuccess
            ? profileResult.Value.IdleTimeout
            : TimeSpan.FromMinutes(10);

        // Periodic check every 5 seconds is plenty for idle timeout
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            if (!_authService.State.IsUnlocked) continue;

            if (DateTime.UtcNow - LastActivity > timeout)
            {
                _logger.LogInformation("Idle timeout reached ({Timeout}). Locking app.", timeout);

                // Jump to UI thread only for the final lock call if necessary, 
                // but usually, AuthService should be thread-safe.
                _authService.Lock();
            }
        }
    }
}


public interface IActivityHeartbeat
    {
        void RecordActivity();
        DateTime LastActivity { get; }
    }

