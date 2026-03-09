using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;
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

    private sealed class StartupStepRecorder
    {
        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
        private readonly List<(int Sequence, string Step, long DeltaMs, long TotalMs)> _steps = [];
        private readonly object _sync = new();
        private int _sequence;
        private long _lastMs;

        public void Mark(string step)
        {
            var now = _stopwatch.ElapsedMilliseconds;
            lock (_sync)
            {
                var delta = now - _lastMs;
                _lastMs = now;
                _sequence++;
                _steps.Add((_sequence, step, delta, now));
            }
        }

        public string BuildTable(string title)
        {
            lock (_sync)
            {
                var sb = new StringBuilder();
                sb.AppendLine(title);
                sb.AppendLine("seq | step                                 | +ms   | total_ms");
                sb.AppendLine("----+--------------------------------------+-------+---------");
                foreach (var item in _steps)
                {
                    sb.AppendLine($"{item.Sequence,3} | {item.Step,-36} | {item.DeltaMs,5} | {item.TotalMs,8}");
                }

                return sb.ToString();
            }
        }
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

        public void SignalClose()
        {
            if (Interlocked.Exchange(ref _closed, 1) == 1)
            {
                return;
            }

            try
            {
                _closeEvent.Set();
            }
            catch { }
        }

        public void Dispose()
        {
            SignalClose();
            try
            {
                if (!_process.HasExited)
                {
                    _process.WaitForExit(1500);
                }
            }
            catch { }
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
        var startupSteps = new StartupStepRecorder();
        var startupTableLogged = 0;

        void EmitStartupTable(bool isError, string title)
        {
            if (Interlocked.Exchange(ref startupTableLogged, 1) == 1)
            {
                return;
            }

            var table = startupSteps.BuildTable(title);
            if (isError)
            {
                Log.Error("{StartupStepsTable}", table);
                return;
            }

            Log.Information("{StartupStepsTable}", table);
        }

        try
        {
            Log.Information("startup.begin");
            startupSteps.Mark("startup.begin");

            using var instance = new SingleInstanceGuard(StringsConstants.AssemblyNameWpf);
            if (!instance.IsFirstInstance)
            {
                startupSteps.Mark("single_instance.redirect_existing");
                SingleInstanceGuard.ActivateExistingWindow(StringsConstants.AssemblyNameWpf);
                EmitStartupTable(isError: false, "Startup Steps (redirected)");
                return Task.CompletedTask;
            }

            startupSteps.Mark("single_instance.first_instance");

            SplashProcessHandle? splashHandle = null;
            try
            {
                splashHandle = StartSplashProcess();
                startupSteps.Mark("splash.process.started");
                Log.Information("startup.splash.process.started");
            }
            catch (Exception ex)
            {
                // Splash is non-critical. Continue startup without it.
                startupSteps.Mark("splash.process.failed");
                Log.Warning(ex, "startup.splash.process.failed");
            }

            var configuration = BootLoader.BuildConfiguration();
            startupSteps.Mark("configuration.built");
            BootLoader.SetCulture(configuration);
            startupSteps.Mark("culture.set");
            BootLoader.RegisterSyncfusionLicenseKey(configuration);
            startupSteps.Mark("syncfusion.license.registered");

            using var host = BootLoader.BuildHostAndConfigureServices(configuration, args);
            startupSteps.Mark("host.built");

            var app = new App();
            startupSteps.Mark("app.created");
            app.InitializeComponent();
            startupSteps.Mark("app.initialized");
            BootLoader.SetupUnhandledExceptionsHooks(app, host);
            startupSteps.Mark("unhandled_hooks.wired");
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

            var mainWindow = CreateMainWindowShell(host);
            startupSteps.Mark("mainwindow.shell.created");
            var vm = (IMainViewModel)mainWindow.DataContext;
            var splashCloseSignaled = 0;

            void SignalSplashCloseWhenReady()
            {
                if (Interlocked.Exchange(ref splashCloseSignaled, 1) == 1)
                {
                    return;
                }

                splashHandle?.SignalClose();
                startupSteps.Mark("splash.close.signaled");
            }

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
                    var loadResult = await host.Services.GetRequiredService<ISettingsService>().LoadAsync();
                    startupSteps.Mark("settings.loaded");
                    if (loadResult.IsFailed)
                    {
                        throw new InvalidOperationException(string.Join("; ", loadResult.Errors.Select(e => e.Message)));
                    }

                    await vm.InitializeMainViewAsync(mainWindow);
                    startupSteps.Mark("mainvm.initialized");

                    await host.StartAsync();
                    hostStarted = true;
                    startupSteps.Mark("host.started");

                    CommandExceptionLogger.Initialize(host.Services.GetRequiredService<ILoggerFactory>());
                    host.Services.GetRequiredService<IScannerWarmupService>().StartWarmupInBackground("program.startup");
                    startupSteps.Mark("scanner_warmup.kicked_off");

                    await host.Services.GetRequiredService<IAutoUpdateService>().InitializeAsync();
                    startupSteps.Mark("autoupdate.initialized");
                    startupSteps.Mark("startup.ready");
                    EmitStartupTable(isError: false, "Startup Steps (ready)");
                }
                catch (Exception ex)
                {
                    startupSteps.Mark("startup.failed.in_loaded");
                    EmitStartupTable(isError: true, "Startup Steps (failed)");
                    Log.Fatal(ex, UI.ex_FatalError);
                    app.Shutdown(-1);
                }
            };

            mainWindow.ContentRendered += (_, __) =>
            {
                startupSteps.Mark("mainwindow.content_rendered");
                SignalSplashCloseWhenReady();
            };

            app.MainWindow = mainWindow;
            mainWindow.Show();
            startupSteps.Mark("mainwindow.show.called");
            if (mainWindow.WindowState == WindowState.Minimized)
            {
                mainWindow.WindowState = WindowState.Normal;
            }
            _ = mainWindow.Activate();

            app.Run();
            splashHandle?.Dispose();
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            startupSteps.Mark("startup.failed.outer");
            EmitStartupTable(isError: true, "Startup Steps (failed)");
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
}
