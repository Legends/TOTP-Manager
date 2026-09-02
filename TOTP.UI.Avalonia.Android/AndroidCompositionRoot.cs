using Android.Content;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TOTP.Avalonia.Mobile.Localization;
using TOTP.Avalonia.Mobile.Presentation;
using TOTP.Core.Security.Interfaces;
using TOTP.Core.Services.Interfaces;
using TOTP.Infrastructure.Extensions;
using TOTP.Infrastructure.Security;
using TOTP.Infrastructure.Services;
using TOTP.Platform.Android;

namespace TOTP.Avalonia.Android;

internal static class AndroidCompositionRoot
{
    public static ServiceProvider Build(Context context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var services = new ServiceCollection();
        var paths = new AndroidApplicationPaths(context);
        var fileSecurity = new AndroidFileSecurity(context);
        var configuration = new ConfigurationBuilder().Build();

        services.AddSingleton(context);
        services.AddSingleton<IConfiguration>(configuration);
        services.AddSingleton<IPlatformApplicationPaths>(paths);
        services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Warning));
        services.AddSingleton<IPlatformQuickUnlock, UnavailablePlatformQuickUnlock>();
        services.AddInfrastructure(configuration, paths, fileSecurity);
        services.AddSingleton<IAsyncPlatformClipboard, AndroidPlatformClipboard>();
        services.AddSingleton<AsyncClipboardService>();
        services.AddSingleton<IAsyncClipboardService>(provider =>
            provider.GetRequiredService<AsyncClipboardService>());
        services.AddSingleton<MobileStringCatalog>();
        services.AddSingleton<MobileShellViewModel>();
        services.AddSingleton<IMobileLifecycleSink>(provider =>
            provider.GetRequiredService<MobileShellViewModel>());

        return services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });
    }
}
