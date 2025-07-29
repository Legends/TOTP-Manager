using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;
using TOTP.Interfaces;
using TOTP.Windows;

namespace TOTP.Services;

public class PlatformSecretDialogService : IPlatformSecretDialogService
{
    private readonly IServiceProvider _provider;

    public PlatformSecretDialogService(IServiceProvider provider)
    {
        _provider = provider;
    }

    public (bool success, string? key, string? value) ShowForm()
    {
        var dlg = _provider.GetRequiredService<KeyValueDialog>();
        dlg.Owner = Application.Current?.MainWindow;
        var result = dlg.ShowDialog() == true;
        return (result, dlg.ViewModel.Platform, dlg.ViewModel.Secret);
    }

    public (bool success, string? key, string? value) ShowForm(string? initialKey = null,
        string? initialValue = null)
    {
        var dlg = _provider.GetRequiredService<KeyValueDialog>();
        dlg.Owner = Application.Current?.MainWindow;

        if (initialKey != null)
            dlg.ViewModel.Platform = initialKey;

        if (initialValue != null)
            dlg.ViewModel.Secret = initialValue;

        var result = dlg.ShowDialog() == true;
        return (result, dlg.ViewModel.Platform, dlg.ViewModel.Secret);
    }
}