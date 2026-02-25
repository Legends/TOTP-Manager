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

    /// <summary>
    /// Builds an application host and configures all required services for the application using the specified
    /// configuration.
    /// </summary>
    /// <remarks>The returned host includes:
    /// - logging configured with Serilog and
    /// - registers core infrastructure, application, and view model services as singletons or transients as appropriate.
    /// 
    /// The provided configuration is
    /// made available to all services via dependency injection. Callers are responsible for managing the host's
    /// lifetime.</remarks>
    /// <param name="configuration">The application configuration to use for service registration and initialization. Cannot be null.</param>
    /// <param name="args"></param>
    /// <returns>An initialized <see cref="IHost"/> instance with all application services configured and ready for use.</returns>
    public static IHost BuildHostAndConfigureServices(IConfiguration configuration, string[] args)
        => Host.CreateDefaultBuilder()
            .UseSerilog(LoggingConfigurator.ConfigureWithHostContext, true)
            .ConfigureServices((_, services) =>
            {
                // config
                services.AddSingleton(configuration);

                #region ### BACKGROUND SERVICES  ###

                services.AddHostedService<SessionLockService>();

                // 1. Detect if a CLI override exists
                var cliLevel = LoggingConfigurator.GetLevelFromArgs(args);
                bool hasOverride = cliLevel.HasValue;
                LogEventLevel initialLevel = cliLevel ?? LogEventLevel.Information;

                // 3. Register the service with the detected values
                services.AddSingleton<ILogSwitchService>(sp => new LogSwitchService(initialLevel, hasOverride));
                services.AddHostedService<LogSwitchInitializationService>();
                services.AddHostedService<BackupService>();

                #region  ### IdleMonitoringService ###


                // 1.Register the concrete class as a Singleton
                services.AddSingleton<IdleMonitoringService>();

                // 2. Tell the Host to run it as a BackgroundService 
                // (We resolve the singleton we just registered)
                services.AddHostedService(sp => sp.GetRequiredService<IdleMonitoringService>());

                // 3. Tell the DI that IActivityHeartbeat points to that SAME singleton instance
                services.AddSingleton<IActivityHeartbeat>(sp => sp.GetRequiredService<IdleMonitoringService>());


                #endregion

                #region  ### ClipboardService ###
                // Register as a singleton so it can be injected as IClipboardService
                services.AddSingleton<ClipboardService>();

                // Map the Interface to the singleton
                services.AddSingleton<IClipboardService>(sp => sp.GetRequiredService<ClipboardService>());

                // Register the singleton as a HostedService so it runs ExecuteAsync
                services.AddHostedService(sp => sp.GetRequiredService<ClipboardService>());

                #endregion
                #endregion


                // infra
                services.AddSingleton<IDelayService, DelayService>();
                services.AddSingleton<IDebounceService, DebounceService>();


                services.AddTransient<IUserMessageDialogViewModel, UserMessageDialogViewModel>();

                services.AddSingleton<Func<IUserMessageDialogViewModel>>(sp => () => sp.GetRequiredService<IUserMessageDialogViewModel>());
                services.AddSingleton<IMessageService, MessageService>();


                services.AddTransient<IFileDialogService, FileDialogService>();
                services.AddSingleton<IQrCodeService, QrCodeService>();

                // app services
                services.AddSingleton<IOtpDAL>(provider =>
                {
                    var logger = provider.GetRequiredService<ILogger<OtpDAL>>();
                    var config = provider.GetRequiredService<IConfiguration>();
                    var fileStoragePathAccounts = config.GetSection(StringsConstants.AccountsStorageFilePath).Value;
                    var resolvedAccountsStorageFilePath = Environment.ExpandEnvironmentVariables(fileStoragePathAccounts ?? "");
                    return new OtpDAL(logger, resolvedAccountsStorageFilePath);
                });

                services.AddSingleton<IErrorHandler, ErrorHandler>();
                services.AddSingleton<IOtpManager, OtpManager>();

                var rawProfilePath = configuration.GetSection(StringsConstants.GlobalSettingsProfileStorageFilePath).Value;
                var resolvedProfilePath = Environment.ExpandEnvironmentVariables(rawProfilePath ?? "");
                services.AddSingleton<IGlobalProfileStore>(_ => new FileGlobalProfileStore(resolvedProfilePath));

                // VMs & Windows
                services.AddTransient<QrScannerViewModel>();
                services.AddTransient<QrScannerWindow>();

                // dialog / orchestration
                services.AddSingleton<IQrScannerDialogService, QrScannerDialogService>();
                services.AddSingleton<Func<IQrScannerDialogService>>(sp =>
                    () => sp.GetRequiredService<IQrScannerDialogService>());

                services.AddSingleton<IAuthorizationService, AuthorizationService>();
                //services.AddSingleton<IUserActivityService, UserActivityService>();
                services.AddSingleton<IInputActivityMonitor, WpfInputActivityMonitor>();
                services.AddSingleton<IMainViewSessionController, MainViewSessionController>();

                services.AddTransient<SettingsViewModel>();

                // Register the Delegate Factory
                services.AddSingleton<SettingsViewModelFactory>(serviceProvider =>
                    (closeCmd, saveAct, exportTst) =>
                    {
                        // Resolve the services from DI
                        var profileStore = serviceProvider.GetRequiredService<IGlobalProfileStore>();
                        var authService = serviceProvider.GetRequiredService<IAuthorizationService>();
                        var logging = serviceProvider.GetRequiredService<ILogSwitchService>();

                        // Create the VM manually with a mix of DI and parameters
                        return new SettingsViewModel(profileStore, authService, logging, closeCmd, saveAct, exportTst);
                    });


                services.AddSingleton<UnlockViewModel>();
                services.AddSingleton<HelloUnlockViewModel>();
                services.AddSingleton<PasswordUnlockViewModel>();

                services.AddSingleton<IHelloGate, HelloGate>();
                services.AddSingleton<IPasswordService>(_ => new PasswordService(new PasswordRecord([], [], 100_000)));

                services.AddSingleton<IMainViewModel, MainViewModel>();
                services.AddSingleton<MainWindow>();
            })
            .Build();

    public static void SetupUnhandledExceptionsHooks(Application app, IHost host)
    {
        var logger = host.Services.GetService<ILogger<App>>();
        var messageService = host.Services.GetService<IMessageService>();

        // Dispatcher (UI) thread exceptions
        app.DispatcherUnhandledException += (_, e) =>
        {
            try
            {
                messageService?.ConfirmError(UI.msg_DispatcherException);
            }
            catch
            {
                MessageBox.Show(e.Exception.Message, "UI Error", MessageBoxButton.OK);
            }

            logger?.LogCritical(e.Exception, "Unhandled UI thread exception");
            e.Handled = true;
        };

        // Non-UI thread exceptions
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            try
            {
                messageService?.ConfirmError(UI.ex_FatalError);
            }
            catch
            {
                MessageBox.Show(UI.ex_FatalError, "AppDomain Error", MessageBoxButton.OK);
            }

            logger?.LogCritical(e.ExceptionObject as Exception, "Unhandled domain exception");
            Environment.Exit(1);
        };

        // Unobserved task exceptions
        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            try
            {
                messageService?.ShowWarning(UI.msg_BackroundTaskException);
            }
            catch
            {
                MessageBox.Show(UI.msg_BackroundTaskException, "Unobserved Task Exception", MessageBoxButton.OK);
            }

            logger?.LogCritical(e.Exception, "Unobserved task exception");
            e.SetObserved();
        };
    }
}