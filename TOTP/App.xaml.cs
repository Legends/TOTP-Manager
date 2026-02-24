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
            
        }
        finally
        {
            base.OnExit(e);
        }
    }



}