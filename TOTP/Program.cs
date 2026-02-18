using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Threading.Tasks;
using TOTP.Helper;
using TOTP.Infrastructure;
using TOTP.Resources;
using TOTP.Security.Interfaces;
using TOTP.Services.Interfaces;
using TOTP.Startup;
using TOTP.Views;

namespace TOTP;


internal static class Program
{

    [STAThread]
    public static void Main(string[] args)
    {
        Run(args);
    }

    private static void Run(string[] args)
    {
        IHost? host = null;

        try
        {
            var configuration = BootLoader.BuildConfiguration();
            BootLoader.SetCulture(configuration);
            BootLoader.RegisterSyncfusionLicenseKey(configuration);

            using var instance = new SingleInstanceGuard(StringsConstants.AssemblyNameWpf);
            if (!instance.IsFirstInstance)
            {
                SingleInstanceGuard.ActivateExistingWindow(StringsConstants.AssemblyNameWpf);
                return;
            }

            host = BootLoader.BuildHostAndConfigureServices(configuration);

            // IMPORTANT: stay on the STA thread
            host.StartAsync().GetAwaiter().GetResult();

            var app = new App
            {
                Host = host,
                AuthorizationService = host.Services.GetRequiredService<IAuthorizationService>()
            };

            app.InitializeComponent();
            BootLoader.SetupUnhandledExceptionsHooks(app, host);

            var mainWindow = host.Services.GetRequiredService<MainWindow>();
            mainWindow.DataContext = host.Services.GetRequiredService<IMainViewModel>();
            mainWindow.ResizeMode = System.Windows.ResizeMode.NoResize;

            app.MainWindow = mainWindow;

            // 2. Resolve the ViewModel and trigger InitializeAsync
            // We do not 'await' here to avoid blocking the STA thread before the Dispatcher starts.
            // The ViewModel internally handles the Task.
            if (mainWindow.DataContext is IMainViewModel vm)
            {
                // Use Task.Run or simply fire-and-forget the Task 
                // because InitializeAsync internally handles its own UI updates/awaiting.
                _ = vm.InitializeMainViewAsync(mainWindow);
            }

            mainWindow.Show();

            // Starts dispatcher; from here you have a real WPF UI thread
            app.Run();

            // Graceful shutdown after UI exits
            host.StopAsync().GetAwaiter().GetResult();
        }
        catch (Exception e)
        {
            Log.Fatal(e, UI.ex_FatalError);
            Environment.Exit(-1);
        }
        finally
        {
            host?.Dispose();
            Log.CloseAndFlush();
        }
    }


    // -> call through to your existing helpers:
    // BuildConfiguration, SetCulture, RegisterSyncfusionLicenseKey,
    // CreateHostAndConfigureServices, SetupUnhandledExceptionsHooks


    ///// <summary>
    ///// DOES NOT WORK !!!!
    ///// </summary>
    ///// <param name="args"></param>
    ///// <returns></returns>
    //[STAThread]
    //public static async Task Main(string[] args)
    //{
    //    // Early, console-safe Serilog to catch boot failures
    //    LoggingConfigurator.SetupEarlyLogger();

    //    SingleInstanceGuard? instanceGuard = null;
    //    IHost? host = null;

    //    try
    //    {
    //        // 1) Configuration (user-secrets + appsettings)
    //        var configuration = BuildConfiguration();

    //        // 2) Culture / Localization
    //        SetCulture(configuration);

    //        // 3) Licensing
    //        RegisterSyncfusionLicenseKey(configuration);

    //        // 4) Single instance check
    //        instanceGuard = new SingleInstanceGuard(StringsConstants.AssemblyNameWpf);
    //        if (!instanceGuard.IsFirstInstance)
    //        {
    //            SingleInstanceGuard.ActivateExistingWindow(StringsConstants.AssemblyNameWpf);
    //            return;
    //        }

    //        // 5) Host (DI + Logging)
    //        host = CreateHostAndConfigureServices(configuration);

    //        // 6) Start the host (awaitable ✅)
    //        await host.StartAsync();

    //        // 7) Create and configure WPF Application
    //        var app = new App
    //        {
    //            Host = host,
    //            InstanceGuard = instanceGuard
    //        };

    //        // Global exception hooks
    //        SetupUnhandledExceptionsHooks(app, host);

    //        // Optional: App XAML resources
    //        app.InitializeComponent();

    //        // 8) Resolve and show MainWindow
    //        var mainWindow = host.Services.GetRequiredService<MainWindow>();
    //        app.Run(mainWindow); // blocks until window closed
    //    }
    //    catch (Exception ex)
    //    {
    //        Log.Fatal(ex, "Fatal error during application startup.");
    //    }
    //    finally
    //    {
    //        try
    //        {
    //            if (host is not null)
    //            {
    //                await host.StopAsync();
    //                host.Dispose();
    //            }
    //            Log.CloseAndFlush();
    //        }
    //        catch
    //        {
    //            // swallow final shutdown errors
    //        }

    //        // Ensure single-instance guard is released
    //        instanceGuard?.Dispose();
    //    }
    //}


    ///// <summary>
    ///// FATAL APPLICATION ERROR ON SHUTDOWN
    ///// </summary>
    ///// <param name="args"></param>
    //[STAThread]
    //public static void Main(string[] args)
    //{
    //    LoggingConfigurator.SetupEarlyLogger();

    //    var configuration = BuildConfiguration();
    //    SetCulture(configuration);
    //    RegisterSyncfusionLicenseKey(configuration);

    //    using var instanceGuard = new SingleInstanceGuard(StringsConstants.AssemblyNameWpf);
    //    if (!instanceGuard.IsFirstInstance)
    //    {
    //        SingleInstanceGuard.ActivateExistingWindow(StringsConstants.AssemblyNameWpf);
    //        return;
    //    }

    //    var host = CreateHostAndConfigureServices(configuration);

    //    // 👇 stay on STA thread
    //    host.StartAsync().GetAwaiter().GetResult();

    //    var app = new App { Host = host, InstanceGuard = instanceGuard };
    //    SetupUnhandledExceptionsHooks(app, host);
    //    app.InitializeComponent();

    //    var mainWindow = host.Services.GetRequiredService<MainWindow>();
    //    app.Run(mainWindow);

    //    // graceful shutdown
    //    host.StopAsync().GetAwaiter().GetResult();
    //    host.Dispose();
    //    Serilog.Log.CloseAndFlush();
    //}


}