using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
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
    private const string SplashTokenArg = "--splash-token";
    private const string SplashParentPidArg = "--splash-parent-pid";

    private sealed class StartupTiming
    {
        public Stopwatch Stopwatch { get; } = Stopwatch.StartNew();
        public long HostStartedMs { get; set; }
        public long MainWindowShellCreateStartMs { get; set; }
        public long MainWindowShellCreateEndMs { get; set; }
        public long WindowShowCalledMs { get; set; }
        public long LoadedStartMs { get; set; }
        public long SettingsLoadedMs { get; set; }
        public long MainVmInitializedMs { get; set; }
        public long LoadedEndMs { get; set; }
    }

    private sealed class SplashProcessHandle : IDisposable
    {
        private readonly EventWaitHandle _closeEvent;
        private readonly Process _process;
        private int _closed;

        public SplashProcessHandle(EventWaitHandle closeEvent, Process process)
        {
            _closeEvent = closeEvent;
            _process = process;
        }

        public void CloseSplash()
        {
            if (Interlocked.Exchange(ref _closed, 1) == 1)
            {
                return;
            }

            try
            {
                _closeEvent.Set();
                if (!_process.HasExited)
                {
                    _process.WaitForExit(1500);
                }
            }
            catch { }
        }

        public void Dispose()
        {
            CloseSplash();
            _closeEvent.Dispose();
            _process.Dispose();
        }
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

    private static Task StartApplication(string[] args)
    {
        var timing = new StartupTiming();

        try
        {
            Log.Information("startup.begin");

            using var instance = new SingleInstanceGuard(StringsConstants.AssemblyNameWpf);
            if (!instance.IsFirstInstance)
            {
                SingleInstanceGuard.ActivateExistingWindow(StringsConstants.AssemblyNameWpf);
                return Task.CompletedTask;
            }

            using var splashHandle = StartSplashProcess();
            Log.Information("startup.splash.process.started");

            var configuration = Task.Run(BootLoader.BuildConfiguration).GetAwaiter().GetResult();
            BootLoader.SetCulture(configuration);
            BootLoader.RegisterSyncfusionLicenseKey(configuration);

            using var host = Task.Run(() => BootLoader.BuildHostAndConfigureServices(configuration, args)).GetAwaiter().GetResult();
            host.StartAsync().GetAwaiter().GetResult();
            timing.HostStartedMs = timing.Stopwatch.ElapsedMilliseconds;
            Log.Information("startup.host.started elapsed_ms={ElapsedMs}", timing.HostStartedMs);

            CommandExceptionLogger.Initialize(host.Services.GetRequiredService<ILoggerFactory>());
            host.Services.GetRequiredService<IScannerWarmupService>().StartWarmupInBackground("program.startup");

            var app = new App();
            app.InitializeComponent();
            BootLoader.SetupUnhandledExceptionsHooks(app, host);

            app.Exit += async (_, __) =>
            {
                try { await host.StopAsync(); }
                catch (Exception ex) { Log.Error(ex, UI.ex_UnexpectedError); }
            };

            timing.MainWindowShellCreateStartMs = timing.Stopwatch.ElapsedMilliseconds;
            Log.Information("startup.mainwindow.shell.create.begin");
            var mainWindow = CreateMainWindowShell(host);
            timing.MainWindowShellCreateEndMs = timing.Stopwatch.ElapsedMilliseconds;
            Log.Information("startup.mainwindow.shell.create.end elapsed_ms={ElapsedMs}", timing.MainWindowShellCreateEndMs - timing.MainWindowShellCreateStartMs);

            timing.LoadedStartMs = timing.Stopwatch.ElapsedMilliseconds;
            Log.Information("startup.mainwindow.loaded.begin");

            var loadResult = host.Services.GetRequiredService<ISettingsService>().LoadAsync().GetAwaiter().GetResult();
            if (loadResult.IsFailed)
            {
                throw new InvalidOperationException(string.Join("; ", loadResult.Errors.Select(e => e.Message)));
            }

            timing.SettingsLoadedMs = timing.Stopwatch.ElapsedMilliseconds;
            Log.Information("startup.settings.loaded elapsed_ms={ElapsedMs}", timing.SettingsLoadedMs - timing.LoadedStartMs);

            var vm = (IMainViewModel)mainWindow.DataContext;
            vm.InitializeMainViewAsync(mainWindow).GetAwaiter().GetResult();
            timing.MainVmInitializedMs = timing.Stopwatch.ElapsedMilliseconds;
            Log.Information("startup.mainvm.initialized elapsed_ms={ElapsedMs}", timing.MainVmInitializedMs - timing.LoadedStartMs);

            host.Services.GetRequiredService<IAutoUpdateService>().InitializeAsync().GetAwaiter().GetResult();
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
                "startup.summary total_ms={TotalMs} host_started_ms={HostStartedMs} shell_create_start_ms={ShellCreateStartMs} shell_create_end_ms={ShellCreateEndMs} shell_create_elapsed_ms={ShellCreateElapsedMs} window_show_called_ms={WindowShowCalledMs} loaded_start_ms={LoadedStartMs} settings_loaded_ms={SettingsLoadedMs} mainvm_initialized_ms={MainVmInitializedMs} loaded_end_ms={LoadedEndMs}",
                timing.Stopwatch.ElapsedMilliseconds,
                timing.HostStartedMs,
                timing.MainWindowShellCreateStartMs,
                timing.MainWindowShellCreateEndMs,
                timing.MainWindowShellCreateEndMs - timing.MainWindowShellCreateStartMs,
                timing.WindowShowCalledMs,
                timing.LoadedStartMs,
                timing.SettingsLoadedMs,
                timing.MainVmInitializedMs,
                timing.LoadedEndMs);

            splashHandle.CloseSplash();
            Log.Information("startup.splash.process.closed elapsed_ms={ElapsedMs}", timing.Stopwatch.ElapsedMilliseconds);

            app.Run();
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, UI.ex_FatalError);
            Environment.Exit(-1);
            return Task.CompletedTask;
        }
    }

    private static MainWindow CreateMainWindowShell(IHost host)
    {
        var vm = host.Services.GetRequiredService<IMainViewModel>();
        var mainWindow = host.Services.GetRequiredService<MainWindow>();
        mainWindow.DataContext = vm;
        mainWindow.ResizeMode = ResizeMode.NoResize;
        return mainWindow;
    }

    private static SplashProcessHandle StartSplashProcess()
    {
        var splashExePath = Path.Combine(AppContext.BaseDirectory, "TOTP.Splash.exe");
        if (!File.Exists(splashExePath))
        {
            Log.Warning("startup.splash.process.missing path={Path}", splashExePath);
            throw new FileNotFoundException("Splash executable not found.", splashExePath);
        }

        var token = $"Local\\TOTP-SplashClose-{Guid.NewGuid():N}";
        var closeEvent = new EventWaitHandle(false, EventResetMode.ManualReset, token);

        var psi = new ProcessStartInfo
        {
            FileName = splashExePath,
            Arguments = $"{SplashTokenArg} \"{token}\" {SplashParentPidArg} {Environment.ProcessId}",
            UseShellExecute = false,
            CreateNoWindow = false,
            WindowStyle = ProcessWindowStyle.Normal
        };

        var process = Process.Start(psi) ?? throw new InvalidOperationException("Failed to launch splash process.");
        return new SplashProcessHandle(closeEvent, process);
    }
}
