using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Syncfusion.SfSkinManager;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using TOTP.Commands;
using TOTP.Core.Common;
using TOTP.Core.Security.Interfaces;
using TOTP.Core.Services;
using TOTP.Helper;
using TOTP.Presentation.Platform;
using TOTP.Presentation;
using TOTP.Infrastructure.Logging;
using TOTP.Resources;
using TOTP.Presentation.Services;
using TOTP.Presentation.Services.Interfaces;
using TOTP.Services.Interfaces;
using TOTP.Startup;
using TOTP.ViewModels.Interfaces;
using TOTP.Views;

namespace TOTP;

internal static class Program
{
    private sealed class SplashThreadHost : IDisposable
    {
        private readonly ManualResetEventSlim _shown = new(false);
        private readonly ManualResetEventSlim _closed = new(false);
        private Thread? _thread;
        private Dispatcher? _dispatcher;
        private SplashWindow? _window;
        private Exception? _startupException;

        public void Start()
        {
            _thread = new Thread(ThreadMain)
            {
                IsBackground = true,
                Name = "TOTP Splash"
            };
            _thread.SetApartmentState(ApartmentState.STA);
            _thread.Start();

            _shown.Wait();
            if (_startupException != null)
            {
                throw new InvalidOperationException("Failed to start splash window.", _startupException);
            }
        }

