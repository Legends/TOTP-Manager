using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Serilog;
using Serilog.Extensions.Logging;
using TOTP.Core.Interfaces;
using TOTP.Extensions;
using TOTP.Interfaces;
using TOTP.Services;
using TOTP.ViewModels;

namespace TOTP.Tests.Integration;

/// <summary>
/// Integration Tests
///  These wire everything together:
///      real ViewModel
///      real services
///      real configuration
///      but maybe a fake IClipboardService
/// </summary>
public class MainViewModelIntegrationTests : IDisposable
{
    //private readonly AutoMocker _mocker;
    private readonly string _testPath;
    //private readonly ISecretsManager _secretsManager;

    public MainViewModelIntegrationTests()
    {
        // Setup temp test path
        _testPath = Path.Combine(Path.GetTempPath(), $"test-secrets-{Guid.NewGuid()}.dat");
    }


    [Fact]
    public void AddNewSecretCommand_ShouldAddSecret_WhenTotpManagerReturnsSuccess()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);

        var umdVm = new Mock<IUserMessageDialogViewModel>();
        //services.AddSingleton<IUserMessageDialogViewModel>(umdVM.Object);
        services.AddTransient<IUserMessageDialogViewModel>(sp => umdVm.Object);

        // Mock only TotpManager
        var secretItem = new SecretItemViewModel("MyKey", "MySecret");
        var totpManagerMock = new Mock<ITotpManager>();
        totpManagerMock.Setup(m => m.AddNewSecretAsync())
            .ReturnsAsync((true, secretItem.ToDomain()));
        services.AddSingleton(totpManagerMock.Object);

        services.AddSingleton<ISecretsManager>(provider =>
        {
            var logger = new Mock<ILogger<SecretsManager>>();
            // Use a temp or mock path for tests
            var testPath = Path.Combine(Path.GetTempPath(), "test-secrets.dat");

            return new SecretsManager(logger.Object, testPath);
        });

        var pltfDialogMock = new Mock<IPlatformSecretDialogService>();
        pltfDialogMock.Setup(p => p.ShowForm()).Returns((true, "MyKey", "MySecret", null));
        services.AddSingleton(pltfDialogMock.Object);

        // Build provider
        var provider = services.BuildServiceProvider();

        // Resolve dependencies
        var vm = new MainViewModel(
            provider.GetRequiredService<IPlatformSecretDialogService>(),
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
        vm.AddNewSecretCommand.Execute(null);

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
        services.AddSingleton<IPlatformSecretDialogService, PlatformSecretDialogService>();
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

    public void Dispose()
    {
        // Clean up test file and backup files
        if (File.Exists(_testPath))
            File.Delete(_testPath);

        for (int i = 1; i <= 5; i++)
        {
            var backup = _testPath + $".bak{i}";
            if (File.Exists(backup))
                File.Delete(backup);
        }
    }
}