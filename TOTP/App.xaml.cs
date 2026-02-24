using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using System;
using System.Windows;
using TOTP.Core.Services.Interfaces;
using TOTP.Resources;
using TOTP.Security.Interfaces;

namespace TOTP;

public partial class App : Application
{
    public IHost Host { get; set; } = default!;
    //public SingleInstanceGuard? InstanceGuard { get; set; }

    public IAuthorizationService AuthorizationService { get; set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        SystemEvents.SessionSwitch += (_, sessionSwitchEventArg) =>
        {
            if (sessionSwitchEventArg.Reason == SessionSwitchReason.SessionLock)
            {
                AuthorizationService.Lock();
            }

            //if (sessionSwitchEventArg.Reason == SessionSwitchReason.SessionUnlock)
            //{

            //}
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