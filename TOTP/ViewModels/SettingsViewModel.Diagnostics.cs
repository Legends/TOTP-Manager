using System.Diagnostics;
using System.Threading.Tasks;
using TOTP.Core.Common;
using TOTP.Resources;

namespace TOTP.ViewModels;

public sealed partial class SettingsViewModel
{
    private async Task CheckForUpdatesAsync()
    {
        IsCheckingForUpdates = true;
        CheckForUpdatesButtonText = UI.ui_Settings_About_CheckingForUpdates;
        RaiseCommandStates();

        try
        {
            await _autoUpdateService.CheckForUpdatesInteractiveAsync();
        }
        finally
        {
            IsCheckingForUpdates = false;
            CheckForUpdatesButtonText = UI.ui_Settings_About_CheckForUpdates;
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
