using System;
using System.Windows;
using Serilog;
using TOTP.Core.Services.Interfaces;

namespace TOTP.Presentation.Services;

public sealed class WpfApplicationLifetime : IApplicationLifetime
{
    public void Shutdown(int exitCode = 0)
    {
        var application = Application.Current;
        if (application == null)
        {
            ExitProcess(exitCode);
            return;
        }

        if (application.Dispatcher.HasShutdownStarted || application.Dispatcher.HasShutdownFinished)
        {
            return;
        }

        if (application.Dispatcher.CheckAccess())
        {
            application.Shutdown(exitCode);
        }
        else
        {
            application.Dispatcher.BeginInvoke(() => application.Shutdown(exitCode));
        }
    }

    public void ExitProcess(int exitCode = 0)
    {
        Log.CloseAndFlush();
        Environment.Exit(exitCode);
    }
}
