using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls;
using System.Globalization;
using Avalonia.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TOTP.Core.Security.Interfaces;
using TOTP.Core.Platform;
using TOTP.Core.Services.Interfaces;
using TOTP.Avalonia.Desktop.Platform;
using TOTP.Avalonia.Desktop.Presentation;
using TOTP.Infrastructure.Extensions;
using TOTP.Infrastructure.Security;
using TOTP.Infrastructure.Services;
using TOTP.Camera.OpenCv;
using TOTP.Avalonia.Desktop.Localization;
using Serilog;
using AppLifetime = TOTP.Core.Services.Interfaces.IApplicationLifetime;
#if TOTP_PLATFORM_WINDOWS
using TOTP.Platform.Windows.Security;
#endif

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
        services.AddSingleton(Application.Current?.Resources ?? new ResourceDictionary());
        services.AddSingleton<AvaloniaStringCatalog>();
        services.AddSingleton<IAvaloniaLocalizationService, AvaloniaLocalizationService>();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddSingleton<IPlatformApplicationPaths>(platformServices.ApplicationPaths);
        services.AddLogging(builder => builder.AddSerilog(Log.Logger, dispose: false));
#if TOTP_PLATFORM_WINDOWS
        services.AddSingleton<IHelloPromptWindowHandleProvider, AvaloniaHelloPromptWindowHandleProvider>();
        services.AddSingleton<IHelloVerificationRequester, WindowsHelloVerificationRequester>();
        services.AddSingleton<IHelloGate, HelloGate>();
        services.AddSingleton<IPlatformQuickUnlock, WindowsPlatformQuickUnlock>();
#else
        services.AddSingleton<IPlatformQuickUnlock, UnavailablePlatformQuickUnlock>();
#endif
        services.AddInfrastructure(
            configuration,
            platformServices.ApplicationPaths,
            platformServices.FileSecurity);
        services.AddSingleton(new AvaloniaUiScheduler(Dispatcher.UIThread));
        services.AddSingleton<IUiScheduler>(provider =>
            provider.GetRequiredService<AvaloniaUiScheduler>());
        services.AddSingleton<AppLifetime, AvaloniaApplicationLifetime>();
        services.AddSingleton<IActivationListener>(
            new NamedPipeActivationListener(DesktopInstanceIdentity.PipeName));
        services.AddSingleton<AvaloniaClipboardAccessor>();
        services.AddSingleton<AvaloniaStorageProviderAccessor>();
        services.AddSingleton<AvaloniaWindowCoordinator>();
        services.AddSingleton<IAvaloniaDialogService, AvaloniaDialogService>();
        services.AddSingleton<IAvaloniaFilePicker, AvaloniaFilePicker>();
        services.AddSingleton<IAvaloniaQrImageFactory, AvaloniaQrImageFactory>();
        services.AddSingleton<IPlatformFolderLauncher, AvaloniaPlatformFolderLauncher>();
        services.AddSingleton<IAsyncPlatformClipboard>(provider =>
            new AvaloniaPlatformClipboard(
                provider.GetRequiredService<AvaloniaClipboardAccessor>(),
                SupportsClipboardOwnership(),
                provider.GetRequiredService<ILogger<AvaloniaPlatformClipboard>>()));
        services.AddSingleton<AsyncClipboardService>();
        services.AddSingleton<IAsyncClipboardService>(provider =>
            provider.GetRequiredService<AsyncClipboardService>());
        services.AddSingleton<IAvaloniaStartupCoordinator, AvaloniaStartupCoordinator>();
        services.AddSingleton<AvaloniaExceptionBoundary>();
        services.AddSingleton<ICameraSessionFactory, OpenCvCameraSessionFactory>();
        services.AddSingleton<IQrScannerRunner, OpenCvQrScannerRunner>();
        services.AddSingleton<PasswordUnlockViewModel>();
        services.AddSingleton<PasswordSetupViewModel>();
        services.AddSingleton<AccountListViewModel>();
        services.AddSingleton<SettingsPageViewModel>();
        services.AddSingleton<AuthorizationSettingsViewModel>();
        services.AddSingleton<NativeFilePickerViewModel>();
        services.AddSingleton<CameraScannerViewModel>();
        services.AddSingleton<UpdateCheckViewModel>();
        services.AddSingleton<DiagnosticsViewModel>();
        services.AddSingleton<MainWindowViewModel>();
        services.AddTransient(provider => new MainWindow
        {
            DataContext = provider.GetRequiredService<MainWindowViewModel>(),
            ClipboardAccessor = provider.GetRequiredService<AvaloniaClipboardAccessor>(),
            StorageProviderAccessor = provider.GetRequiredService<AvaloniaStorageProviderAccessor>(),
            WindowCoordinator = provider.GetRequiredService<AvaloniaWindowCoordinator>()
        });

        var provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });
        provider.GetRequiredService<IAvaloniaLocalizationService>()
            .ApplyCulture(CultureInfo.CurrentUICulture.Name);
        return provider;
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
