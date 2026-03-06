using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TOTP.Core.Interfaces;
using TOTP.Core.Security;
using TOTP.Core.Security.Interfaces;
using TOTP.Core.Services.Interfaces;
using TOTP.DAL.Services;
using TOTP.Infrastructure.Common;
using TOTP.Infrastructure.Security;
using TOTP.Infrastructure.Services;

namespace TOTP.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {

        var rawProfilePath = configuration.GetSection(StringsConstants.AppSettingsStorageFilePathConfigKey).Value;
        var resolvedProfilePath = Environment.ExpandEnvironmentVariables(rawProfilePath ?? "");
        services.AddSingleton<IAppSettingsDAL>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<AppSettingsDAL>>();
            return new AppSettingsDAL(resolvedProfilePath, logger);
        });
       
        services.AddSingleton<ISettingsService, SettingsService>();

        // 1. Master Password & Security Context
        services.AddSingleton<ISecurityContext, SecurityContext>();
        services.AddTransient<IMasterPasswordService, MasterPasswordService>();
        services.AddSingleton<IPasswordValidationService, PasswordValidationService>();

        // 2. The Vault & DAL logic
        services.AddSingleton<IVaultService, VaultService>();
        services.AddSingleton<IExportService, ExportService>();

        services.AddSingleton<IOtpDAL>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<OtpDAL>>();
            var vault = sp.GetRequiredService<IVaultService>();
            var path = configuration[StringsConstants.TokensStorageFilePathConfigKey]
                       ?? "master.totp";

            return new OtpDAL(logger, vault, path);
        });

        // 3. Authorization Logic (The bridge)
        services.AddSingleton<IAuthorizationService, AuthorizationService>();
        services.AddSingleton<AuthorizationState>();

        return services;
    }
}
