using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using TOTP.Avalonia.Desktop.Startup;
using TOTP.Core.Platform;

namespace TOTP.Avalonia.Desktop;

public partial class App : Application
{
    private ServiceProvider? _services;
    private AvaloniaExceptionHooks? _exceptionHooks;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            _services = AvaloniaCompositionRoot.Build(desktop);
            _exceptionHooks = new AvaloniaExceptionHooks(
                global::Avalonia.Threading.Dispatcher.UIThread,
                _services.GetRequiredService<AvaloniaExceptionBoundary>());
            desktop.Exit += (_, _) =>
            {
                _exceptionHooks.Dispose();
                _services.Dispose();
            };
            var mainWindow = _services.GetRequiredService<MainWindow>();
            desktop.MainWindow = mainWindow;
            _services.GetRequiredService<IActivationListener>().Start(request =>
            {
                if (request.Kind != ApplicationActivationKind.ActivateMainWindow) return;
                global::Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    if (mainWindow.WindowState == global::Avalonia.Controls.WindowState.Minimized)
                        mainWindow.WindowState = global::Avalonia.Controls.WindowState.Normal;
                    mainWindow.Show();
                    mainWindow.Activate();
                });
            });
        }

        base.OnFrameworkInitializationCompleted();
    }
}
