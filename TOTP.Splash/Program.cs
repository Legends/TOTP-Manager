using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Threading;

namespace TOTP.Splash;

internal static class Program
{
    private const string SplashTokenArg = "--splash-token";
    private const string SplashParentPidArg = "--splash-parent-pid";

    [STAThread]
    public static void Main(string[] args)
    {
        var token = GetArgValue(args, SplashTokenArg);
        var parentPidArg = GetArgValue(args, SplashParentPidArg);

        if (string.IsNullOrWhiteSpace(token))
        {
            return;
        }

        EventWaitHandle? closeEvent = null;
        try
        {
            closeEvent = EventWaitHandle.OpenExisting(token);
        }
        catch
        {
            return;
        }

        int.TryParse(parentPidArg, out var parentPid);
        if (ShouldClose(closeEvent, parentPid))
        {
            closeEvent.Dispose();
            return;
        }

        var app = new App();
        var splash = new SplashWindow();
        app.MainWindow = splash;

        var timer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromMilliseconds(100)
        };

        timer.Tick += (_, __) =>
        {
            if (ShouldClose(closeEvent, parentPid))
            {
                splash.Close();
            }
        };

        splash.Closed += (_, __) =>
        {
            timer.Stop();
            closeEvent.Dispose();
        };

        if (ShouldClose(closeEvent, parentPid))
        {
            closeEvent.Dispose();
            return;
        }

        splash.Show();
        timer.Start();
        app.Run();
    }

    private static bool ShouldClose(EventWaitHandle closeEvent, int parentPid)
    {
        try
        {
            if (closeEvent.WaitOne(0))
            {
                return true;
            }

            if (parentPid > 0)
            {
                try
                {
                    var parent = Process.GetProcessById(parentPid);
                    return parent.HasExited;
                }
                catch
                {
                    return true;
                }
            }
        }
        catch
        {
            return true;
        }

        return false;
    }

    private static string? GetArgValue(string[] args, string key)
    {
        for (var i = 0; i < args.Length - 1; i++)
        {
            if (string.Equals(args[i], key, StringComparison.OrdinalIgnoreCase))
            {
                return args[i + 1];
            }
        }

        return null;
    }
}
