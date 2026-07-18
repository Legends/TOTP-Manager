using Avalonia;
using TOTP.Core.Platform;
using TOTP.Infrastructure.Services;
using TOTP.Avalonia.Desktop.Startup;
using TOTP.Camera.OpenCv;

namespace TOTP.Avalonia.Desktop;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        if (args is ["--m3-native-probe"])
        {
            Environment.ExitCode = OpenCvNativeRuntimeProbe.Probe().IsAvailable ? 0 : 2;
            return;
        }

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
