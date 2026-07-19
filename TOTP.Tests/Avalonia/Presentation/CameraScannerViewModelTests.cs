using Avalonia.Media;
using FluentResults;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TOTP.Avalonia.Desktop.Platform;
using TOTP.Avalonia.Desktop.Presentation;
using TOTP.Core.Services.Interfaces;
using TOTP.Core.Services.Models;
using TOTP.Avalonia.Desktop.Localization;

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
        var import = new Mock<IQrAccountImportService>();
        var importedAccountId = Guid.NewGuid();
        import.Setup(value => value.ImportAsync(
                It.IsAny<string>(),
                It.IsAny<Func<QrAccountConflict, CancellationToken, Task<QrAccountConflictDecision>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(new QrAccountImportOutcome(
                QrAccountImportStatus.Added,
                importedAccountId,
                "Example",
                "alice")));
        var lifetime = new TrackingDisposable();
        var imageFactory = new Mock<IAvaloniaQrImageFactory>();
        imageFactory.Setup(value => value.Create(It.IsAny<ReadOnlyMemory<byte>>()))
            .Returns(new AvaloniaQrImageHandle(Mock.Of<IImage>(), lifetime));
        using var sut = CreateSut(runner.Object, validator.Object, imageFactory.Object, import.Object);
        var closeRequested = false;
        AccountImportedEventArgs? importedEvent = null;
        sut.CloseRequested += (_, _) => closeRequested = true;
        sut.AccountImported += (_, args) => importedEvent = args;

        await sut.StartAsync();

        Assert.False(sut.IsScanning);
        Assert.True(sut.HasPreview);
        Assert.Equal(AvaloniaStringKeys.QrAccountAdded, sut.Message);
        Assert.DoesNotContain("JBSWY", sut.Message, StringComparison.Ordinal);
        Assert.All(encodedFrame, value => Assert.Equal(0, value));
        Assert.True(closeRequested);
        Assert.Equal(importedAccountId, importedEvent!.AccountId);
        Assert.Equal(AvaloniaStringKeys.QrAccountAdded, importedEvent.Message);
        validator.Verify(value => value.Validate(It.Is<string>(payload => payload.Contains("secret="))), Times.Once);
    }

    [Fact]
    public async Task CancelAsync_WhenScannerIsIdle_RequestsDialogClose()
    {
        using var sut = CreateSut(Mock.Of<IQrScannerRunner>());
        var closeRequested = false;
        sut.CloseRequested += (_, _) => closeRequested = true;

        await sut.CancelAsync();

        Assert.True(closeRequested);
        Assert.True(sut.CancelCommand.CanExecute(null));
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
        IAvaloniaQrImageFactory? imageFactory = null,
        IQrAccountImportService? importService = null) =>
        new(
            runner,
            validator ?? Mock.Of<IQrPayloadValidator>(),
            imageFactory ?? Mock.Of<IAvaloniaQrImageFactory>(),
            new ImmediateUiScheduler(),
            NullLogger<CameraScannerViewModel>.Instance,
            importService ?? Mock.Of<IQrAccountImportService>(),
            Mock.Of<IAvaloniaDialogService>(),
            Localization());

    private static IAvaloniaLocalizationService Localization()
    {
        var localization = new Mock<IAvaloniaLocalizationService>();
        localization.Setup(value => value.GetString(It.IsAny<string>()))
            .Returns((string key) => key);
        return localization.Object;
    }

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
