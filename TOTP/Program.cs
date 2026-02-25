using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Serilog;
using Serilog.Events;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using TOTP.Core.Services;
using TOTP.Helper;
using TOTP.Infrastructure;
using TOTP.Resources;
using TOTP.Security;
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
        try
        {
            StartApplication(args).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, UI.ex_FatalError);
            Debug.WriteLine(ex.Message);
            Environment.Exit(-1);
        }
    }

    private static async Task StartApplication(string[] args)
    {
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

            using var host = BootLoader.BuildHostAndConfigureServices(configuration);

            // Since there is no SynchronizationContext established yet, await will default to the thread pool anyway.
            await host.StartAsync(); // All BackgroundServices run now!

            var app = new App // the SynchronizationContext is established when the first DispatcherObject is created like Application
            {
                Host = host
            };

            //// safe to call await here since the SynchronizationContext is established
            //// and it will not cause deadlocks.  
            //app.Startup += async (_, __) =>
            //  {
            //      await InitializeLogSwitchService();
            //  };

            //async Task InitializeLogSwitchService()
            //{
            //    try
            //    {
            //        var profileStore = host.Services.GetRequiredService<IGlobalProfileStore>();

            //        var profile = await profileStore.LoadAsync();

            //        if (profile != null)
            //        {
            //var logSwitchService = host.Services.GetRequiredService<ILogSwitchService>();
            //var level = profile?.MinimumLogLevel ?? LogEventLevel.Information;
            //logSwitchService.SetLevel(level);
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        // The error is handled right where it happens
            //        Log.Error(ex, "Error initializing logSwitchService");
            //    }
            //}

            app.InitializeComponent();
            BootLoader.SetupUnhandledExceptionsHooks(app, host);

            var mainWindow = SetupMainWindow(host, app);


            app.Exit += async (_, __) =>
            {
                try
                {
                    if (host != null) await host?.StopAsync();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, UI.ex_UnexpectedError);
                }
                finally
                {
                    await Log.CloseAndFlushAsync();
                }
            };

            app.Run(mainWindow); // app.Run() is a blocking call. It is the message loop.

        }
        catch (Exception e)
        {
            Log.Fatal(e, UI.ex_FatalError);
            Environment.Exit(-1);
        }
    }

    private static MainWindow SetupMainWindow(IHost host, App app)
    {
        var vm = host.Services.GetRequiredService<IMainViewModel>();
        var mainWindow = host.Services.GetRequiredService<MainWindow>();
        mainWindow.DataContext = vm;
        mainWindow.ResizeMode = ResizeMode.NoResize;
        app.MainWindow = mainWindow;

        mainWindow.Loaded += async (_, __) =>
        {
            try
            {
                await vm.InitializeMainViewAsync(mainWindow); // called on UI-Thread 
            }
            catch (Exception e)
            {
                Log.Fatal(e, UI.ex_FatalError);
                app.Shutdown(-1);
            }
        };
        return mainWindow;
    }
}