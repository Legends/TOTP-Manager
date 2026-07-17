using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TOTP.Core.Security.Interfaces;
using TOTP.Infrastructure.Platform;
using TOTP.Infrastructure.Security;
using TOTP.Infrastructure.Security.Provider;
using TOTP.Infrastructure.Services;
using TOTP.Startup;

namespace TOTP.Tests.Startup;

public sealed class BootLoaderCompositionTests : IDisposable
{
    private readonly string _root = Path.Combine(
        Path.GetTempPath(),
        $"totp-bootloader-tests-{Guid.NewGuid():N}");

    [Fact]
    public void BuildHost_ActivatesPortableSettingsAndAuthorizationGraph()
    {
        var paths = new WindowsApplicationPaths(_root, _root);
        var configuration = new ConfigurationBuilder().Build();

        using var host = BootLoader.BuildHostAndConfigureServices(
            configuration,
            [],
            paths);

        Assert.IsType<PortableSettingsService>(
            host.Services.GetRequiredService<ISettingsService>());
        Assert.IsType<PortableAuthorizationService>(
            host.Services.GetRequiredService<IAuthorizationService>());
        Assert.IsType<AuthorizationEnvelopePasswordLifecycle>(
            host.Services.GetRequiredService<IAuthorizationEnvelopePasswordLifecycle>());
        Assert.IsType<PlatformQuickUnlockEnrollment>(
            host.Services.GetRequiredService<IPlatformQuickUnlockEnrollment>());
        Assert.IsType<WindowsPlatformQuickUnlock>(
            host.Services.GetRequiredService<IPlatformQuickUnlock>());
        Assert.Null(host.Services.GetService<IAppSettingsDAL>());
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_root))
                Directory.Delete(_root, recursive: true);
        }
        catch
        {
            // best-effort cleanup
        }
    }
}
