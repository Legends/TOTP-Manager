using Avalonia.Media;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TOTP.Avalonia.Desktop.Platform;
using TOTP.Avalonia.Desktop.Presentation;
using TOTP.Core.Services.Interfaces;

namespace TOTP.Tests.Avalonia.Presentation;

public sealed class CameraScannerViewModelTests
{
    [Fact]
    public async Task StartAsync_WhenValidQrIsDecoded_ShowsOnlySafeAccountMetadataAndClearsFrame()
    {
        var encodedFrame = new byte[] { 1, 2, 3 };
        var runner = new Mock<IQrScannerRunner>();
        runner.Setup(value => value.RunAsync(
                It.IsAny<CancellationToken>(),
                It.IsAny<Action<byte[]>>(),
                It.IsAny<Action>()))
            .Returns<CancellationToken, Action<byte[]>, Action>((_, onPreview, onFirstFrame) =>
            {
                onPreview(encodedFrame);
                onFirstFrame();
                return Task.FromResult(QrScannerRunResult.Decoded(
                    "otpauth://totp/Example:alice?secret=JBSWY3DPEHPK3PXP"));
            });
        var validator = new Mock<IQrPayloadValidator>();
        validator.Setup(value => value.Validate(It.IsAny<string>()))
            .Returns(new QrPayloadValidationResult(true, "Example", "alice"));
        var lifetime = new TrackingDisposable();
        var imageFactory = new Mock<IAvaloniaQrImageFactory>();
        imageFactory.Setup(value => value.Create(It.IsAny<ReadOnlyMemory<byte>>()))
            .Returns(new AvaloniaQrImageHandle(Mock.Of<IImage>(), lifetime));
        using var sut = CreateSut(runner.Object, validator.Object, imageFactory.Object);

        await sut.StartAsync();

        Assert.False(sut.IsScanning);
        Assert.True(sut.HasPreview);
        Assert.Contains("Example / alice", sut.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("JBSWY", sut.Message, StringComparison.Ordinal);
        Assert.All(encodedFrame, value => Assert.Equal(0, value));
        validator.Verify(value => value.Validate(It.Is<string>(payload => payload.Contains("secret="))), Times.Once);
    }

    [Theory]
    [InlineData(QrScannerFailureKind.PermissionDenied, "permission")]
    [InlineData(QrScannerFailureKind.NativeRuntimeUnavailable, "runtime")]
    [InlineData(QrScannerFailureKind.DeviceLost, "disconnected")]
    public async Task StartAsync_WhenRunnerReturnsFailure_ProjectsTypedSafeMessage(
        QrScannerFailureKind failure,
        string expected)
    {
        var runner = new Mock<IQrScannerRunner>();
        runner.Setup(value => value.RunAsync(
                It.IsAny<CancellationToken>(),
                It.IsAny<Action<byte[]>>(),
                It.IsAny<Action>()))
            .ReturnsAsync(QrScannerRunResult.Failed(failure));
        using var sut = CreateSut(runner.Object);

        await sut.StartAsync();

        Assert.Contains(expected, sut.Message, StringComparison.OrdinalIgnoreCase);
        Assert.False(sut.IsScanning);
    }

    [Fact]
    public async Task Clear_WhileScanning_CancelsRunnerAndClearsPreview()
    {
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var runner = new Mock<IQrScannerRunner>();
        runner.Setup(value => value.RunAsync(
                It.IsAny<CancellationToken>(),
                It.IsAny<Action<byte[]>>(),
                It.IsAny<Action>()))
            .Returns<CancellationToken, Action<byte[]>, Action>(async (token, _, _) =>
            {
                started.SetResult();
                await Task.Delay(Timeout.InfiniteTimeSpan, token);
                return QrScannerRunResult.Failed(QrScannerFailureKind.Unexpected);
            });
        using var sut = CreateSut(runner.Object);

        var scan = sut.StartAsync();
        await started.Task;
        sut.Clear();
        await scan;

        Assert.False(sut.IsScanning);
        Assert.False(sut.HasPreview);
        Assert.Contains("starts only", sut.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static CameraScannerViewModel CreateSut(
        IQrScannerRunner runner,
        IQrPayloadValidator? validator = null,
        IAvaloniaQrImageFactory? imageFactory = null) =>
        new(
            runner,
            validator ?? Mock.Of<IQrPayloadValidator>(),
            imageFactory ?? Mock.Of<IAvaloniaQrImageFactory>(),
            new ImmediateUiScheduler(),
            NullLogger<CameraScannerViewModel>.Instance);

    private sealed class ImmediateUiScheduler : IUiScheduler
    {
        public bool CheckAccess() => true;
        public void Post(Action action) => action();
        public Task InvokeAsync(Action action)
        {
            action();
            return Task.CompletedTask;
        }

        public async Task InvokeAsync(Func<Task> action) => await action();
    }

    private sealed class TrackingDisposable : IDisposable
    {
        public bool IsDisposed { get; private set; }
        public void Dispose() => IsDisposed = true;
    }
}
