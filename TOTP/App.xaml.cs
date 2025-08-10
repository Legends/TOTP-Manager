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
using TOTP.Interfaces;
using TOTP.Logging;
using TOTP.Services;
using TOTP.ViewModels;
using TOTP.Windows;

namespace TOTP;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App : Application, IDisposable
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

            SetCulture(configuration);

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

    private static void SetCulture(IConfigurationRoot configuration)
    {

        var cultureCode = configuration["Localization:Culture"] ?? "en";
        var culture = new CultureInfo(cultureCode);

        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;

        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;

        LocalizationService.ChangeCulture(cultureCode);
    }


    // can be removed
    private static IConfigurationRoot BuildConfiguration()
    {
        return new ConfigurationBuilder().AddUserSecrets(Assembly.GetExecutingAssembly(), true)
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
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
                services.AddTransient<PlatformSecretDialog>();
                services.AddTransient<IPlatformSecretDialogViewModel, PlatformSecretDialogViewModel>();
                services.AddTransient<IUserMessageDialogViewModel, UserMessageDialogViewModel>();

                services.AddSingleton<ISecretsManager>(provider =>
                {
                    var logger = provider.GetRequiredService<ILogger<SecretsManager>>();
                    var config = provider.GetRequiredService<IConfiguration>();
                    var rawPath = config.GetSection("Secrets:StorageFilePath").Value;
                    var resolvedPath = Environment.ExpandEnvironmentVariables(rawPath ?? "");

                    return new SecretsManager(logger, resolvedPath);
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
        System.Diagnostics.PresentationTraceSources.DataBindingSource.Switch.Level =
            System.Diagnostics.SourceLevels.Error | System.Diagnostics.SourceLevels.Critical;

        System.Diagnostics.Trace.Listeners.Add(new System.Diagnostics.ConsoleTraceListener());
        //// Or implement a TraceListener that writes to Serilog:
        //System.Diagnostics.Trace.Listeners.Add(new SerilogTraceListener.SerilogTraceListener());


        DispatcherUnhandledException += (s, exArgs) =>
        {
            try
            {
                var messageService = _host?.Services.GetService<IMessageService>();
                messageService?.ShowErrorMessageDialog(
                    "An unexpected error occurred.\n\nYou can continue using the application, but some features may not work correctly.");
            }
            catch
            {
                // fallback in case DI isn't ready
                MessageBox.Show(exArgs.Exception.Message, "UI Error");
            }

            _logger?.LogCritical(exArgs.Exception, "Unhandled UI thread exception");

            //Log.Error(exArgs.Exception, "Unhandled UI thread exception2");
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

            _logger?.LogCritical(exArgs.ExceptionObject as Exception, "Unhandled domain exception");
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

            _logger?.LogCritical(exArgs.Exception, "Unobserved task exception");
            exArgs.SetObserved();
        };
    }


    private static void RegisterSyncfusionLicenseKey(IConfigurationRoot configuration)
    {
        // Register Syncfusion license
        var syncfusionLicense = configuration["syncfusion"] ?? Environment.GetEnvironmentVariable("SYNCFUSION_LICENSE");
        if (!string.IsNullOrEmpty(syncfusionLicense))
            SyncfusionLicenseProvider.RegisterLicense(syncfusionLicense);
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        _ = InitializeAsync(); // controlled fire-and-forget

    }

    private async Task InitializeAsync()
    {
        try
        {
            await _host.StartAsync();

            _logger = _host.Services.GetRequiredService<ILogger<App>>();
            _logger.LogInformation("Application startup triggered.");

            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
        catch (Exception ex)
        {
            _logger?.LogCritical(ex, "Startup failed");
            MessageBox.Show("Fehler beim Start: " + ex.Message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(-1);
        }
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
        var secretsManager = _host?.Services.GetService<ISecretsManager>();

        try
        {
            secretsManager?.BackupSecretsFile();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[BackupManager] Failed to create backup for secret.dat: {ex}");

            _logger?.LogError(ex, "Failed to create backup for secret.dat");
            throw;
        }


    }
}