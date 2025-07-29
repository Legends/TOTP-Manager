using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;
using Moq.AutoMock;
using TOTP.Interfaces;
using TOTP.Services;

namespace TOTP.Tests.Services;

public class TotpManagerTests
{
    private readonly IFixture _fixture;

    #region Mock.AutoMock

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

    #region ### AutoFixture.AutoMoq

    public TotpManagerTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
    }

    [Fact]
    public void TryComputeCode_ShouldReturnFalse_WhenInvalidBase32()
    {
        // Arrange
        var dialog = _fixture.Freeze<Mock<IPlatformSecretDialogService>>();
        var messageSvc = _fixture.Freeze<Mock<IMessageService>>();
        var secretsHelper = _fixture.Freeze<Mock<ISecretsManager>>();
        var errorHandler = _fixture.Freeze<Mock<IErrorHandler>>();

        var manager = new TotpManager(
            dialog.Object,
            messageSvc.Object,
            secretsHelper.Object,
            errorHandler.Object
        );

        // Act
        var result = manager.TryComputeCode("!!!invalid!!!", out var code, out var error);

        // Assert
        Assert.False(result);
        Assert.Null(code);
        Assert.Contains("Invalid secret format", error);
    }

    #endregion
}