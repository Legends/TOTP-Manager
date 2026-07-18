using Avalonia;
using TOTP.Core.Platform;
using TOTP.Infrastructure.Services;
using TOTP.Avalonia.Desktop.Startup;

namespace TOTP.Avalonia.Desktop;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        using var instance = new SingleInstanceCoordinator(
            new NamedMutexInstanceLock(DesktopInstanceIdentity.MutexName),
            new NamedPipeActivationDispatcher(DesktopInstanceIdentity.PipeName));
        var outcome = instance.Start(ApplicationActivationRequest.ActivateMainWindow());
        if (outcome == SingleInstanceOutcome.ActivationRedirected) return;
        if (outcome == SingleInstanceOutcome.ActivationFailed)
        {
            Environment.ExitCode = 1;
            return;
        }

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
            .UsePlatformDetect();
    }
}
