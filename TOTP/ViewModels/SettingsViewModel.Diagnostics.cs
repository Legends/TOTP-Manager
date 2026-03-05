using System.Diagnostics;
using TOTP.Infrastructure.Common;

namespace TOTP.ViewModels;

public sealed partial class SettingsViewModel
{
    private void OnOpenLogFolder()
    {
        var path = System.IO.Path.GetDirectoryName(StringsConstants.AppLogPath);
        if (System.IO.Directory.Exists(path))
        {
            Process.Start(new ProcessStartInfo("explorer.exe", path) { UseShellExecute = true });
        }
    }
}
