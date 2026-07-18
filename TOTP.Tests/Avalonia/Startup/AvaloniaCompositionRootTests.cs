using Avalonia.Controls.ApplicationLifetimes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TOTP.Core.Security.Interfaces;
using TOTP.Core.Services.Interfaces;
using TOTP.Avalonia.Desktop.Startup;
using TOTP.Avalonia.Desktop.Presentation;
using TOTP.Infrastructure.Security;
using TOTP.Infrastructure.Services;
using TOTP.Platform.Windows;
using AppLifetime = TOTP.Core.Services.Interfaces.IApplicationLifetime;

namespace TOTP.Tests.Avalonia.Startup;

public sealed class AvaloniaCompositionRootTests
{
    [Fact]
    public void Build_RegistersAvaloniaPlatformContracts()
    {
        var desktopLifetime = new Mock<IClassicDesktopStyleApplicationLifetime>().Object;

        using var services = AvaloniaCompositionRoot.Build(desktopLifetime);

        Assert.Same(
            desktopLifetime,
            services.GetRequiredService<IClassicDesktopStyleApplicationLifetime>());
        Assert.NotNull(services.GetRequiredService<IUiScheduler>());
        Assert.NotNull(services.GetRequiredService<AppLifetime>());
        Assert.IsType<WindowsApplicationPaths>(
            services.GetRequiredService<IPlatformApplicationPaths>());
        Assert.IsType<WindowsFileSecurity>(
            services.GetRequiredService<IPlatformFileSecurity>());
        Assert.NotNull(services.GetRequiredService<IConfiguration>());
        Assert.IsType<PortableSettingsService>(
            services.GetRequiredService<ISettingsService>());
        Assert.IsType<PortableAuthorizationService>(
            services.GetRequiredService<IAuthorizationService>());
        Assert.IsType<PlatformQuickUnlockEnrollment>(
            services.GetRequiredService<IPlatformQuickUnlockEnrollment>());
        Assert.IsType<UnavailablePlatformQuickUnlock>(
            services.GetRequiredService<IPlatformQuickUnlock>());
        Assert.IsType<AccountManager>(
            services.GetRequiredService<IAccountManager>());
        Assert.IsType<AvaloniaStartupCoordinator>(
            services.GetRequiredService<IAvaloniaStartupCoordinator>());
        Assert.IsType<MainWindowViewModel>(
            services.GetRequiredService<MainWindowViewModel>());
        Assert.IsType<PasswordUnlockViewModel>(
            services.GetRequiredService<PasswordUnlockViewModel>());
        Assert.IsType<AccountListViewModel>(
            services.GetRequiredService<AccountListViewModel>());
        Assert.IsType<SettingsPageViewModel>(
            services.GetRequiredService<SettingsPageViewModel>());
        Assert.IsType<AsyncClipboardService>(
            services.GetRequiredService<IAsyncClipboardService>());
    }
}
