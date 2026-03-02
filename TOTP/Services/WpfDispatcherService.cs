 
using System;
using System.Windows;
using System.Windows.Threading;
using TOTP.Core.Interfaces;
using TOTP.Core.Services.Interfaces;

namespace TOTP.Infrastructure.Services;

public sealed class WpfDispatcherService : IDispatcherService
{
    public bool CheckAccess() => Application.Current?.Dispatcher?.CheckAccess() ?? true;

    public void InvokeOnUI(Action action)
    {
        Application.Current?.Dispatcher?.BeginInvoke(action, DispatcherPriority.DataBind);
    }
}