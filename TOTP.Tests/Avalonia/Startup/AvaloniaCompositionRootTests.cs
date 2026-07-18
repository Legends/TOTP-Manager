using Avalonia.Controls.ApplicationLifetimes;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TOTP.Core.Services.Interfaces;
using TOTP.Avalonia.Desktop.Startup;
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
    }
}
