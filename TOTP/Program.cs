using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
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
        try
        {
            Run(args).GetAwaiter().GetResult();
        }
        catch (Exception runEX)
        {
            Debug.WriteLine(runEX);
            throw;
        }
    }

    private static async Task Run(string[] args)
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
         
            // Since there is no SynchronizationContext established yet, await will default to the thread pool anyway.
            await host.StartAsync();  


            var app = new App // the SynchronizationContext is established when the first DispatcherObject is created like Application
            {
                Host = host,
                AuthorizationService = host.Services.GetRequiredService<IAuthorizationService>()
            };

            app.InitializeComponent();
            BootLoader.SetupUnhandledExceptionsHooks(app, host);
            
            var vm = host.Services.GetRequiredService<IMainViewModel>();
            var mainWindow = host.Services.GetRequiredService<MainWindow>();
            mainWindow.DataContext = vm;
            mainWindow.ResizeMode = System.Windows.ResizeMode.NoResize;

            app.MainWindow = mainWindow;
          
            mainWindow.Loaded += async (_, __) =>
            {
                try
                {
                    await vm.InitializeMainViewAsync(mainWindow); // läuft auf UI-Thread weiter
                }
                catch (Exception e)
                {
                    Log.Fatal(e, UI.ex_FatalError);
                    app.Shutdown(-1);
                }
            };


            app.Exit += async (_, __) =>
            {
                try { if (host != null) await host.StopAsync(); } catch { /* log if you want */ }
                host?.Dispose();
                Log.CloseAndFlush();
            };
            
            app.Run(mainWindow); // app.Run() is a blocking call. It is the message loop.

        }
        catch (Exception e)
        {
            Log.Fatal(e, UI.ex_FatalError);
            Environment.Exit(-1);
        }
    }

   
}