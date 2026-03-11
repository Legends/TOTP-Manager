using System.Windows;

namespace TOTP.Updater;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var window = new UpdateInstallerForm(e.Args);
        MainWindow = window;
        window.Show();
    }
}
