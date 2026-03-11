using System.Diagnostics;
using System.Threading.Tasks;
using TOTP.Infrastructure.Common;

namespace TOTP.ViewModels;

public sealed partial class SettingsViewModel
{
    private async Task CheckForUpdatesAsync()
    {
        IsCheckingForUpdates = true;
        CheckForUpdatesButtonText = "Checking for updates...";
        RaiseCommandStates();

        try
        {
            await _autoUpdateService.CheckForUpdatesInteractiveAsync();
        }
        finally
        {
            IsCheckingForUpdates = false;
            CheckForUpdatesButtonText = "Check for updates";
            RaiseCommandStates();
        }
    }

    private void OnOpenLogFolder()
    {
        var path = System.IO.Path.GetDirectoryName(StringsConstants.AppLogPath);
        if (System.IO.Directory.Exists(path))
        {
            Process.Start(new ProcessStartInfo("explorer.exe", path) { UseShellExecute = true });
        }
    }
}
