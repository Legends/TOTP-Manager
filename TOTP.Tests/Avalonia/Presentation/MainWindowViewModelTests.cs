using Moq;
using TOTP.Avalonia.Desktop.Presentation;
using TOTP.Avalonia.Desktop.Startup;

namespace TOTP.Tests.Avalonia.Presentation;

public sealed class MainWindowViewModelTests
{
    [Theory]
    [InlineData(AvaloniaStartupOutcome.ReadyForPasswordSetup, false, "Create a master password")]
    [InlineData(AvaloniaStartupOutcome.ReadyForUnlock, false, "Enter your master password")]
    [InlineData(AvaloniaStartupOutcome.PreferencesUnavailable, true, "preferences could not be loaded")]
    [InlineData(AvaloniaStartupOutcome.UnexpectedFailure, true, "could not start safely")]
    public async Task InitializeAsync_ProjectsSafeRecoverableState(
        AvaloniaStartupOutcome outcome,
        bool canRetry,
        string expectedText)
    {
        var coordinator = new Mock<IAvaloniaStartupCoordinator>();
        coordinator.Setup(value => value.InitializeAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(outcome);
        using var sut = new MainWindowViewModel(coordinator.Object);

        await sut.InitializeAsync();

        Assert.False(sut.IsBusy);
        Assert.Equal(canRetry, sut.CanRetry);
        Assert.Contains(expectedText, sut.StatusText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task InitializeAsync_WhenCoordinatorContractThrows_RemainsRecoverable()
    {
        var coordinator = new Mock<IAvaloniaStartupCoordinator>();
        coordinator.Setup(value => value.InitializeAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("sensitive synthetic detail"));
        using var sut = new MainWindowViewModel(coordinator.Object);

        await sut.InitializeAsync();

        Assert.True(sut.CanRetry);
        Assert.DoesNotContain("sensitive", sut.StatusText, StringComparison.OrdinalIgnoreCase);
    }
}
