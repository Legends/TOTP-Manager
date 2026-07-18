#region ### USINGS ###
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Syncfusion.Licensing;
using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using TOTP.Core.Enums;
using TOTP.Core.Platform;
using TOTP.Core.Security.Interfaces;
using TOTP.Core.Services;
using TOTP.Core.Services.Interfaces;
using TOTP.Camera.OpenCv;
using TOTP.Infrastructure.Extensions;
using TOTP.Infrastructure.Logging;
using TOTP.Infrastructure.Security;
using TOTP.Infrastructure.Services;
using TOTP.Platform.Windows;
using TOTP.Platform.Windows.Security;
using TOTP.Presentation.Services;
using TOTP.Resources;
using TOTP.Presentation.Services.Interfaces;
using TOTP.Presentation.Platform;
using TOTP.Services;
using TOTP.Services.Interfaces;
using TOTP.ViewModels;
using TOTP.ViewModels.Interfaces;
using TOTP.Views;

#endregion

namespace TOTP.Startup;

public static class BootLoader
{
    public static IConfigurationRoot BuildConfiguration(IPlatformApplicationPaths applicationPaths)
    {
        var configurationDirectory = Path.GetDirectoryName(applicationPaths.ConfigurationFilePath)
            ?? throw new InvalidOperationException("The application configuration directory is unavailable.");

        return new ConfigurationBuilder()
            .SetBasePath(configurationDirectory)
            .AddJsonFile(Path.GetFileName(applicationPaths.ConfigurationFilePath), optional: false, reloadOnChange: true)
            .AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true)
            .Build();
    }

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

    public static IHost BuildHostAndConfigureServices(
        IConfiguration configuration,
        string[] args,
        IPlatformApplicationPaths applicationPaths)
        => Host.CreateDefaultBuilder()
            .UseSerilog(LoggingConfigurator.ConfigureWithHostContext, true)
            .ConfigureServices((_, services) =>
            {
                // config
                services.AddSingleton(configuration);
                services.AddSingleton(applicationPaths);

                services.AddInfrastructure(configuration, applicationPaths, new WindowsFileSecurity());

                #region ### BACKGROUND SERVICES  ###
                services.AddHostedService<SessionLockPolicyBackgroundService>();

                var cliLevel = LoggingConfigurator.GetLevelFromArgs(args);
                bool hasOverride = cliLevel.HasValue;
                AppLogLevel initialLevel = cliLevel ?? AppLogLevel.Information;

                services.AddSingleton<ILogSwitchService>(sp => new LogSwitchService(initialLevel, hasOverride));
                services.AddHostedService<BackupBackgroundService>();

                #region  ### IdleMonitoringService ###
                services.AddSingleton<IdleMonitoringBackgroundService>();
                services.AddHostedService(sp => sp.GetRequiredService<IdleMonitoringBackgroundService>());
                services.AddSingleton<IActivityHeartbeat>(sp => sp.GetRequiredService<IdleMonitoringBackgroundService>());
                #endregion

                #region  ### ClipboardService ###
                services.AddSingleton<IPlatformClipboard, WpfClipboard>();
                services.AddSingleton<ClipboardBackgroundService>();
                services.AddSingleton<IClipboardService>(sp => sp.GetRequiredService<ClipboardBackgroundService>());
                services.AddHostedService(sp => sp.GetRequiredService<ClipboardBackgroundService>());
                #endregion
                #endregion

                // 1. Register the platform-specific infrastructure
                services.AddSingleton<IUiScheduler, WpfDispatcherService>();
                services.AddSingleton<TOTP.Core.Services.Interfaces.IApplicationLifetime, WpfApplicationLifetime>();
                services.AddSingleton<ISettingsWindowCoordinator, SettingsWindowCoordinator>();
                services.AddSingleton<WindowsPlatformEventSource>();
                services.AddSingleton<IPlatformSessionEventSource>(sp => sp.GetRequiredService<WindowsPlatformEventSource>());
                services.AddSingleton<IPlatformLifecycleEventSource>(sp => sp.GetRequiredService<WindowsPlatformEventSource>());

                #region ### SECURITY & CORE SERVICES ###

                // Security Infrastructure
                services.AddSingleton<IHelloGate, HelloGate>();
                services.AddSingleton<IPlatformQuickUnlock, WindowsPlatformQuickUnlock>();
                services.AddSingleton<IHelloPromptWindowHandleProvider, HelloPromptWindowHandleProvider>();
                services.AddSingleton<IHelloVerificationRequester, WindowsHelloVerificationRequester>();
                services.AddSingleton<IMainViewSessionController, MainViewSessionController>();

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
                services.AddSingleton<ILocalizationService, LocalizationService>();
                services.AddSingleton<IAccountsWorkflowService, AccountsWorkflowService>();
                services.AddSingleton<IAccountTransferWorkflowService, AccountTransferWorkflowService>();
                services.AddSingleton<IQrAccountImportWorkflow, QrAccountImportWorkflow>();
                services.AddSingleton<ISettingsDialogOrchestrationService, SettingsDialogOrchestrationService>();
                services.AddSingleton<ISettingsAuthorizationWorkflowService, SettingsAuthorizationWorkflowService>();
                services.AddSingleton<ISettingsPersistenceService, SettingsPersistenceService>();
                services.AddTransient<IFileDialogService, FileDialogService>();
                services.AddTransient<ICameraSessionFactory, OpenCvCameraSessionFactory>();
                services.AddTransient<IQrScannerRunner, OpenCvQrScannerRunner>();
                services.AddSingleton<ICameraBackendWarmup, OpenCvCameraBackendWarmup>();



                // VMs & Windows
                services.AddTransient<QrScannerViewModel>();
                services.AddTransient<QrScannerWindow>();
                services.AddSingleton<IQrScannerDialogService>(sp =>
                    new QrScannerDialogService(() => sp.GetRequiredService<QrScannerWindow>()));

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
            catch { MessageBox.Show(e.Exception.Message, UI.ui_Error_UI, MessageBoxButton.OK); }
            logger?.LogCritical(e.Exception, "Unhandled UI thread exception");
            e.Handled = true;
        };

        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            try { messageService?.ConfirmError(UI.ex_FatalError); }
            catch { MessageBox.Show(UI.ex_FatalError, UI.ui_Error_AppDomain, MessageBoxButton.OK); }
            logger?.LogCritical(e.ExceptionObject as Exception, "Unhandled domain exception");
            Environment.Exit(1);
        };

        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            try { messageService?.ShowWarning(UI.msg_BackroundTaskException); }
            catch { MessageBox.Show(UI.msg_BackroundTaskException, UI.ui_Error_UnobservedTaskException, MessageBoxButton.OK); }
            logger?.LogCritical(e.Exception, "Unobserved task exception");
            e.SetObserved();
        };
    }
}
