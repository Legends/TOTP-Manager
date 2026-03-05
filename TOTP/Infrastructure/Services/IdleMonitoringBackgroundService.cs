using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TOTP.Core.Models;
using TOTP.Core.Security.Interfaces;

namespace TOTP.Infrastructure.Services;

public sealed class IdleMonitoringBackgroundService : BackgroundService, IActivityHeartbeat
{
    private readonly IAuthorizationService _authService;
    private readonly ISettingsService _settingsService;
    private readonly ILogger<IdleMonitoringBackgroundService> _logger;

    private bool _wasUnlocked;

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
        var profileResult = await _settingsService.LoadAsync();
        if (profileResult.IsFailed)
        {
            _logger.LogWarning("Failed to load settings for idle monitoring. Using in-memory defaults.");
        }

        // Periodic check every 5 seconds is sufficient for idle timeout.
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            var isUnlocked = _authService.State.IsUnlocked;

            // Freshly unlocked session should start a new idle window.
            if (isUnlocked && !_wasUnlocked)
            {
                RecordActivity();
            }

            _wasUnlocked = isUnlocked;

            if (!isUnlocked)
            {
                continue;
            }

            var timeout = _settingsService.Current?.IdleTimeout ?? AppSettings.DefaultIdleTimeout;
            if (timeout <= TimeSpan.Zero)
            {
                continue;
            }

            if (DateTime.UtcNow - LastActivity >= timeout)
            {
                _logger.LogInformation("Idle timeout reached ({Timeout}). Locking app.", timeout);
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
