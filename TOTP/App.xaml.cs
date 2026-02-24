using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Serilog;
using System;
using System.Windows;
using TOTP.Core.Services;
using TOTP.Core.Services.Interfaces;
using TOTP.Resources;
using TOTP.Security.Interfaces;
using TOTP.Services.Interfaces;
using TOTP.Startup;
using TOTP.Views;

namespace TOTP;

public partial class App : Application
{
    public IHost Host { get; set; } = default!;
    //public SingleInstanceGuard? InstanceGuard { get; set; }

    public IAuthorizationService AuthorizationService { get; set; }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            // 1. Load Profile (The 'await' works perfectly here!)
            var profileStore = Host.Services.GetRequiredService<IGlobalProfileStore>();
            var profile = await profileStore.LoadAsync();

            if (profile != null)
            {
                var loggingService = Host.Services.GetRequiredService<ILogSwitchService>();
                loggingService.SetLevel(profile.MinimumLogLevel);
            }

            // 2. Now that we are back on the STA thread, resolve UI components
            var vm = Host.Services.GetRequiredService<IMainViewModel>();
            var mainWindow = Host.Services.GetRequiredService<MainWindow>();

            mainWindow.DataContext = vm;
            mainWindow.ResizeMode = ResizeMode.NoResize;
            this.MainWindow = mainWindow;

            // 3. Setup hooks and initialize VM
            BootLoader.SetupUnhandledExceptionsHooks(this, Host);

            // Wire up the loaded event as you had before
            mainWindow.Loaded += async (s, args) =>
            {
                await vm.InitializeMainViewAsync(mainWindow);
            };

            mainWindow.Show();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Failed to initialize application.");
            MessageBox.Show("A critical error occurred during startup.");
            Shutdown(-1);
        }

        SystemEvents.SessionSwitch += (_, sessionSwitchEventArg) =>
        {
            try
            {
                if (sessionSwitchEventArg.Reason == SessionSwitchReason.SessionLock)
                {
                    AuthorizationService.Lock();
                }

                //if (sessionSwitchEventArg.Reason == SessionSwitchReason.SessionUnlock)
                //{

                //}
            }
            catch (Exception ex)
            {
                var logger = Host.Services.GetService(typeof(ILogger<App>)) as ILogger<App>;
                logger?.LogError(string.Format(UI.ex_BackupFailed, ex.Message));
            }
           
        };

    }

    protected override async void OnExit(ExitEventArgs e)
    {
        // Optional: anything *extra* at WPF exit (Program.cs already stops host & flushes Serilog)
        try
        {
            var accountsManager = Host.Services.GetService(typeof(IAccountsManager)) as IAccountsManager;
            if (accountsManager != null)
            {
                await accountsManager.BackupAccountsStorageFileAsync();
            }
        }
        catch (Exception ex)
        {
            var logger = Host.Services.GetService(typeof(ILogger<App>)) as ILogger<App>;
            logger?.LogError(string.Format(UI.ex_BackupFailed, ex.Message));
            // Don't rethrow here; we're shutting down.
        }
        finally
        {
            base.OnExit(e);
        }
    }



}