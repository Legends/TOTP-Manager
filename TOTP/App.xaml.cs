using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Windows;
using TOTP.Infrastructure.AppLifecycle;
using TOTP.Interfaces;
using TOTP.Resources;

namespace TOTP;

public partial class App : Application
{
    public IHost Host { get; set; } = default!;
    public SingleInstanceGuard? InstanceGuard { get; set; }

    protected override void OnExit(ExitEventArgs e)
    {
        // Optional: anything *extra* at WPF exit (Program.cs already stops host & flushes Serilog)
        try
        {
            var secretsManager = Host.Services.GetService(typeof(ISecretsManager)) as ISecretsManager;
            secretsManager?.BackupSecretsFile();
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