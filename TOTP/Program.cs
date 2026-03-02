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
using Windows.Security.Credentials.UI;
using TOTP.Core.Services;
using TOTP.Helper;
using TOTP.Infrastructure;
using TOTP.Infrastructure.Common;
using TOTP.Infrastructure.Logging;
using TOTP.Resources;
using TOTP.Security;
using TOTP.Security.Interfaces;
using TOTP.Startup;
using TOTP.ViewModels.Interfaces;
using TOTP.Views;

namespace TOTP;


internal static class Program
{

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

            using var host = BootLoader.BuildHostAndConfigureServices(configuration, args);

            // Since there is no SynchronizationContext established yet, await will default to the thread pool anyway.
            await host.StartAsync(); // All BackgroundServices run now! TOTP.Infrastructure.Services

            var app = new App // the SynchronizationContext is established when the first DispatcherObject is created like Application
            {
                Host = host
            };

            app.InitializeComponent();
            BootLoader.SetupUnhandledExceptionsHooks(app, host);

            var mainWindow = SetupMainWindow(host, app);


            app.Exit += async (_, __) =>
            {
                try
                {
                    if (host != null)
                    {
                        // Stops all Background-Services (TOTP.UI.WPF: => Infrastructure.Services) gracefully.
                        // Waits until they are stopped.
                        // Dispose is called after that, which disposes all services.
                        await host?.StopAsync();
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, UI.ex_UnexpectedError);
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