        public void Close()
        {
            var dispatcher = _dispatcher;
            if (dispatcher == null)
            {
                return;
            }

            dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                _window?.Close();
            }));

            _closed.Wait(TimeSpan.FromSeconds(5));
        }

        public void Dispose()
        {
            Close();
            _shown.Dispose();
            _closed.Dispose();
        }

        private void ThreadMain()
        {
            try
            {
                _dispatcher = Dispatcher.CurrentDispatcher;
                _window = new SplashWindow();
                _window.Closed += (_, __) =>
                {
                    _closed.Set();
                    Dispatcher.CurrentDispatcher.BeginInvokeShutdown(DispatcherPriority.Background);
                };
                _window.Show();
                _shown.Set();
                Dispatcher.Run();
            }
            catch (Exception ex)
            {
                _startupException = ex;
                _shown.Set();
                _closed.Set();
            }
        }
    }

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

    [STAThread]
    public static void Main(string[] args)
    {
        var processEntryUtc = DateTimeOffset.UtcNow;
        var processEntryTick = Stopwatch.GetTimestamp();

        try
        {
            LoggingConfigurator.SetupEarlyLogger(args);
            Log.Information("process.entry utc={Utc} pid={Pid}", processEntryUtc, Environment.ProcessId);
            StartApplication(args, processEntryUtc, processEntryTick).GetAwaiter().GetResult();
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

    private static async Task StartApplication(string[] args, DateTimeOffset processEntryUtc, long processEntryTick)
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
            var processEntryElapsedMs = (long)((Stopwatch.GetTimestamp() - processEntryTick) * 1000.0 / Stopwatch.Frequency);
            Log.Information("startup.premain elapsed_ms={ElapsedMs} process_entry_utc={Utc}", processEntryElapsedMs, processEntryUtc);
            Log.Information("startup.begin");
            startupSteps.Mark("startup.begin");

            using var instance = new SingleInstanceGuard(PresentationConstants.AssemblyName);
            if (!instance.IsFirstInstance)
            {
                startupSteps.Mark("single_instance.redirect_existing");
                SingleInstanceGuard.ActivateExistingWindow(PresentationConstants.AssemblyName);
                EmitStartupTable(isError: false, "Startup Steps (redirected)");
                return;
            }

            startupSteps.Mark("single_instance.first_instance");

            SplashThreadHost? splash = null;
            try
            {
                splash = new SplashThreadHost();
                splash.Start();
                startupSteps.Mark("splash.shown");
                Log.Information("startup.splash.shown");
            }
            catch (Exception ex)
            {
                startupSteps.Mark("splash.failed");
                Log.Warning(ex, "startup.splash.failed");
            }

            var app = new App
            {
                ShutdownMode = ShutdownMode.OnExplicitShutdown
            };
            startupSteps.Mark("app.created");
            SfSkinManager.ApplyStylesOnApplication = true;
            startupSteps.Mark("syncfusion.apply_styles_on_application");
            app.InitializeComponent();
            startupSteps.Mark("app.initialized");

            var configuration = BootLoader.BuildConfiguration();
            startupSteps.Mark("configuration.built");
            BootLoader.SetCulture(configuration);
            startupSteps.Mark("culture.set");
            BootLoader.RegisterSyncfusionLicenseKey(configuration);
            startupSteps.Mark("syncfusion.license.registered");

            using var host = BootLoader.BuildHostAndConfigureServices(configuration, args);
            startupSteps.Mark("host.built");
            BootLoader.SetupUnhandledExceptionsHooks(app, host);
            startupSteps.Mark("unhandled_hooks.wired");
            using var startupCancellation = new CancellationTokenSource();
            Task? startupTask = null;
            var hostStartAttempted = false;
            var hostStopping = 0;

            async Task StopHostAsync()
            {
                if (!hostStartAttempted || Interlocked.Exchange(ref hostStopping, 1) == 1)
                {
                    Log.Information(
                        "shutdown.host_stop.skipped host_start_attempted={HostStartAttempted} host_stopping={HostStopping}",
                        hostStartAttempted,
                        hostStopping);
                    return;
                }

                try
                {
                    Log.Information("shutdown.host_stop.begin");
                    await host.StopAsync(TimeSpan.FromSeconds(10));
                    Log.Information("shutdown.host_stop.end");
                }
                catch (Exception ex) { Log.Error(ex, UI.ex_UnexpectedError); }
            }

            app.Exit += (_, args) =>
            {
                Log.Information("app.exit code={ExitCode}", args.ApplicationExitCode);
                startupCancellation.Cancel();
            };

            var mainWindow = CreateMainWindowShell(host);
            startupSteps.Mark("mainwindow.shell.created");
            var vm = (IMainViewModel)mainWindow.DataContext;
            var mainWindowShown = false;

            void ShowMainWindowWhenReady()
            {
                if (mainWindowShown || vm.IsBusy)
                {
                    return;
                }

                mainWindowShown = true;
                app.MainWindow = mainWindow;
                app.ShutdownMode = ShutdownMode.OnMainWindowClose;

                if (splash is not null)
                {
                    splash.Close();
                    splash.Dispose();
                    splash = null;
                    startupSteps.Mark("splash.closed");
                }

                mainWindow.Show();
                startupSteps.Mark("mainwindow.show.called");
                if (mainWindow.WindowState == WindowState.Minimized)
                {
                    mainWindow.WindowState = WindowState.Normal;
                }

                _ = mainWindow.Activate();
            }

            var startupTablePending = false;
            var mainWindowContentRendered = false;
            var mainWindowRendered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            void EmitReadyStartupTableIfPossible()
            {
                if (!startupTablePending || !mainWindowContentRendered)
                {
                    return;
                }

                EmitStartupTable(isError: false, "Startup Steps (ready)");
                startupTablePending = false;
            }
            mainWindow.ContentRendered += (_, __) =>
            {
                startupSteps.Mark("mainwindow.content_rendered");
                mainWindowContentRendered = true;
                mainWindowRendered.TrySetResult();
                EmitReadyStartupTableIfPossible();
            };

            if (vm.IsBusy)
            {
                PropertyChangedEventHandler? busyHandler = null;
                busyHandler = (_, e) =>
                {
                    if (!string.Equals(e.PropertyName, nameof(IMainViewModel.IsBusy), StringComparison.Ordinal))
                    {
                        return;
                    }

                    if (vm.IsBusy)
                    {
                        return;
                    }

                    vm.PropertyChanged -= busyHandler;
                    app.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(ShowMainWindowWhenReady));
                };

                vm.PropertyChanged += busyHandler;
            }
            else
            {
                ShowMainWindowWhenReady();
            }

            startupTask = app.Dispatcher.InvokeAsync(async () =>
            {
                try
                {
                    startupCancellation.Token.ThrowIfCancellationRequested();

                    var loadResult = await host.Services.GetRequiredService<ISettingsService>().LoadAsync();
                    startupSteps.Mark("settings.loaded");
                    if (loadResult.IsFailed)
                    {
                        throw new InvalidOperationException(string.Join("; ", loadResult.Errors.Select(e => e.Message)));
                    }

                    host.Services.GetRequiredService<ILocalizationService>().ApplyCurrentCulture();

                    startupCancellation.Token.ThrowIfCancellationRequested();

                    await vm.InitializeMainViewAsync(mainWindow, startupCancellation.Token);
                    startupSteps.Mark("mainvm.initialized");
                    ShowMainWindowWhenReady();

                    await mainWindowRendered.Task.WaitAsync(startupCancellation.Token);
                    startupSteps.Mark("mainwindow.ready_for_authorization");

                    await vm.SessionController.TryUnlockOnStartupAsync(startupCancellation.Token);
                    startupSteps.Mark("startup_unlock.completed");

                    startupCancellation.Token.ThrowIfCancellationRequested();

                    hostStartAttempted = true;
                    await host.StartAsync(startupCancellation.Token);
                    startupSteps.Mark("host.started");

                    startupCancellation.Token.ThrowIfCancellationRequested();

                    CommandExceptionLogger.Initialize(host.Services.GetRequiredService<ILoggerFactory>());
                    host.Services.GetRequiredService<IScannerWarmupService>().StartWarmupInBackground("program.startup");
                    startupSteps.Mark("scanner_warmup.kicked_off");
                    startupSteps.Mark("startup.ready");
                    startupTablePending = true;
                    EmitReadyStartupTableIfPossible();
                }
                catch (OperationCanceledException) when (startupCancellation.IsCancellationRequested)
                {
                    startupSteps.Mark("startup.canceled");
                }
                catch (Exception ex)
                {
                    startupSteps.Mark("startup.failed.in_loaded");
                    EmitStartupTable(isError: true, "Startup Steps (failed)");
                    Log.Fatal(ex, UI.ex_FatalError);
                    app.Shutdown(-1);
                }
            }, DispatcherPriority.Background).Task.Unwrap();

            app.Run();
            Log.Information("app.run.returned");
            startupCancellation.Cancel();

            if (startupTask != null)
            {
                Log.Information("shutdown.startup_task.wait.begin");
                try { await startupTask.WaitAsync(TimeSpan.FromSeconds(5)); }
                catch (OperationCanceledException) when (startupCancellation.IsCancellationRequested) { }
                catch (TimeoutException ex) { Log.Warning(ex, "Timed out waiting for startup task to finish during shutdown."); }
                catch (Exception ex) { Log.Error(ex, UI.ex_UnexpectedError); }
                Log.Information("shutdown.startup_task.wait.end");
            }

            await StopHostAsync();
        }
        catch (Exception ex)
        {
            startupSteps.Mark("startup.failed.outer");
            EmitStartupTable(isError: true, "Startup Steps (failed)");
            Log.Fatal(ex, UI.ex_FatalError);
            Environment.Exit(-1);
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
}
