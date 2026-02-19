using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Syncfusion.Licensing;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using TOTP.Core.Services;
using TOTP.Core.Services.Interfaces;
using TOTP.Helper;
using TOTP.Infrastructure.Logging;
using TOTP.Resources;
using TOTP.Security;
using TOTP.Security.Interfaces;
using TOTP.Security.Models;
using TOTP.Services;
using TOTP.Services.Interfaces;
using TOTP.ViewModels;
using TOTP.Views;

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

        //LocalizationService.ChangeCulture(cultureCode);
    }

    public static void RegisterSyncfusionLicenseKey(IConfiguration configuration)
    {
        var key = configuration["syncfusion"] ?? Environment.GetEnvironmentVariable("SYNCFUSION_LICENSE");
        if (!string.IsNullOrWhiteSpace(key))
            SyncfusionLicenseProvider.RegisterLicense(key);
    }

    public static IHost BuildHostAndConfigureServices(IConfiguration configuration)
        => Host.CreateDefaultBuilder()
            .UseSerilog(LoggingConfigurator.ConfigureWithHostContext, true)
            .ConfigureServices((_, services) =>
            {
                // config
                services.AddSingleton(configuration);

                // infra
                services.AddSingleton<IClipboardService, ClipboardService>();
                services.AddSingleton<IDelayService, DelayService>();
                services.AddSingleton<IDebounceService, DebounceService>();
                services.AddSingleton<IMessageService, MessageService>();
                services.AddTransient<IFileDialogService, FileDialogService>();
                services.AddSingleton<IQrCodeService, QrCodeService>();

                // dialogs
                services.AddTransient<IUserMessageDialogViewModel, UserMessageDialogViewModel>();

                // app services
                services.AddSingleton<IAccountsDAL>(provider =>
                {
                    var logger = provider.GetRequiredService<ILogger<AccountsDAL>>();
                    var config = provider.GetRequiredService<IConfiguration>();
                    // default is:  "%AppData%\\TOTP-Manager\\secrets.dat"
                    var filePathAccounts = config.GetSection(StringsConstants.AccountsStorageFilePath).Value;
                    var resolvedPath = Environment.ExpandEnvironmentVariables(filePathAccounts ?? "");
                    return new AccountsDAL(logger, resolvedPath);
                });

                services.AddSingleton<IErrorHandler, ErrorHandler>();
                services.AddSingleton<IAccountsManager, AccountsManager>();

                var rawProfilePath = configuration.GetSection(StringsConstants.GlobalSettingsProfileStorageFilePath).Value;
                var resolvedProfilePath = Environment.ExpandEnvironmentVariables(rawProfilePath ?? "");
                services.AddSingleton<IGlobalProfileStore>(_ => new FileGlobalProfileStore(resolvedProfilePath));

                // VMs
                services.AddTransient<QrScannerViewModel>();   // transient: new scan each time

                // windows
                services.AddTransient<QrScannerWindow>();

                // dialog / orchestration
                services.AddSingleton<IQrScannerDialogService, QrScannerDialogService>();
                services.AddSingleton<Func<IQrScannerDialogService>>(sp =>
                    () => sp.GetRequiredService<IQrScannerDialogService>());



                services.AddSingleton<IAuthorizationService, AuthorizationService>();
                services.AddSingleton<IUserActivityService, UserActivityService>();
                services.AddSingleton<IInputActivityMonitor, WpfInputActivityMonitor>();

                services.AddSingleton<UnlockViewModel>();
                services.AddSingleton<HelloUnlockViewModel>();
                services.AddSingleton<PasswordUnlockViewModel>();

                services.AddSingleton<IHelloGate, HelloGate>();
                services.AddSingleton<IPasswordService>(_ => new PasswordService(new PasswordRecord([], [], 100_000)));

                // VMs
                services.AddSingleton<IMainViewModel, MainViewModel>();

                // windows
                services.AddSingleton<MainWindow>();
            })
            .Build();

    public static void SetupUnhandledExceptionsHooks(Application app, IHost host)
    {
        var logger = host.Services.GetService<ILogger<App>>();
        var messageService = host.Services.GetService<IMessageService>();

        System.Diagnostics.PresentationTraceSources.DataBindingSource.Switch.Level =
            SourceLevels.Error | SourceLevels.Critical;

        // Dispatcher (UI) thread exceptions
        app.DispatcherUnhandledException += (_, e) =>
        {
            try { messageService?.ShowErrorMessageDialog(UI.msg_DispatcherException); }
            catch { MessageBox.Show(e.Exception.Message, "UI Error"); }

            logger?.LogCritical(e.Exception, "Unhandled UI thread exception");
            e.Handled = true;
        };

        // Non-UI thread exceptions
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            try { messageService?.ShowErrorMessageDialog(UI.ex_FatalError); }
            catch { MessageBox.Show(UI.ex_FatalError, "AppDomain Error"); }

            logger?.LogCritical(e.ExceptionObject as Exception, "Unhandled domain exception");
            Environment.Exit(1);
        };

        // Unobserved task exceptions
        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            try { messageService?.ShowWarningMessage(UI.msg_BackroundTaskException); }
            catch { MessageBox.Show(UI.msg_BackroundTaskException, "Unobserved Task Exception"); }

            logger?.LogCritical(e.Exception, "Unobserved task exception");
            e.SetObserved();
        };
    }
}
