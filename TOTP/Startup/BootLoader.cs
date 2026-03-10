#region ### USINGS ###
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Syncfusion.Licensing;
using System;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using TOTP.Core.Enums;
using TOTP.Core.Interfaces;
using TOTP.Core.Security.Interfaces;
using TOTP.Core.Services;
using TOTP.Core.Services.Interfaces;
using TOTP.DAL.Services;
using TOTP.Infrastructure.Common;
using TOTP.Infrastructure.Extensions;
using TOTP.Infrastructure.Logging;
using TOTP.Infrastructure.Security.Provider;
using TOTP.Infrastructure.Services;
using TOTP.Resources;
using TOTP.Security;
using TOTP.Security.Interfaces;
using TOTP.Services;
using TOTP.Services.Interfaces;
using TOTP.ViewModels;
using TOTP.ViewModels.Interfaces;
using TOTP.Views;

#endregion

namespace TOTP.Startup;

public static class BootLoader
{
    public static IConfigurationRoot BuildConfiguration()
        => new ConfigurationBuilder()
            .SetBasePath(StringsConstants.AppRootDirectory)
            .AddJsonFile(StringsConstants.AppSettingsFileName, optional: false, reloadOnChange: true)
            .AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true)
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

                services.AddInfrastructure(configuration);

                #region ### BACKGROUND SERVICES  ###
                services.AddHostedService<SessionLockBackgroundService>();

                var cliLevel = LoggingConfigurator.GetLevelFromArgs(args);
                bool hasOverride = cliLevel.HasValue;
                AppLogLevel initialLevel = cliLevel ?? AppLogLevel.Information;

                services.AddSingleton<ILogSwitchService>(sp => new LogSwitchService(initialLevel, hasOverride));
                //services.AddHostedService<LogSwitchInitializationBackgroundService>();
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

                // 1. Register the platform-specific infrastructure
                services.AddSingleton<IDispatcherService, WpfDispatcherService>();

                #region ### SECURITY & CORE SERVICES ###

                // Security Infrastructure
                services.AddSingleton<IHelloGate, HelloGate>();
                services.AddSingleton<IMainViewSessionController, MainViewSessionController>();

                services.AddSingleton<IAccountManager, AccountManager>();

                #endregion

                // infra
                services.AddSingleton<IDelayService, DelayService>();
                services.AddSingleton<IDebounceService, DebounceService>();
                services.AddSingleton<ILogFileService, LogFileService>();
                services.AddSingleton<IQrPreviewService, QrPreviewService>();
                services.AddSingleton<IScannerWarmupService, ScannerWarmupService>();
                services.AddSingleton<IAutoUpdateService, AutoUpdateService>();
                services.AddSingleton<IPasswordPromptDialogFactory, PasswordPromptDialogFactory>();
                services.AddSingleton<IPasswordPromptService, PasswordPromptService>();
                services.AddSingleton<INotificationUiClient, NotificationUiClient>();
                services.AddSingleton<IMessageService, MessageService>();
                services.AddSingleton<IAccountsWorkflowService, AccountsWorkflowService>();
                services.AddSingleton<IAccountTransferWorkflowService, AccountTransferWorkflowService>();
                services.AddSingleton<ISettingsDialogOrchestrationService, SettingsDialogOrchestrationService>();
                services.AddSingleton<ISettingsAuthorizationWorkflowService, SettingsAuthorizationWorkflowService>();
                services.AddSingleton<ISettingsPersistenceService, SettingsPersistenceService>();
                services.AddTransient<IFileDialogService, FileDialogService>();
                services.AddSingleton<IQrCodeService, QrCodeService>();
                services.AddTransient<IQrScannerRunner, QrScannerRunner>();
                services.AddTransient<IVideoCaptureFactory, OpenCvVideoCaptureFactory>();
                services.AddTransient<IQrCodeDecoder, OpenCvQrCodeDecoder>();
                services.AddTransient<IFrameBitmapSourceConverter, MatBitmapSourceConverter>();



                // VMs & Windows
                services.AddTransient<QrScannerViewModel>();
                services.AddTransient<QrScannerWindow>();
                services.AddSingleton<IQrScannerDialogService, QrScannerDialogService>();
                services.AddSingleton<Func<IQrScannerDialogService>>(sp => () => sp.GetRequiredService<IQrScannerDialogService>());

                services.AddSingleton<IInputActivityMonitor, WpfInputActivityMonitor>();
                services.AddTransient<SettingsViewModel>();

                services.AddSingleton<UnlockViewModel>();
                services.AddSingleton<HelloUnlockViewModel>();
                services.AddSingleton<PasswordUnlockViewModel>();

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
