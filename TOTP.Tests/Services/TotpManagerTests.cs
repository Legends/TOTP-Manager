using AutoFixture;
using AutoFixture.AutoMoq;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.AutoMock;
using TOTP.Core.Common;
using TOTP.Core.Enums;
using TOTP.Core.Events;
using TOTP.Core.Models;
using TOTP.Interfaces;
using TOTP.Services;

namespace TOTP.Tests.Services;

// TODO:
// add test cases for:
// - duplicates
// 
public class TotpManagerTests
{
    private readonly IFixture _fixture;

    #region ### AutoFixture.AutoMoq

    public TotpManagerTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
    }

    [Fact]
    public void TryComputeCode_ShouldReturnFalse_WhenInvalidBase32()
    {
        // Arrange
        var secretsManager = _fixture.Freeze<Mock<ISecretsManager>>();
        var logger = _fixture.Freeze<Mock<ILogger<TotpManager>>>();

        var manager = new TotpManager(
            secretsManager.Object,
            logger.Object
        );

        // Act
        var result = manager.TryComputeCode("!!!invalid!!!", out var code, out var ex);

        // Assert
        Assert.False(result);
        Assert.Null(code);
        Assert.Contains("invalid Base32", ex?.Message);
    }

    #endregion

    #region Mock.AutoMock

    [Fact]
    public async Task UpdateSecretItemShouldReturnAlreadyExists()
    {

        var mockSecretsManager = new Mock<ISecretsManager>();

        var mockLogger = new Mock<ILogger<TotpManager>>();
        var totpManager = new TotpManager(mockSecretsManager.Object, mockLogger.Object);

        var id = Guid.NewGuid();
        var PreviousVersion = new SecretItem(id, "A", "dfgdsafdsf");

        var domainSecretsList = new List<SecretItem>
        {
            PreviousVersion,
            new(Guid.NewGuid(), "B", "sdfgsdfgsdfg"),
            new(Guid.NewGuid(), "C", "xcvxcvxcvxcv")
        };

        var updated = new SecretItem(id, "A", "JBSWY3DPEHPK3PXP");

        mockSecretsManager
            .Setup(m => m.UpdateItemAsync("A", It.IsAny<SecretItem>()))
            .ReturnsAsync(Result<bool>.Fail(OperationStatus.AlreadyExists));

        var success = await totpManager.UpdateSecretAsync(PreviousVersion, updated, domainSecretsList);
        Assert.False(success);
    }

    [Fact]
    public async Task AddNewDuplicateSecretAsync_ShouldReturnAlreadyExists()
    {

        var mockSecretsManager = new Mock<ISecretsManager>();
        var mockLogger = new Mock<ILogger<TotpManager>>();
        var totpManager = new TotpManager(mockSecretsManager.Object, mockLogger.Object);

        int promptCount = 0;

        // Simulate user prompt: first returns duplicate, then valid
        totpManager.OnAddNewPrompt += (_) =>
        {
            promptCount++;
            return promptCount == 1
                ? new AddNewPromptArgs { Success = true, Platform = "DUPLICATE", Secret = "valid" }
                : new AddNewPromptArgs { Success = true, Platform = "DUPLICATE", Secret = "valid" };
        };

        // Simulate AddNewItemAsync behavior
        mockSecretsManager
            .SetupSequence(m => m.AddNewItemAsync(It.IsAny<SecretItem>()))
            .ReturnsAsync(Result<bool>.Fail(OperationStatus.AlreadyExists))
            .ReturnsAsync(Result<bool>.Success(true));// ensure exit from loop

        var receivedStatuses = new List<OperationStatus>();
        totpManager.OnMessageSend += (_, status, _) =>
        {
            receivedStatuses.Add(status);
        };

        // Act
        var (success, item) = await totpManager.AddNewSecretAsync();

        // Assert
        Assert.True(success);
        Assert.NotNull(item);
        Assert.Equal("DUPLICATE", item.Platform);
        Assert.Equal("valid", item.Secret);
        Assert.Contains(OperationStatus.AlreadyExists, receivedStatuses);
    }


    [Fact]
    public void TryComputeCode_ShouldReturnFalse_ForInvalidBase32()
    {
        // Manual setup of dependencies is not needed when using AutoMocker
        //var totpManager = new TotpManager(
        //    Mock.Of<IPlatformSecretDialogService>(),
        //    Mock.Of<IMessageService>(),
        //    Mock.Of<ISecretsManager>(),
        //    Mock.Of<IErrorHandler>()
        //);

        // Arrange
        var mocker = new AutoMocker();

        // Auto-resolve all dependencies
        var totpManager = mocker.CreateInstance<TotpManager>();

        var secret = "!!!INVALID!!!";

        // Act
        var result = totpManager.TryComputeCode(secret, out var code, out var error);

        // Assert
        Assert.False(result);
        Assert.Null(code);
        Assert.NotNull(error);
    }

    #endregion


}