using Avalonia;
using TOTP.Core.Platform;
using TOTP.Infrastructure.Services;
using TOTP.Avalonia.Desktop.Startup;
using TOTP.Camera.OpenCv;
using TOTP.Infrastructure.Logging;
using Serilog;
using System.Text.Json;

namespace TOTP.Avalonia.Desktop;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        if (args is ["--m3-native-probe"])
        {
            var probe = OpenCvNativeRuntimeProbe.Probe();
            if (!probe.IsAvailable)
                Console.Error.WriteLine($"OpenCV native probe failure: {probe.Failure}");
            Environment.ExitCode = probe.IsAvailable ? 0 : 2;
            return;
        }

        if (args is ["--m3-measurement-probe"])
        {
            var measurements = M3MeasurementProbe.Measure();
            Console.WriteLine(JsonSerializer.Serialize(measurements));
            Environment.ExitCode = measurements.NativeRuntimeAvailable ? 0 : 2;
            return;
        }

        try
        {
            var platformServices = DesktopPlatformServiceFactory.Create();
            LoggingConfigurator.SetupEarlyLogger(args, platformServices.ApplicationPaths);
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
        catch (Exception exception)
        {
            Log.Fatal(
                "Avalonia process boundary failed with exception type {ExceptionType}.",
                exception.GetType().FullName);
            Environment.ExitCode = 1;
        }
        finally
        {
            LoggingConfigurator.Shutdown();
        }
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
            .UsePlatformDetect();
    }
}
