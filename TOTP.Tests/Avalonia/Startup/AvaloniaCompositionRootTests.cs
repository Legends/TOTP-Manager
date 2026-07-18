using Avalonia.Controls.ApplicationLifetimes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TOTP.Core.Security.Interfaces;
using TOTP.Core.Services.Interfaces;
using TOTP.Core.Platform;
using TOTP.Avalonia.Desktop.Startup;
using TOTP.Avalonia.Desktop.Presentation;
using TOTP.Avalonia.Desktop.Platform;
using TOTP.Infrastructure.Security;
using TOTP.Infrastructure.Services;
using TOTP.Platform.Windows;
using TOTP.Platform.Windows.Security;
using AppLifetime = TOTP.Core.Services.Interfaces.IApplicationLifetime;
using TOTP.Avalonia.Desktop.Localization;

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
        Assert.IsType<WindowsPlatformQuickUnlock>(
            services.GetRequiredService<IPlatformQuickUnlock>());
        Assert.IsType<AvaloniaHelloPromptWindowHandleProvider>(
            services.GetRequiredService<IHelloPromptWindowHandleProvider>());
        Assert.IsType<HelloGate>(services.GetRequiredService<IHelloGate>());
        Assert.IsType<AccountManager>(
            services.GetRequiredService<IAccountManager>());
        Assert.IsType<AvaloniaStartupCoordinator>(
            services.GetRequiredService<IAvaloniaStartupCoordinator>());
        Assert.IsType<AvaloniaExceptionBoundary>(
            services.GetRequiredService<AvaloniaExceptionBoundary>());
        Assert.IsType<MainWindowViewModel>(
            services.GetRequiredService<MainWindowViewModel>());
        Assert.IsType<PasswordUnlockViewModel>(
            services.GetRequiredService<PasswordUnlockViewModel>());
        Assert.IsType<PasswordSetupViewModel>(
            services.GetRequiredService<PasswordSetupViewModel>());
        Assert.IsType<AccountListViewModel>(
            services.GetRequiredService<AccountListViewModel>());
        Assert.IsType<AccountQrCodeService>(
            services.GetRequiredService<IAccountQrCodeService>());
        Assert.IsType<AvaloniaQrImageFactory>(
            services.GetRequiredService<IAvaloniaQrImageFactory>());
        Assert.IsType<AvaloniaFilePicker>(
            services.GetRequiredService<IAvaloniaFilePicker>());
        Assert.IsType<AvaloniaDialogService>(
            services.GetRequiredService<IAvaloniaDialogService>());
        Assert.IsType<AvaloniaLocalizationService>(
            services.GetRequiredService<IAvaloniaLocalizationService>());
        Assert.IsType<NativeFilePickerViewModel>(
            services.GetRequiredService<NativeFilePickerViewModel>());
        Assert.IsType<NamedPipeActivationListener>(
            services.GetRequiredService<IActivationListener>());
        Assert.IsType<SettingsPageViewModel>(
            services.GetRequiredService<SettingsPageViewModel>());
        Assert.IsType<AsyncClipboardService>(
            services.GetRequiredService<IAsyncClipboardService>());
    }
}
