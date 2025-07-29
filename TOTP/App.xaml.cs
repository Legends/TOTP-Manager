using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Syncfusion.Licensing;
using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using TOTP.Interfaces;
using TOTP.Logging;
using TOTP.Services;
using TOTP.ViewModels;
using TOTP.Windows;

namespace TOTP;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private readonly IHost _host = null!;

    private ILogger<App>? _logger;
    //public static IServiceProvider Services { get; private set; }

    public App()
    {
        LoggingConfigurator.SetupEarlyLogger();

        try
        {
            SetupUnhandledExceptionsHooks();

            // Build configuration first to get secrets
            var configuration = BuildConfiguration();

            RegisterSyncfusionLicenseKey(configuration);

            // Create the host builder
            _host = CreateHostAndConfigureServices(configuration);

            Log.Information($"{this.GetType().AssemblyQualifiedName} successfully started!");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred during application startup.");
        }
    }


    // can be removed
    private static IConfigurationRoot BuildConfiguration()
    {
        return new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddUserSecrets(Assembly.GetExecutingAssembly(), true)
            .Build();
    }


    private static IHost CreateHostAndConfigureServices(IConfigurationRoot configuration)
    {
        return Host.CreateDefaultBuilder().UseSerilog(LoggingConfigurator.ConfigureWithHostContext, true)
            .ConfigureServices((context, services) =>
            {
                // Register configuration so it can be injected
                services.AddSingleton<IConfiguration>(configuration);
                services.AddSingleton<IClipboardService, ClipboardService>();
                services.AddSingleton<IDelayService, DelayService>();

                // Register services
                services.AddSingleton<IQrCodeService, QrCodeService>();
                services.AddSingleton<IDebounceService, DebounceService>();
                services.AddSingleton<IPlatformSecretDialogService, PlatformSecretDialogService>();
                services.AddSingleton<IMessageService, MessageService>();
                services.AddTransient<KeyValueDialog>();
                services.AddTransient<IKeyValueDialogViewModel, KeyValueDialogViewModel>();
                services.AddTransient<IUserMessageDialogViewModel, UserMessageDialogViewModel>();

                services.AddSingleton<ISecretsManager>(provider =>
                {
                    var messageService = provider.GetRequiredService<IMessageService>();
                    var config = provider.GetRequiredService<IConfiguration>();

                    var rawPath = config.GetSection("Secrets:StorageFilePath").Value;
                    var resolvedPath = Environment.ExpandEnvironmentVariables(rawPath ?? "");

                    return new SecretsManager(messageService, resolvedPath);
                });

                services.AddSingleton<IErrorHandler, ErrorHandler>();
                services.AddSingleton<ITotpManager, TotpManager>();

                // Register ViewModels
                services.AddSingleton<IMainViewModel, MainViewModel>();

                // Register MainWindow
                services.AddSingleton<MainWindow>();

                //Services = services.BuildServiceProvider();
            })
            .Build();


        //var messageService = new MessageService(); // Or a mock if not needed
        //var targetManager = new SecretsManager(messageService);

        //SecretsMigration.MigrateFromUserSecrets("6f888768-43da-4da8-9820-96b854382d72", targetManager);
    }


    private void SetupUnhandledExceptionsHooks()
    {
        DispatcherUnhandledException += (s, exArgs) =>
        {
            try
            {
                var messageService = _host?.Services.GetService<IMessageService>();
                messageService?.ShowErrorMessageDialog(
                    "An unexpected error occurred in the UI.\n\nYou can continue using the application, but some features may not work correctly.");

            }
            catch
            {
                // fallback in case DI isn't ready
                MessageBox.Show(exArgs.Exception.Message, "UI Error");
            }

            _logger?.LogError(exArgs.Exception, "Unhandled UI thread exception");
            exArgs.Handled = true;
            //messageService ?.ShowWarningMessage();
        };

        AppDomain.CurrentDomain.UnhandledException += (s, exArgs) =>
        {
            try
            {
                var messageService = _host?.Services.GetService<IMessageService>();
                messageService?.ShowErrorMessageDialog(
                    "A fatal application error occurred. The app must now close.\n\nSee log files for details.");

            }
            catch
            {
                MessageBox.Show("A fatal error occurred. The app will shut down!", "AppDomain Error");
            }

            _logger?.LogError(exArgs.ExceptionObject as Exception, "Unhandled domain exception");
            Environment.Exit(1);
        };

        TaskScheduler.UnobservedTaskException += (s, exArgs) =>
        {
            try
            {
                var messageService = _host?.Services.GetService<IMessageService>();
                messageService?.ShowWarningMessage(
                    "A background task failed. You can continue using the application.");

            }
            catch
            {
                MessageBox.Show("A background task failed.", "Task Error");
            }

            _logger?.LogError(exArgs.Exception, "Unobserved task exception");
            exArgs.SetObserved();
        };
    }


    private static void RegisterSyncfusionLicenseKey(IConfigurationRoot configuration)
    {
        // Register Syncfusion license
        var syncfusionLicense = configuration["syncfusion"];
        if (!string.IsNullOrEmpty(syncfusionLicense))
            SyncfusionLicenseProvider.RegisterLicense(syncfusionLicense);
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Start the host
        await _host.StartAsync();

        // 🔥 Resolve logger now that DI container is available
        _logger = _host.Services.GetRequiredService<ILogger<App>>();
        _logger.LogInformation("Application startup triggered.");

        // Show the main window via DI
        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        // Gracefully shut down the host
        if (_host != null)
            await _host.StopAsync();

        _host?.Dispose();

        await Log.CloseAndFlushAsync();

        base.OnExit(e);
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}