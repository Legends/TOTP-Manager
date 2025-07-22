using System.Windows;
using TOTP.Interfaces;
using TOTP.Windows;

namespace TOTP.Services;

public class DialogService : IDialogService
{
    public (bool success, string? key, string? value) ShowKeyValueDialog()
    {
        var dlg = new KeyValueDialog
        {
            Owner = System.Windows.Application.Current?.MainWindow
        };
        var result = dlg.ShowDialog() == true;
        return (result, dlg.ViewModel.Platform, dlg.ViewModel.Secret);
    }

    public (bool success, string? key, string? value) ShowKeyValueDialog(string? initialKey = null, string? initialValue = null)
    {
        var dlg = new KeyValueDialog
        {
            Owner = Application.Current?.MainWindow
        };

        if (initialKey != null)
            dlg.ViewModel.Platform = initialKey;

        if (initialValue != null)
            dlg.ViewModel.Secret = initialValue;

        var result = dlg.ShowDialog() == true;
        return (result, dlg.ViewModel.Platform, dlg.ViewModel.Secret);
    }

}
