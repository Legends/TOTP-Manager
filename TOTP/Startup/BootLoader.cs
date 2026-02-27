using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Syncfusion.Licensing;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using TOTP.Core.Services;
using TOTP.Core.Services.Interfaces;
using TOTP.Helper;
using TOTP.Infrastructure.Logging;
using TOTP.Infrastructure.Services;
using TOTP.Resources;
using TOTP.Security;
using TOTP.Security.Interfaces;
using TOTP.Security.Models;
using TOTP.Services;
using TOTP.Services.Interfaces;
using TOTP.ViewModels;
using TOTP.ViewModels.Interfaces;
using TOTP.Views;
using static TOTP.ViewModels.SettingsViewModel;

namespace TOTP.Startup;

public static class BootLoader
{
    public static IConfigurationRoot BuildConfiguration()
        => new ConfigurationBuilder()
            .AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true)
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile(StringsConstants.AppSettingsFileName, optional: false, reloadOnChange: true)
            .Build();

    public static void SetCulture(IConfiguration configuration)
    {
        var cultureCode = configuration["Localization:Culture"] ?? "en";
        var culture = new CultureInfo(cultureCode);

        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;
    }

    public static void RegisterSyncfusionLicenseKey(IConfiguration configuration)
    {
        var key = configuration["syncfusion"] ?? Environment.GetEnvironmentVariable("SYNCFUSION_LICENSE");
        if (!string.IsNullOrWhiteSpace(key))
            SyncfusionLicenseProvider.RegisterLicense(key);
    }

    public static IHost BuildHostAndConfigureServices(IConfiguration configuration, string[] args)
        => Host.CreateDefaultBuilder()
            .UseSerilog(LoggingConfigurator.ConfigureWithHostContext, true)
            .ConfigureServices((_, services) =>
            {
                // config
                services.AddSingleton(configuration);

                #region ### BACKGROUND SERVICES  ###
                services.AddHostedService<SessionLockBackgroundService>();

                var cliLevel = LoggingConfigurator.GetLevelFromArgs(args);
                bool hasOverride = cliLevel.HasValue;
                LogEventLevel initialLevel = cliLevel ?? LogEventLevel.Information;

                services.AddSingleton<ILogSwitchService>(sp => new LogSwitchService(initialLevel, hasOverride));
                services.AddHostedService<LogSwitchInitializationBackgroundService>();
                services.AddHostedService<BackupBackgroundService>();

                #region  ### IdleMonitoringService ###
                services.AddSingleton<IdleMonitoringBackgroundService>();
                services.AddHostedService(sp => sp.GetRequiredService<IdleMonitoringBackgroundService>());
                services.AddSingleton<IActivityHeartbeat>(sp => sp.GetRequiredService<IdleMonitoringBackgroundService>());
                #endregion

                #region  ### ClipboardService ###
                services.AddSingleton<ClipboardBackgroundService>();
                services.AddSingleton<IClipboardService>(sp => sp.GetRequiredService<ClipboardBackgroundService>());
                services.AddHostedService(sp => sp.GetRequiredService<ClipboardBackgroundService>());
                #endregion
                #endregion

                #region ### SECURITY & CORE SERVICES ###

                // Security Infrastructure
                services.AddSingleton<IKeyWrappingService, KeyWrappingService>();
                services.AddSingleton<ISecurityContext, SecurityContext>();
                services.AddSingleton<IHelloGate, HelloGate>();

                // Authorization Logic
                services.AddSingleton<IAuthorizationService, AuthorizationService>();
                services.AddSingleton<IMainViewSessionController, MainViewSessionController>();

                // Data Access Layer (FIXED WIRING)
                services.AddSingleton<IOtpDAL>(provider =>
                {
                    var logger = provider.GetRequiredService<ILogger<OtpDAL>>();
                    var securityContext = provider.GetRequiredService<ISecurityContext>();
                    var config = provider.GetRequiredService<IConfiguration>();

                    var fileStoragePathAccounts = config.GetSection(StringsConstants.AccountsStorageFilePath).Value;
                    var resolvedAccountsStorageFilePath = Environment.ExpandEnvironmentVariables(fileStoragePathAccounts ?? "");

                    // Passing the required ISecurityContext now
                    return new OtpDAL(logger, securityContext, resolvedAccountsStorageFilePath);
                });

                services.AddSingleton<IOtpManager, OtpManager>();

                #endregion

                // infra
                services.AddSingleton<IDelayService, DelayService>();
                services.AddSingleton<IDebounceService, DebounceService>();
                services.AddTransient<IUserMessageDialogViewModel, UserMessageDialogViewModel>();
                services.AddSingleton<Func<IUserMessageDialogViewModel>>(sp => () => sp.GetRequiredService<IUserMessageDialogViewModel>());
                services.AddSingleton<IMessageService, MessageService>();
                services.AddTransient<IFileDialogService, FileDialogService>();
                services.AddSingleton<IQrCodeService, QrCodeService>();
                services.AddSingleton<IErrorHandler, ErrorHandler>();

                var rawProfilePath = configuration.GetSection(StringsConstants.GlobalSettingsProfileStorageFilePath).Value;
                var resolvedProfilePath = Environment.ExpandEnvironmentVariables(rawProfilePath ?? "");
                services.AddSingleton<IGlobalProfileStore>(_ => new FileGlobalProfileStore(resolvedProfilePath));

                // VMs & Windows
                services.AddTransient<QrScannerViewModel>();
                services.AddTransient<QrScannerWindow>();
                services.AddSingleton<IQrScannerDialogService, QrScannerDialogService>();
                services.AddSingleton<Func<IQrScannerDialogService>>(sp => () => sp.GetRequiredService<IQrScannerDialogService>());

                services.AddSingleton<IInputActivityMonitor, WpfInputActivityMonitor>();
                services.AddTransient<SettingsViewModel>();

                // Register the Delegate Factory for Settings
                services.AddSingleton<SettingsViewModelFactory>(serviceProvider =>
                    (closeCmd, saveAct, exportTst) =>
                    {
                        var profileStore = serviceProvider.GetRequiredService<IGlobalProfileStore>();
                        var authService = serviceProvider.GetRequiredService<IAuthorizationService>();
                        var logging = serviceProvider.GetRequiredService<ILogSwitchService>();
                        return new SettingsViewModel(profileStore, authService, logging, closeCmd, saveAct, exportTst);
                    });

                services.AddSingleton<UnlockViewModel>();
                services.AddSingleton<HelloUnlockViewModel>();
                services.AddSingleton<PasswordUnlockViewModel>();

                // Master Password configuration for the derived keys
                services.AddSingleton<IMasterPasswordService>(_ => new MasterPasswordService(new PasswordRecord([], [], 4, 128 * 1024)));

                services.AddSingleton<IMainViewModel, MainViewModel>();
                services.AddSingleton<MainWindow>();
            })
            .Build();

    public static void SetupUnhandledExceptionsHooks(Application app, IHost host)
    {
        var logger = host.Services.GetService<ILogger<App>>();
        var messageService = host.Services.GetService<IMessageService>();

        app.DispatcherUnhandledException += (_, e) =>
        {
            try { messageService?.ConfirmError(UI.msg_DispatcherException); }
            catch { MessageBox.Show(e.Exception.Message, "UI Error", MessageBoxButton.OK); }
            logger?.LogCritical(e.Exception, "Unhandled UI thread exception");
            e.Handled = true;
        };

        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            try { messageService?.ConfirmError(UI.ex_FatalError); }
            catch { MessageBox.Show(UI.ex_FatalError, "AppDomain Error", MessageBoxButton.OK); }
            logger?.LogCritical(e.ExceptionObject as Exception, "Unhandled domain exception");
            Environment.Exit(1);
        };

        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            try { messageService?.ShowWarning(UI.msg_BackroundTaskException); }
            catch { MessageBox.Show(UI.msg_BackroundTaskException, "Unobserved Task Exception", MessageBoxButton.OK); }
            logger?.LogCritical(e.Exception, "Unobserved task exception");
            e.SetObserved();
        };
    }
}