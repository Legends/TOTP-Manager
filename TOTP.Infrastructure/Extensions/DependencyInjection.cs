using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TOTP.Core.Common;
using TOTP.Core.Security;
using TOTP.Core.Security.Interfaces;
using TOTP.Core.Services.Interfaces;
using TOTP.DAL.Services;
using TOTP.Infrastructure.Security;
using TOTP.Infrastructure.Services;
using TOTP.Infrastructure.Platform;

namespace TOTP.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IPlatformApplicationPaths applicationPaths)
    {
        services.AddSingleton<IPlatformFileSecurity, WindowsFileSecurity>();

        var rawProfilePath = configuration.GetSection(StringsConstants.AppSettingsStorageFilePathConfigKey).Value;
        var resolvedProfilePath = string.IsNullOrWhiteSpace(rawProfilePath)
            ? applicationPaths.SettingsFilePath
            : Environment.ExpandEnvironmentVariables(rawProfilePath);
        var rawVaultPath = configuration[StringsConstants.TokensStorageFilePathConfigKey];
        var resolvedVaultPath = string.IsNullOrWhiteSpace(rawVaultPath)
            ? applicationPaths.VaultFilePath
            : Environment.ExpandEnvironmentVariables(rawVaultPath);
        services.AddSingleton<IAppSettingsDAL>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<AppSettingsDAL>>();
            var fileSecurity = sp.GetRequiredService<IPlatformFileSecurity>();
            return new AppSettingsDAL(resolvedProfilePath, logger, fileSecurity);
        });
        services.AddSingleton<IAuthorizationEnvelopeStore>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<AuthorizationEnvelopeStore>>();
            var fileSecurity = sp.GetRequiredService<IPlatformFileSecurity>();
            return new AuthorizationEnvelopeStore(
                applicationPaths.AuthorizationEnvelopeFilePath,
                logger,
                fileSecurity);
        });
        services.AddSingleton<IAppPreferencesStore>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<AppPreferencesStore>>();
            var fileSecurity = sp.GetRequiredService<IPlatformFileSecurity>();
            return new AppPreferencesStore(applicationPaths.PreferencesFilePath, logger, fileSecurity);
        });
       
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<ITotpGenerator, OtpNetTotpGenerator>();

        // 1. Master Password & Security Context
        services.AddSingleton<ISecurityContext, SecurityContext>();
        services.AddTransient<IMasterPasswordService, MasterPasswordService>();
        services.AddSingleton<IPasswordValidationService, PasswordValidationService>();
        services.AddSingleton<IAuthorizationEnvelopeActivator, AuthorizationEnvelopeActivator>();

        // 2. The Vault & DAL logic
        services.AddSingleton<VaultService>();
        services.AddSingleton<IVaultService>(sp => sp.GetRequiredService<VaultService>());
        services.AddSingleton<IVaultKeyVerifier>(sp => sp.GetRequiredService<VaultService>());
        services.AddSingleton<IStoredVaultKeyVerifier>(sp => new StoredVaultKeyVerifier(
            resolvedVaultPath,
            sp.GetRequiredService<IVaultKeyVerifier>(),
            sp.GetRequiredService<ILogger<StoredVaultKeyVerifier>>(),
            sp.GetRequiredService<IPlatformFileSecurity>()));
        services.AddSingleton<IExportService, ExportService>();

        services.AddSingleton<IAccountDAL>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<AccountDAL>>();
            var vault = sp.GetRequiredService<IVaultService>();
            var fileSecurity = sp.GetRequiredService<IPlatformFileSecurity>();
            return new AccountDAL(logger, vault, resolvedVaultPath, fileSecurity);
        });

        // 3. Authorization Logic (The bridge)
        services.AddSingleton<IAuthorizationService, AuthorizationService>();
        services.AddSingleton<AuthorizationState>();

        return services;
    }
}
