using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using TOTP.Core.Security.Interfaces;
using TOTP.Core.Services;
using TOTP.Commands;
using TOTP.Helper;
using TOTP.Infrastructure;
using TOTP.Infrastructure.Common;
using TOTP.Infrastructure.Logging;
using TOTP.Resources;
using TOTP.Security;
using TOTP.Security.Interfaces;
using TOTP.Startup;
using TOTP.Services.Interfaces;
using TOTP.ViewModels.Interfaces;
using TOTP.Views;

namespace TOTP;

internal static class Program
{
    private sealed class StartupTiming
    {
        public Stopwatch Stopwatch { get; } = Stopwatch.StartNew();
        public long HostStartedMs { get; set; }
        public long WindowShowCalledMs { get; set; }
        public long LoadedStartMs { get; set; }
        public long SettingsLoadedMs { get; set; }
        public long MainVmInitializedMs { get; set; }
        public long LoadedEndMs { get; set; }
    }

    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            LoggingConfigurator.SetupEarlyLogger(args);
            StartApplication(args).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, UI.ex_FatalError);
            Debug.WriteLine(ex.Message);
            Environment.Exit(-1);
        }
        finally
        {
            LoggingConfigurator.Shutdown();
        }
    }

    private static async Task StartApplication(string[] args)
    {
        var timing = new StartupTiming();

        try
        {
            Log.Information("startup.begin");

            var configuration = BootLoader.BuildConfiguration();
            BootLoader.SetCulture(configuration);
            BootLoader.RegisterSyncfusionLicenseKey(configuration);

            using var instance = new SingleInstanceGuard(StringsConstants.AssemblyNameWpf);
            if (!instance.IsFirstInstance)
            {
                SingleInstanceGuard.ActivateExistingWindow(StringsConstants.AssemblyNameWpf);
                return;
            }

            using var host = BootLoader.BuildHostAndConfigureServices(configuration, args);

            var app = new App();
            app.InitializeComponent();
            BootLoader.SetupUnhandledExceptionsHooks(app, host);

            app.Exit += async (_, __) =>
            {
                try
                {
                    await host.StopAsync();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, UI.ex_UnexpectedError);
                }
            };

            var splash = new SplashWindow();
            app.MainWindow = splash;

            splash.Loaded += async (_, __) =>
            {
                try
                {
                    Log.Information("startup.splash.loaded.begin");
                    await BootstrapMainWindowFromSplashAsync(app, splash, host, timing);
                }
                catch (Exception ex)
                {
                    Log.Fatal(ex, UI.ex_FatalError);
                    app.Shutdown(-1);
                }
            };

            splash.Show();
            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, UI.ex_FatalError);
            Environment.Exit(-1);
        }
    }

    private static async Task BootstrapMainWindowFromSplashAsync(App app, SplashWindow splash, IHost host, StartupTiming timing)
    {
        await host.StartAsync();
        timing.HostStartedMs = timing.Stopwatch.ElapsedMilliseconds;
        Log.Information("startup.host.started elapsed_ms={ElapsedMs}", timing.HostStartedMs);

        CommandExceptionLogger.Initialize(host.Services.GetRequiredService<ILoggerFactory>());
        host.Services.GetRequiredService<IScannerWarmupService>().StartWarmupInBackground("program.startup");

        var mainWindow = CreateMainWindowShell(host);
        timing.LoadedStartMs = timing.Stopwatch.ElapsedMilliseconds;
        Log.Information("startup.mainwindow.loaded.begin");

        var loadResult = await host.Services.GetRequiredService<ISettingsService>().LoadAsync();
        if (loadResult.IsFailed)
        {
            throw new InvalidOperationException(string.Join("; ", loadResult.Errors.Select(e => e.Message)));
        }
        timing.SettingsLoadedMs = timing.Stopwatch.ElapsedMilliseconds;
        Log.Information("startup.settings.loaded elapsed_ms={ElapsedMs}", timing.SettingsLoadedMs - timing.LoadedStartMs);

        var vm = (IMainViewModel)mainWindow.DataContext;
        await vm.InitializeMainViewAsync(mainWindow);
        timing.MainVmInitializedMs = timing.Stopwatch.ElapsedMilliseconds;
        Log.Information("startup.mainvm.initialized elapsed_ms={ElapsedMs}", timing.MainVmInitializedMs - timing.LoadedStartMs);

        await host.Services.GetRequiredService<IAutoUpdateService>().InitializeAsync();
        timing.LoadedEndMs = timing.Stopwatch.ElapsedMilliseconds;
        Log.Information("startup.mainwindow.loaded.end elapsed_ms={ElapsedMs}", timing.LoadedEndMs - timing.LoadedStartMs);

        app.MainWindow = mainWindow;
        mainWindow.Show();
        if (mainWindow.WindowState == WindowState.Minimized)
        {
            mainWindow.WindowState = WindowState.Normal;
        }

        _ = mainWindow.Activate();

        timing.WindowShowCalledMs = timing.Stopwatch.ElapsedMilliseconds;
        Log.Information("startup.window.show.called elapsed_ms={ElapsedMs}", timing.WindowShowCalledMs);
        Log.Information(
            "startup.summary total_ms={TotalMs} host_started_ms={HostStartedMs} window_show_called_ms={WindowShowCalledMs} loaded_start_ms={LoadedStartMs} settings_loaded_ms={SettingsLoadedMs} mainvm_initialized_ms={MainVmInitializedMs} loaded_end_ms={LoadedEndMs}",
            timing.Stopwatch.ElapsedMilliseconds,
            timing.HostStartedMs,
            timing.WindowShowCalledMs,
            timing.LoadedStartMs,
            timing.SettingsLoadedMs,
            timing.MainVmInitializedMs,
            timing.LoadedEndMs);

        splash.Close();
        Log.Information("startup.splash.closed elapsed_ms={ElapsedMs}", timing.Stopwatch.ElapsedMilliseconds);
    }

    private static MainWindow CreateMainWindowShell(IHost host)
    {
        var vm = host.Services.GetRequiredService<IMainViewModel>();
        var mainWindow = host.Services.GetRequiredService<MainWindow>();
        mainWindow.DataContext = vm;
        mainWindow.ResizeMode = ResizeMode.NoResize;
        return mainWindow;
    }
}
