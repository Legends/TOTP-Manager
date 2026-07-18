using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TOTP.Core.Security.Interfaces;
using TOTP.Core.Services.Interfaces;
using TOTP.Avalonia.Desktop.Platform;
using TOTP.Avalonia.Desktop.Presentation;
using TOTP.Infrastructure.Extensions;
using TOTP.Infrastructure.Security;
using AppLifetime = TOTP.Core.Services.Interfaces.IApplicationLifetime;

namespace TOTP.Avalonia.Desktop.Startup;

public static class AvaloniaCompositionRoot
{
    public static ServiceProvider Build(IClassicDesktopStyleApplicationLifetime desktopLifetime)
    {
        ArgumentNullException.ThrowIfNull(desktopLifetime);

        var services = new ServiceCollection();
        var platformServices = DesktopPlatformServiceFactory.Create();
        var configuration = new ConfigurationBuilder().Build();

        services.AddSingleton(desktopLifetime);
        services.AddSingleton<IConfiguration>(configuration);
        services.AddSingleton<IPlatformApplicationPaths>(platformServices.ApplicationPaths);
        services.AddLogging();
        services.AddSingleton<IPlatformQuickUnlock, UnavailablePlatformQuickUnlock>();
        services.AddInfrastructure(
            configuration,
            platformServices.ApplicationPaths,
            platformServices.FileSecurity);
        services.AddSingleton(new AvaloniaUiScheduler(Dispatcher.UIThread));
        services.AddSingleton<IUiScheduler>(provider =>
            provider.GetRequiredService<AvaloniaUiScheduler>());
        services.AddSingleton<AppLifetime, AvaloniaApplicationLifetime>();
        services.AddSingleton<IAvaloniaStartupCoordinator, AvaloniaStartupCoordinator>();
        services.AddSingleton<PasswordUnlockViewModel>();
        services.AddSingleton<MainWindowViewModel>();
        services.AddTransient(provider => new MainWindow
        {
            DataContext = provider.GetRequiredService<MainWindowViewModel>()
        });

        return services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });
    }
}
