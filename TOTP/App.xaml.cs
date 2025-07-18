using Github2FA.Interfaces;
using Github2FA.Services;
using Github2FA.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Syncfusion.Licensing;
using System;
using System.Reflection;
using System.Windows;

namespace Github2FA
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IHost _host;
        //public static IServiceProvider Services { get; private set; }
        public App()
        {
            // Build configuration first to get secrets
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true)
                .Build();

            RegisterSyncfusionLicenseKey(configuration);

            // Create the host builder
            _host = Host.CreateDefaultBuilder()
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

            // Global unhandled exception handler
            AppDomain.CurrentDomain.UnhandledException += (s, ex) =>
            {
                //var msgSvc = Services.GetRequiredService<IMessageService>();
                var msgSvc = _host.Services.GetRequiredService<IMessageService>();
                msgSvc.ShowMessage($"A fatal error occurred:\n{ex.ExceptionObject}", "Fatal Error");

            };

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

            base.OnExit(e);
        }
    }
}
