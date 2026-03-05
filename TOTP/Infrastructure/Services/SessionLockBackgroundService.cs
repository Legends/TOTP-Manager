using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TOTP.Core.Security.Interfaces;
using TOTP.Security.Interfaces;

namespace TOTP.Infrastructure.Services;

/// <summary>
/// Provides a background service that locks the application when the Windows session is locked.
/// </summary>
/// <remarks>SessionLockService monitors Windows session lock events and triggers application locking through the
/// provided authorization service. This service is intended to enhance security by ensuring the application is locked
/// whenever the user's session is locked. The service subscribes to session events for the duration of its lifetime and
/// unsubscribes during shutdown to prevent resource leaks. Thread safety and proper event unsubscription are handled
/// internally.(formerly:app.xaml.cs)</remarks>
public sealed class SessionLockBackgroundService : BackgroundService
{
    private readonly IAuthorizationService _authorizationService;
    private readonly ISettingsService _settingsService;
    private readonly ILogger<SessionLockBackgroundService> _logger;

    public SessionLockBackgroundService(
        IAuthorizationService authorizationService,
        ISettingsService settingsService,
        ILogger<SessionLockBackgroundService> logger)
    {
        _authorizationService = authorizationService;
        _settingsService = settingsService;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // We don't need a loop here, just subscribe to the event
        SystemEvents.SessionSwitch += OnSessionSwitch;

        // BackgroundService expects a Task that represents the lifetime. 
        // We return a Task that completes only when the cancellation token is triggered.
        return Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private void OnSessionSwitch(object sender, SessionSwitchEventArgs e)
    {
        if (e.Reason == SessionSwitchReason.SessionLock)
        {
            if (!_settingsService.Current.LockOnSessionLock)
            {
                return;
            }

            try
            {
                _logger.LogInformation("Windows session locked. Locking application.");
                _authorizationService.Lock();
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Failed to lock application during session switch.");
            }
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        // CRITICAL: Unsubscribe to prevent memory leaks and crashes during shutdown
        SystemEvents.SessionSwitch -= OnSessionSwitch;
        await base.StopAsync(cancellationToken);
    }
}
