using Moq;
using TOTP.Core.Security.Interfaces;
using TOTP.Core.Security.Models;
using TOTP.Avalonia.Desktop.Presentation;

namespace TOTP.Tests.Avalonia.Presentation;

public sealed class PasswordUnlockViewModelTests
{
    [Fact]
    public async Task UnlockAsync_WithSyntheticCredential_ClearsInputBeforeAwaitAndSignalsSuccess()
    {
        var completion = new TaskCompletionSource<AuthorizationResult>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var authorization = new Mock<IAuthorizationService>();
        authorization.Setup(service => service.TryUnlockWithPasswordAsync("synthetic password"))
            .Returns(completion.Task);
        var sut = new PasswordUnlockViewModel(authorization.Object)
        {
            Password = "synthetic password"
        };
        var unlocked = false;
        sut.Unlocked += (_, _) => unlocked = true;

        var pending = sut.UnlockAsync();

        Assert.Empty(sut.Password);
        Assert.True(sut.IsBusy);

        completion.SetResult(AuthorizationResult.Success);
        await pending;

        Assert.True(unlocked);
        Assert.False(sut.IsBusy);
        Assert.False(sut.HasMessage);
    }

    [Theory]
    [InlineData(AuthorizationResult.InvalidCredentials, "could not unlock")]
    [InlineData(AuthorizationResult.TooManyAttempts, "Too many attempts")]
    public async Task UnlockAsync_WhenAuthorizationRefuses_ShowsGenericFailure(
        AuthorizationResult result,
        string expectedMessage)
    {
        var authorization = new Mock<IAuthorizationService>();
        authorization.Setup(service => service.TryUnlockWithPasswordAsync(It.IsAny<string>()))
            .ReturnsAsync(result);
        var sut = new PasswordUnlockViewModel(authorization.Object) { Password = "wrong" };

        await sut.UnlockAsync();

        Assert.Empty(sut.Password);
        Assert.Contains(expectedMessage, sut.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UnlockAsync_WhenBoundaryThrows_DoesNotExposeExceptionText()
    {
        var authorization = new Mock<IAuthorizationService>();
        authorization.Setup(service => service.TryUnlockWithPasswordAsync(It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("sensitive synthetic detail"));
        var sut = new PasswordUnlockViewModel(authorization.Object) { Password = "synthetic" };

        await sut.UnlockAsync();

        Assert.True(sut.HasMessage);
        Assert.DoesNotContain("sensitive", sut.Message, StringComparison.OrdinalIgnoreCase);
    }
}
