using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using TOTP.Core.Services.Interfaces;
using TOTP.Avalonia.Desktop.Platform;
using AppLifetime = TOTP.Core.Services.Interfaces.IApplicationLifetime;

namespace TOTP.Avalonia.Desktop.Startup;

public static class AvaloniaCompositionRoot
{
    public static ServiceProvider Build(IClassicDesktopStyleApplicationLifetime desktopLifetime)
    {
        ArgumentNullException.ThrowIfNull(desktopLifetime);

        var services = new ServiceCollection();

        services.AddSingleton(desktopLifetime);
        services.AddSingleton(new AvaloniaUiScheduler(Dispatcher.UIThread));
        services.AddSingleton<IUiScheduler>(provider =>
            provider.GetRequiredService<AvaloniaUiScheduler>());
        services.AddSingleton<AppLifetime, AvaloniaApplicationLifetime>();
        services.AddTransient<MainWindow>();

        return services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });
    }
}
