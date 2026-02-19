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

            //mainWindow.Show();

            // Starts dispatcher; from here you have a real WPF UI thread
            app.Run(mainWindow);

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


  
}