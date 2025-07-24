using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Serilog;
using Serilog.Extensions.Logging;
using TOTP.Interfaces;
using TOTP.Models;
using TOTP.Services;
using TOTP.ViewModels;

namespace TOTP.Tests.Integration;

/// <summary>
/// Integration Tests
//  These wire everything together:
//      real ViewModel
//      real services
//      real configuration
//      but maybe a fake IClipboardService
/// </summary>
public class MainViewModelIntegrationTests
{
    [Fact]
    public void AddNewTotpCommand_ShouldAddSecret_WhenTotpManagerReturnsSuccess()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);

        // Mock only TotpManager
        var secretItem = new SecretItem("MyKey", "MySecret");
        var totpManagerMock = new Mock<ITotpManager>();
        totpManagerMock.Setup(m => m.AddNewTotp())
            .Returns((true, secretItem));
        services.AddSingleton(totpManagerMock.Object);


        // Build provider
        var provider = services.BuildServiceProvider();

        // Resolve dependencies
        var vm = new MainViewModel(
            provider.GetRequiredService<ILogger<MainViewModel>>(),
            provider.GetRequiredService<IQrCodeService>(),
            provider.GetRequiredService<IMessageService>(),
            provider.GetRequiredService<IClipboardService>(),
            provider.GetRequiredService<IConfiguration>(),
            provider.GetRequiredService<ITotpManager>(),
            provider.GetRequiredService<IDebounceService>(),
            provider.GetRequiredService<IDelayService>(),
            provider.GetRequiredService<ISecretsManager>()
        );

        var initialCount = vm.AllSecrets.Count;

        // Act
        vm.AddNewTotpCommand.Execute(null);

        // Assert
        Assert.Equal(initialCount + 1, vm.AllSecrets.Count);
        Assert.Contains(vm.AllSecrets, s => s.Platform == "MyKey" && s.Secret == "MySecret");
    }

    private static void ConfigureServices(ServiceCollection services)
    {
        // 🔧 Use Serilog for logging
        var logger = new LoggerConfiguration()
            .WriteTo.File("Logs/test.log", rollingInterval: RollingInterval.Day)
            .MinimumLevel.Debug()
            .CreateLogger();

        services.AddSingleton<ILoggerFactory>(new SerilogLoggerFactory(logger));
        services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));

        // Real services
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<IMessageService, MessageService>();
        services.AddSingleton<ISecretsManager, SecretsManager>();
        services.AddSingleton<IErrorHandler, ErrorHandler>();
        services.AddSingleton<IClipboardService, ClipboardService>();
        services.AddSingleton<IDebounceService, DebounceService>();
        services.AddSingleton<IDelayService, DelayService>();
        services.AddSingleton<IQrCodeService, QrCodeService>();

        // Configuration
        var config = new ConfigurationBuilder().Build();
        services.AddSingleton<IConfiguration>(config);
    }
}