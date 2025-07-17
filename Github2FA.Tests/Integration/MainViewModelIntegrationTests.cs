using Github2FA.Interfaces;
using Github2FA.Models;
using Github2FA.Services;
using Github2FA.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Github2FA.Tests.Integration;

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
        // Arrange
        var services = new ServiceCollection();

        services.AddLogging();

        // All real services except TotpManager
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<IMessageService, MessageService>();
        services.AddSingleton<ISecretsManager, SecretsManager>();
        services.AddSingleton<IErrorHandler, ErrorHandler>();
        services.AddSingleton<IClipboardService, ClipboardService>();
        services.AddSingleton<IDebounceService, DebounceService>();
        services.AddSingleton<IDelayService, DelayService>();

        // Mock configuration
        var config = new ConfigurationBuilder().Build();
        services.AddSingleton<IConfiguration>(config);

        // Instead of real TotpManager, register a fake one:
        var secretItem = new SecretItem("MyKey", "MySecret");
        var totpManagerMock = new Mock<ITotpManager>();
        totpManagerMock.Setup(m => m.PromptAndAddTotp())
                       .Returns((true, secretItem));

        services.AddSingleton(totpManagerMock.Object);

        // Build provider
        var provider = services.BuildServiceProvider();

        // Resolve dependencies
        var vm = new MainViewModel(
            provider.GetRequiredService<IQrCodeService>(),
            provider.GetRequiredService<IMessageService>(),
            provider.GetRequiredService<IClipboardService>(),
            provider.GetRequiredService<IConfiguration>(),
            provider.GetRequiredService<ITotpManager>(),
            provider.GetRequiredService<IDebounceService>(),
            provider.GetRequiredService<IDelayService>()
        );

        int initialCount = vm.AllSecrets.Count;

        // Act
        vm.AddNewTotpCommand.Execute(null);

        // Assert
        Assert.Equal(initialCount + 1, vm.AllSecrets.Count);
        Assert.Contains(vm.AllSecrets, s => s.Platform == "MyKey" && s.Secret == "MySecret");
    }
}
