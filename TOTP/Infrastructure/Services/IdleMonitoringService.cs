using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TOTP.Infrastructure.Services;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using TOTP.Security.Interfaces;

public sealed class IdleMonitoringService : BackgroundService, IActivityHeartbeat
{
    private readonly IAuthorizationService _authService;
    private readonly IGlobalProfileStore _profileStore;
    private readonly ILogger<IdleMonitoringService> _logger;

    public DateTime LastActivity { get; private set; } = DateTime.UtcNow;

    public IdleMonitoringService(
        IAuthorizationService authService,
        IGlobalProfileStore profileStore,
        ILogger<IdleMonitoringService> logger)
    {
        _authService = authService;
        _profileStore = profileStore;
        _logger = logger;
    }

    public void RecordActivity() => LastActivity = DateTime.UtcNow;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {

        // We DONT reload profile in case user changed timeout in settings
        var profile = await _profileStore.LoadAsync();
        var timeout = profile?.IdleTimeout ?? TimeSpan.FromMinutes(10);

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

