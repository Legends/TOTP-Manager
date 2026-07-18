using Microsoft.Extensions.Logging;
using TOTP.Core.Security.Interfaces;

namespace TOTP.Avalonia.Desktop.Startup;

public sealed class AvaloniaStartupCoordinator(
    ISettingsService settingsService,
    IAuthorizationService authorizationService,
    ILogger<AvaloniaStartupCoordinator> logger) : IAvaloniaStartupCoordinator
{
    public async Task<AvaloniaStartupOutcome> InitializeAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var settings = await settingsService.LoadAsync();
            if (settings.IsFailed)
            {
                logger.LogError("Avalonia startup could not load application preferences.");
                return AvaloniaStartupOutcome.PreferencesUnavailable;
            }

            cancellationToken.ThrowIfCancellationRequested();
            await authorizationService.InitializeAsync();
            cancellationToken.ThrowIfCancellationRequested();

            return authorizationService.State.IsConfigured
                ? AvaloniaStartupOutcome.ReadyForUnlock
                : AvaloniaStartupOutcome.ReadyForPasswordSetup;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            // Do not log exception messages here: boundary exceptions may contain
            // paths or user-controlled data. The type and stage are sufficient.
            logger.LogError(
                "Avalonia startup failed with exception type {ExceptionType}.",
                exception.GetType().FullName);
            return AvaloniaStartupOutcome.UnexpectedFailure;
        }
    }
}
