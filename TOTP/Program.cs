using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Collections.Generic;
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
            WriteEarlyStartupTraceToFile("startup.begin");

            using var instance = new SingleInstanceGuard(StringsConstants.AssemblyNameWpf);
            if (!instance.IsFirstInstance)
            {
                SingleInstanceGuard.ActivateExistingWindow(StringsConstants.AssemblyNameWpf);
                return Task.CompletedTask;
            }

            SplashProcessHandle? splashHandle = null;
            try
            {
                splashHandle = StartSplashProcess();
                Log.Information("startup.splash.process.started");
            }
            catch (Exception ex)
            {
                // Splash is non-critical. Continue startup without it.
                Log.Warning(ex, "startup.splash.process.failed");
                WriteEarlyStartupFailureToFile("startup.splash.process.failed", ex);
            }

            var configuration = Task.Run(BootLoader.BuildConfiguration).GetAwaiter().GetResult();
            WriteEarlyStartupTraceToFile("startup.configuration.loaded");
            BootLoader.SetCulture(configuration);
            BootLoader.RegisterSyncfusionLicenseKey(configuration);

            using var host = Task.Run(() => BootLoader.BuildHostAndConfigureServices(configuration, args)).GetAwaiter().GetResult();
            WriteEarlyStartupTraceToFile("startup.host.built");

            var app = new App();
            app.InitializeComponent();
            BootLoader.SetupUnhandledExceptionsHooks(app, host);
            var hostStarted = false;

            app.Exit += async (_, __) =>
            {
                if (!hostStarted)
                {
                    return;
                }

                try { await host.StopAsync(); }
                catch (Exception ex) { Log.Error(ex, UI.ex_UnexpectedError); }
            };

            timing.MainWindowShellCreateStartMs = timing.Stopwatch.ElapsedMilliseconds;
            Log.Information("startup.mainwindow.shell.create.begin");
            var mainWindow = CreateMainWindowShell(host);
            WriteEarlyStartupTraceToFile("startup.mainwindow.created");
            timing.MainWindowShellCreateEndMs = timing.Stopwatch.ElapsedMilliseconds;
            Log.Information("startup.mainwindow.shell.create.end elapsed_ms={ElapsedMs}", timing.MainWindowShellCreateEndMs - timing.MainWindowShellCreateStartMs);
            var vm = (IMainViewModel)mainWindow.DataContext;

            var postShowInitializationStarted = false;
            mainWindow.Loaded += async (_, __) =>
            {
                if (postShowInitializationStarted)
                {
                    return;
                }

                postShowInitializationStarted = true;

                try
                {
                    timing.LoadedStartMs = timing.Stopwatch.ElapsedMilliseconds;
                    Log.Information("startup.mainwindow.loaded.begin");

                    var loadResult = await host.Services.GetRequiredService<ISettingsService>().LoadAsync();
                    if (loadResult.IsFailed)
                    {
                        throw new InvalidOperationException(string.Join("; ", loadResult.Errors.Select(e => e.Message)));
                    }

                    timing.SettingsLoadedMs = timing.Stopwatch.ElapsedMilliseconds;
                    Log.Information("startup.settings.loaded elapsed_ms={ElapsedMs}", timing.SettingsLoadedMs - timing.LoadedStartMs);

                    await vm.InitializeMainViewAsync(mainWindow);
                    WriteEarlyStartupTraceToFile("startup.mainvm.initialized");
                    timing.MainVmInitializedMs = timing.Stopwatch.ElapsedMilliseconds;
                    Log.Information("startup.mainvm.initialized elapsed_ms={ElapsedMs}", timing.MainVmInitializedMs - timing.LoadedStartMs);

                    await host.StartAsync();
                    hostStarted = true;
                    timing.HostStartedMs = timing.Stopwatch.ElapsedMilliseconds;
                    Log.Information("startup.host.started elapsed_ms={ElapsedMs}", timing.HostStartedMs);

                    CommandExceptionLogger.Initialize(host.Services.GetRequiredService<ILoggerFactory>());
                    host.Services.GetRequiredService<IScannerWarmupService>().StartWarmupInBackground("program.startup");

                    await host.Services.GetRequiredService<IAutoUpdateService>().InitializeAsync();
                    WriteEarlyStartupTraceToFile("startup.autoupdate.initialized");
                    timing.LoadedEndMs = timing.Stopwatch.ElapsedMilliseconds;
                    Log.Information("startup.mainwindow.loaded.end elapsed_ms={ElapsedMs}", timing.LoadedEndMs - timing.LoadedStartMs);
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
                }
                catch (Exception ex)
                {
                    WriteEarlyStartupFailureToFile("startup.postshow.fatal.exception", ex);
                    Log.Fatal(ex, UI.ex_FatalError);
                    app.Shutdown(-1);
                }
            };

            app.MainWindow = mainWindow;
            splashHandle?.CloseSplash();
            Log.Information("startup.splash.process.closed elapsed_ms={ElapsedMs}", timing.Stopwatch.ElapsedMilliseconds);
            mainWindow.Show();
            if (mainWindow.WindowState == WindowState.Minimized)
            {
                mainWindow.WindowState = WindowState.Normal;
            }
            _ = mainWindow.Activate();

            timing.WindowShowCalledMs = timing.Stopwatch.ElapsedMilliseconds;
            Log.Information("startup.window.show.called elapsed_ms={ElapsedMs}", timing.WindowShowCalledMs);

            app.Run();
            splashHandle?.Dispose();
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            WriteEarlyStartupFailureToFile("startup.fatal.exception", ex);
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
        var cmd0 = Environment.GetCommandLineArgs().FirstOrDefault();
        var commandLineDir = Path.GetDirectoryName(cmd0);
        var processDir = Path.GetDirectoryName(Environment.ProcessPath);
        var currentDir = Directory.GetCurrentDirectory();

        var candidates = new List<string>();
        if (!string.IsNullOrWhiteSpace(commandLineDir))
        {
            candidates.Add(Path.Combine(commandLineDir, "TOTP.Splash.exe"));
        }

        if (!string.IsNullOrWhiteSpace(processDir))
        {
            candidates.Add(Path.Combine(processDir, "TOTP.Splash.exe"));
        }

        if (!string.IsNullOrWhiteSpace(currentDir))
        {
            candidates.Add(Path.Combine(currentDir, "TOTP.Splash.exe"));
        }

        candidates.Add(Path.Combine(AppContext.BaseDirectory, "TOTP.Splash.exe"));

        var existingCandidates = candidates
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Where(File.Exists)
            .ToList();

        var splashExePath = existingCandidates
            .FirstOrDefault(path => !path.Contains("\\AppData\\Local\\Temp\\.net\\", StringComparison.OrdinalIgnoreCase))
            ?? existingCandidates.FirstOrDefault()
            ?? string.Empty;

        WriteEarlyStartupTraceToFile(
            $"startup.splash.path.select cmd0={cmd0} process_dir={processDir} current_dir={currentDir} appcontext_base={AppContext.BaseDirectory} selected={splashExePath} existing={string.Join(";", existingCandidates)}");

        if (!File.Exists(splashExePath))
        {
            Log.Warning("startup.splash.process.missing path={Path} process_dir={ProcessDir} appcontext_base={AppContextBaseDirectory}", splashExePath, processDir, AppContext.BaseDirectory);
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

    private static void WriteEarlyStartupFailureToFile(string message, Exception ex)
    {
        try
        {
            Directory.CreateDirectory(StringsConstants.AppLogDirectoryPath);
            var line =
                $"{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss.fff zzz} [WRN] {message}{Environment.NewLine}{ex}{Environment.NewLine}";
            File.AppendAllText(StringsConstants.AppLogFilePath, line);
        }
        catch
        {
            // Best-effort fallback logging only.
        }
    }

    private static void WriteEarlyStartupTraceToFile(string message)
    {
        try
        {
            Directory.CreateDirectory(StringsConstants.AppLogDirectoryPath);
            var line = $"{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss.fff zzz} [INF] {message}{Environment.NewLine}";
            File.AppendAllText(StringsConstants.AppLogFilePath, line);
        }
        catch
        {
            // Best-effort fallback logging only.
        }
    }
}
