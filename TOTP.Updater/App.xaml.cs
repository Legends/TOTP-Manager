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
            UpdaterBootstrapSupport.RegisterSyncfusionLicenseFromEnvironment();
            var arguments = UpdateInstallArguments.Parse(e.Args);
            var window = new UpdateInstallerWindow(arguments);
            MainWindow = window;
            window.Show();
            UpdaterBootstrapSupport.WriteBootstrapLog(arguments.LogPath, "updater window shown");
            UpdaterBootstrapSupport.WriteReadySignal(arguments.ReadySignalPath);

            Dispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new Action(window.StartInstall));
        }
        catch (Exception ex)
        {
            var logPath = UpdaterBootstrapSupport.TryGetArgumentValue(e.Args, "--logPath");
            UpdaterBootstrapSupport.WriteBootstrapLog(logPath, $"updater bootstrap failed: {ex}");
            MessageBox.Show(
                $"The update installer could not start.{Environment.NewLine}{Environment.NewLine}{ex.Message}",
                "TOTP Manager Updater",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(-1);
        }
    }
}
