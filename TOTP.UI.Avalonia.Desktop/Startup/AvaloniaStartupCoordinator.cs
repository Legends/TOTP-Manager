using Microsoft.Extensions.Logging;
using TOTP.Core.Enums;
using TOTP.Core.Security.Interfaces;
using TOTP.Core.Security.Models;
using TOTP.Avalonia.Desktop.Localization;

namespace TOTP.Avalonia.Desktop.Startup;

public sealed class AvaloniaStartupCoordinator(
    ISettingsService settingsService,
    IAuthorizationService authorizationService,
    IAvaloniaLocalizationService localizationService,
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

            localizationService.ApplyCulture(settings.Value.CultureName);

            cancellationToken.ThrowIfCancellationRequested();
            await authorizationService.InitializeAsync();
            cancellationToken.ThrowIfCancellationRequested();

            if (!authorizationService.State.IsConfigured)
                return AvaloniaStartupOutcome.ReadyForPasswordSetup;

            if (authorizationService.State.PreferredUnlockMethod != PreferredUnlockMethod.PlatformQuickUnlock)
                return AvaloniaStartupOutcome.ReadyForUnlock;

            var quickUnlock = await authorizationService.TryUnlockOnStartupAsync(cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            return quickUnlock == AuthorizationResult.Success
                && authorizationService.State.IsUnlocked
                    ? AvaloniaStartupOutcome.ReadyUnlocked
                    : AvaloniaStartupOutcome.ReadyForPasswordFallback;
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
