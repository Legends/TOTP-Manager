using Github2FA.Helper;
using Github2FA.Interfaces;
using Github2FA.Services;
using Github2FA.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Syncfusion.Licensing;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace Github2FA
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static ServiceProvider ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            var serviceCollection = new ServiceCollection();

            // Build configuration
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets(Assembly.GetExecutingAssembly(), true)
                .Build();

            // Register Syncfusion license
            SyncfusionLicenseProvider.RegisterLicense(configuration["syncfusion"]);

            // Register configuration
            serviceCollection.AddSingleton<IMessageService, MessageService>();
            serviceCollection.AddSingleton<ISecretsHelper, SecretsHelper>();

            serviceCollection.AddSingleton<IConfiguration>(configuration);
            serviceCollection.AddSingleton<IDialogService, DialogService>();
            serviceCollection.AddSingleton<IMainViewModel, MainViewModel>();


            // Register MainWindow
            serviceCollection.AddTransient<MainWindow>();

            ServiceProvider = serviceCollection.BuildServiceProvider();

            // Start MainWindow via DI
            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
    }
}
