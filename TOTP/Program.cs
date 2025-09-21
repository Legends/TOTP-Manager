using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Threading.Tasks;
using TOTP.Helper;
using TOTP.Infrastructure.AppLifecycle;
using TOTP.Resources;
using TOTP.Startup;

namespace TOTP;


internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        Run(args).GetAwaiter().GetResult();
    }

    private static async Task Run(string[] args)
    {
        IHost? host = null;
        try
        {
            //await QrWarmupService.WarmUpAsync();
            // 1) config / culture / license
            var configuration = BootLoader.BuildConfiguration();         // your helper
            BootLoader.SetCulture(configuration);                        // your helper
            BootLoader.RegisterSyncfusionLicenseKey(configuration);      // your helper

            // 2) single instance (optional)
            using var instance = new SingleInstanceGuard(StringsConstants.AssemblyNameWpf);
            if (!instance.IsFirstInstance)
            {
                SingleInstanceGuard.ActivateExistingWindow(StringsConstants.AssemblyNameWpf);
                return;
            }

            host = BootLoader.CreateHostAndConfigureServices(configuration);

            // IMPORTANT: stay on STA thread (no await here)
            await host.StartAsync();

            // 4) WPF app
            var app = new App { Host = host, InstanceGuard = instance };
            app.InitializeComponent();
            BootLoader.SetupUnhandledExceptionsHooks(app, host); // your helper

            // 5) resolve & show window (explicit)
            var mainWindow = host.Services.GetRequiredService<MainWindow>();
            app.MainWindow = mainWindow;
            mainWindow.Show();

            // 6) run dispatcher
            app.Run();

        }
        catch (Exception e)
        {
            Log.Fatal(e, UI.ex_FatalError);
            Environment.Exit(-1);
        }
        finally
        {
            // 7) graceful shutdown
            if (host is not null)
            {
                await host.StopAsync();
                host.Dispose();
            }
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