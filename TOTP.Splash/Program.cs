using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
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

        var cts = new CancellationTokenSource();
        Task.Run(() =>
        {
            try
            {
                // Wait for the close event or for the parent process to exit
                while (!cts.Token.IsCancellationRequested)
                {
                    // Check the event (wait up to 100ms)
                    if (closeEvent.WaitOne(100))
                    {
                        splash.Dispatcher.BeginInvoke(splash.Close);
                        break;
                    }

                    // Fallback: check if parent process is still alive
                    if (parentPid > 0)
                    {
                        try
                        {
                            var parent = Process.GetProcessById(parentPid);
                            if (parent.HasExited)
                            {
                                splash.Dispatcher.BeginInvoke(splash.Close);
                                break;
                            }
                        }
                        catch
                        {
                            splash.Dispatcher.BeginInvoke(splash.Close);
                            break;
                        }
                    }
                }
            }
            catch { }
        });

        splash.Closed += (_, __) =>
        {
            cts.Cancel();
            closeEvent.Dispose();
        };

        if (ShouldClose(closeEvent, parentPid))
        {
            closeEvent.Dispose();
            return;
        }

        splash.Show();
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
