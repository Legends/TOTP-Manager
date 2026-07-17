using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using TOTP.Core.Security.Interfaces;
using TOTP.Core.Services.Interfaces;
using TOTP.DAL.Services;
using TOTP.Infrastructure.Extensions;
using TOTP.Infrastructure.Platform;

namespace TOTP.Tests.Infrastructure.Extensions;

public sealed class DependencyInjectionPathTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"totp-path-di-tests-{Guid.NewGuid():N}");

    [Fact]
    public void AddInfrastructure_WhenStorageOverridesAreMissing_UsesPlatformDefaults()
    {
        var paths = new WindowsApplicationPaths(_root, _root);
        using var provider = BuildProvider(new ConfigurationBuilder().Build(), paths);

        var settingsDal = Assert.IsType<AppSettingsDAL>(provider.GetRequiredService<IAppSettingsDAL>());
        var accountDal = Assert.IsType<AccountDAL>(provider.GetRequiredService<IAccountDAL>());
        var envelopeStore = Assert.IsType<AuthorizationEnvelopeStore>(
            provider.GetRequiredService<IAuthorizationEnvelopeStore>());
        var preferencesStore = Assert.IsType<AppPreferencesStore>(
            provider.GetRequiredService<IAppPreferencesStore>());
        var vaultService = provider.GetRequiredService<IVaultService>();
        var vaultKeyVerifier = provider.GetRequiredService<IVaultKeyVerifier>();
        var storedVaultVerifier = Assert.IsType<StoredVaultKeyVerifier>(
            provider.GetRequiredService<IStoredVaultKeyVerifier>());

        Assert.IsType<WindowsFileSecurity>(provider.GetRequiredService<IPlatformFileSecurity>());
        Assert.Same(vaultService, vaultKeyVerifier);
        Assert.Equal(paths.SettingsFilePath, ReadPath(settingsDal, "_path"));
        Assert.Equal(paths.VaultFilePath, ReadPath(accountDal, "_secretsPath"));
        Assert.Equal(paths.VaultFilePath, ReadPath(storedVaultVerifier, "_path"));
        Assert.Equal(paths.AuthorizationEnvelopeFilePath, ReadPath(envelopeStore, "_path"));
        Assert.Equal(paths.PreferencesFilePath, ReadPath(preferencesStore, "_path"));
    }

    [Fact]
    public void AddInfrastructure_WhenStorageOverridesExist_PreservesConfiguredLocations()
    {
        var paths = new WindowsApplicationPaths(_root, _root);
        var configuredSettingsPath = Path.Combine(_root, "custom", "profile.totp");
        var configuredVaultPath = Path.Combine(_root, "custom", "accounts.totp");
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AppSettings:StorageFilePath"] = configuredSettingsPath,
                ["Accounts:StorageFilePath"] = configuredVaultPath
            })
            .Build();

        using var provider = BuildProvider(configuration, paths);

        Assert.Equal(
            configuredSettingsPath,
            ReadPath(Assert.IsType<AppSettingsDAL>(provider.GetRequiredService<IAppSettingsDAL>()), "_path"));
        Assert.Equal(
            configuredVaultPath,
            ReadPath(Assert.IsType<AccountDAL>(provider.GetRequiredService<IAccountDAL>()), "_secretsPath"));
        Assert.Equal(
            configuredVaultPath,
            ReadPath(Assert.IsType<StoredVaultKeyVerifier>(
                provider.GetRequiredService<IStoredVaultKeyVerifier>()), "_path"));
    }

    private static ServiceProvider BuildProvider(
        IConfiguration configuration,
        IPlatformApplicationPaths paths)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddInfrastructure(configuration, paths);
        return services.BuildServiceProvider();
    }

    private static string ReadPath(object instance, string fieldName)
    {
        var field = instance.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(field);
        return Assert.IsType<string>(field!.GetValue(instance));
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_root))
            {
                Directory.Delete(_root, recursive: true);
            }
        }
        catch
        {
            // best-effort cleanup
        }
    }
}
