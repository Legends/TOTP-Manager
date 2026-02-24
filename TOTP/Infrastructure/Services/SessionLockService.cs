using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TOTP.Security.Interfaces;

namespace TOTP.Infrastructure.Services;

public sealed class SessionLockService : BackgroundService
{
    private readonly IAuthorizationService _authorizationService;
    private readonly ILogger<SessionLockService> _logger;

    public SessionLockService(
        IAuthorizationService authorizationService,
        ILogger<SessionLockService> logger)
    {
        _authorizationService = authorizationService;
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
