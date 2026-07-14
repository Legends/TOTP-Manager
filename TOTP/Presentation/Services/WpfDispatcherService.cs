 
using System;
using System.Windows;
using System.Windows.Threading;
using System.Threading.Tasks;
using TOTP.Presentation.Services.Interfaces;

namespace TOTP.Presentation.Services;

public sealed class WpfDispatcherService : IDispatcherService
{
    public bool CheckAccess() => Application.Current?.Dispatcher?.CheckAccess() ?? true;

    public void InvokeOnUI(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);

        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher is null)
        {
            action();
            return;
        }

        dispatcher.BeginInvoke(action, DispatcherPriority.DataBind);
    }

    public async Task InvokeAsync(Func<Task> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher is null || dispatcher.CheckAccess())
        {
            await action();
            return;
        }

        await dispatcher.InvokeAsync(action, DispatcherPriority.Background).Task.Unwrap();
    }
}
