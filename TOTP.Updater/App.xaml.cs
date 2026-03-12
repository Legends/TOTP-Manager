using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace TOTP.Updater;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            var arguments = UpdateInstallArguments.Parse(e.Args);
            var window = new UpdateInstallerWindow(arguments);
            MainWindow = window;
            window.Show();
            WriteBootstrapLog(arguments.LogPath, "updater window shown");
            WriteReadySignal(arguments.ReadySignalPath);

            Dispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new Action(window.StartInstall));
        }
        catch (Exception ex)
        {
            var logPath = TryGetArgumentValue(e.Args, "--logPath");
            WriteBootstrapLog(logPath, $"updater bootstrap failed: {ex}");
            MessageBox.Show(
                $"The update installer could not start.{Environment.NewLine}{Environment.NewLine}{ex.Message}",
                "TOTP Manager Updater",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(-1);
        }
    }

    private static void WriteReadySignal(string? readySignalPath)
    {
        if (string.IsNullOrWhiteSpace(readySignalPath))
        {
            return;
        }

        var directory = Path.GetDirectoryName(readySignalPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(readySignalPath, "ready", Encoding.UTF8);
    }

    private static void WriteBootstrapLog(string? logPath, string message)
    {
        if (string.IsNullOrWhiteSpace(logPath))
        {
            return;
        }

        var directory = Path.GetDirectoryName(logPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var line = $"{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss} {message}{Environment.NewLine}";
        File.AppendAllText(logPath, line, Encoding.UTF8);
    }

    private static string? TryGetArgumentValue(IReadOnlyList<string> args, string key)
    {
        for (var index = 0; index < args.Count - 1; index++)
        {
            if (string.Equals(args[index], key, StringComparison.OrdinalIgnoreCase))
            {
                return args[index + 1];
            }
        }

        return null;
    }
}
