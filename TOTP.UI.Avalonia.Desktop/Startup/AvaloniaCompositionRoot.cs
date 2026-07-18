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
using TOTP.Infrastructure.Services;
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
        services.AddSingleton<AvaloniaClipboardAccessor>();
        services.AddSingleton<IAsyncPlatformClipboard>(provider =>
            new AvaloniaPlatformClipboard(
                provider.GetRequiredService<AvaloniaClipboardAccessor>(),
                SupportsClipboardOwnership(),
                provider.GetRequiredService<ILogger<AvaloniaPlatformClipboard>>()));
        services.AddSingleton<AsyncClipboardService>();
        services.AddSingleton<IAsyncClipboardService>(provider =>
            provider.GetRequiredService<AsyncClipboardService>());
        services.AddSingleton<IAvaloniaStartupCoordinator, AvaloniaStartupCoordinator>();
        services.AddSingleton<PasswordUnlockViewModel>();
        services.AddSingleton<AccountListViewModel>();
        services.AddSingleton<SettingsPageViewModel>();
        services.AddSingleton<MainWindowViewModel>();
        services.AddTransient(provider => new MainWindow
        {
            DataContext = provider.GetRequiredService<MainWindowViewModel>(),
            ClipboardAccessor = provider.GetRequiredService<AvaloniaClipboardAccessor>()
        });

        return services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });
    }

    private static bool SupportsClipboardOwnership() =>
        OperatingSystem.IsWindows()
        || OperatingSystem.IsMacOS()
        || (OperatingSystem.IsLinux()
            && string.Equals(
                Environment.GetEnvironmentVariable("XDG_SESSION_TYPE"),
                "x11",
                StringComparison.OrdinalIgnoreCase));
}
