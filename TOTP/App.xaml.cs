using Github2FA.Interfaces;
using Github2FA.Services;
using Github2FA.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Syncfusion.Licensing;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
namespace Github2FA
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IHost _host;
        ILogger<App>? _logger;
        //public static IServiceProvider Services { get; private set; }
        public App()
        {
            SetupUnhandledExceptionsHooks();

            // Build configuration first to get secrets
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true)
                .Build();

            RegisterSyncfusionLicenseKey(configuration);

            // Create the host builder
            _host = Host.CreateDefaultBuilder().UseSerilog((context, services, config) =>
            {
                config
                    .Enrich.FromLogContext()
                    .WriteTo.Console() // 
                    .WriteTo.Debug()
                    .WriteTo.File("Logs/app.log", rollingInterval: RollingInterval.Day);
            })
                .ConfigureServices((context, services) =>
                {
                    // Register configuration so it can be injected
                    services.AddSingleton<IConfiguration>(configuration);
                    services.AddSingleton<IClipboardService, ClipboardService>();
                    services.AddSingleton<IDelayService, DelayService>();

                    // Register services
                    services.AddSingleton<IQrCodeService, QrCodeService>();
                    services.AddSingleton<IDebounceService, DebounceService>();
                    services.AddSingleton<IDialogService, DialogService>();
                    services.AddSingleton<IMessageService, MessageService>();
                    services.AddSingleton<ISecretsManager, SecretsManager>();
                    services.AddSingleton<IErrorHandler, ErrorHandler>();
                    services.AddSingleton<ITotpManager, TotpManager>();

                    // Register ViewModels
                    services.AddSingleton<IMainViewModel, MainViewModel>();

                    // Register MainWindow
                    services.AddSingleton<MainWindow>();

                    //Services = services.BuildServiceProvider();
                })
                .Build();
        }

        private void SetupUnhandledExceptionsHooks()
        {

            this.DispatcherUnhandledException += (s, exArgs) =>
            {
                _logger.LogError(exArgs.Exception, "Unhandled UI thread exception");
                MessageBox.Show("A critical UI error occurred. Check logs.");
                exArgs.Handled = true;
            };

            AppDomain.CurrentDomain.UnhandledException += (s, exArgs) =>
            {
                _logger.LogError(exArgs.ExceptionObject as Exception, "Unhandled domain exception");
                MessageBox.Show("A fatal error occurred. Check logs.");
            };

            TaskScheduler.UnobservedTaskException += (s, exArgs) =>
            {
                _logger.LogError(exArgs.Exception, "Unobserved task exception");
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

            _logger?.LogInformation("Application startup triggered.");

            // Start the host
            await _host.StartAsync();

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

            base.OnExit(e);
        }

    }
}
