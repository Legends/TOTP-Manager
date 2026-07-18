using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TOTP.Core.Platform;
using TOTP.Core.Security.Interfaces;

namespace TOTP.Infrastructure.Services;

public sealed class SessionLockPolicyBackgroundService(
    IPlatformSessionEventSource sessionEvents,
    IAuthorizationService authorizationService,
    ISettingsService settingsService,
    ILogger<SessionLockPolicyBackgroundService> logger) : BackgroundService
{
    public event EventHandler? ApplicationLocked;

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        sessionEvents.SessionChanged += OnSessionChanged;
        try
        {
            sessionEvents.Start();
        }
        catch
        {
            sessionEvents.SessionChanged -= OnSessionChanged;
            throw;
        }

        return Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private void OnSessionChanged(object? sender, PlatformSessionChangedEventArgs args)
    {
        if (args.State != PlatformSessionState.Locked || !settingsService.Current.LockOnSessionLock)
        {
            return;
        }

        try
        {
            logger.LogInformation("Platform session locked. Locking application.");
            authorizationService.Lock();
            ApplicationLocked?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception exception)
        {
            logger.LogCritical(
                "Failed to lock application after the platform session was locked. Exception type: {ExceptionType}.",
                exception.GetType().FullName);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            sessionEvents.Stop();
        }
        finally
        {
            sessionEvents.SessionChanged -= OnSessionChanged;
            await base.StopAsync(cancellationToken);
        }
    }
}
