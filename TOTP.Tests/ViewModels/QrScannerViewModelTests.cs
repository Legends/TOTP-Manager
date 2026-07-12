using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TOTP.Core.Interfaces;
using TOTP.Services.Interfaces;
using TOTP.ViewModels;

namespace TOTP.Tests.ViewModels;

public sealed class QrScannerViewModelTests
{
    [Fact]
    public async Task Start_WhenRunnerProvidesFrameAndDecode_UpdatesStateAndRequestsSuccessClose()
    {
        var closeEvents = new List<bool>();
        var runner = new FakeRunner(async (token, onPreview, onFirstFrame, onDecoded) =>
        {
            onPreview(CreateBitmap());
            onFirstFrame();
            onDecoded("otpauth://totp/test");
            await Task.CompletedTask;
        });

        var vm = new QrScannerViewModel(runner, NullLogger<QrScannerViewModel>.Instance);
        vm.CloseRequested += (_, e) => closeEvents.Add(e.DialogResult);

        vm.Start();

        await WaitUntilAsync(() => closeEvents.Count == 1);

        Assert.False(vm.IsInitializing);
        Assert.NotNull(vm.PreviewImage);
        Assert.Equal("otpauth://totp/test", vm.DecodedText);
        Assert.Single(closeEvents);
        Assert.True(closeEvents[0]);
    }

    [Fact]
    public async Task Start_WhenRunnerThrows_ShowsRecoverableErrorWithoutClosing()
    {
        var closeEvents = new List<bool>();
        var runner = new FakeRunner((token, onPreview, onFirstFrame, onDecoded) =>
            Task.FromException(new InvalidOperationException("No camera found.")));

        var vm = new QrScannerViewModel(runner, NullLogger<QrScannerViewModel>.Instance);
        vm.CloseRequested += (_, e) => closeEvents.Add(e.DialogResult);

        vm.Start();

        await WaitUntilAsync(() => vm.ErrorMessage is not null);

        Assert.Equal(
            "The camera is unavailable. Turn it on using your keyboard or privacy switch. The scanner will reconnect automatically.",
            vm.ErrorMessage);
        Assert.False(vm.IsInitializing);
        Assert.Empty(closeEvents);
        Assert.True(vm.CancelCommand.CanExecute(null));
        vm.Dispose();
    }

    [Fact]
    public async Task Start_WhenCameraBecomesAvailable_RetriesAndResumesScanningAutomatically()
    {
        var attempts = 0;
        var runner = new FakeRunner((token, onPreview, onFirstFrame, onDecoded) =>
        {
            if (Interlocked.Increment(ref attempts) == 1)
            {
                throw new InvalidOperationException("No camera found.");
            }

            onPreview(CreateBitmap());
            onFirstFrame();
            onDecoded("otpauth://totp/reconnected");
            return Task.CompletedTask;
        });
        var vm = new QrScannerViewModel(runner, NullLogger<QrScannerViewModel>.Instance);

        vm.Start();
        await WaitUntilAsync(() => vm.DecodedText is not null, timeoutMs: 2000);

        Assert.Equal(2, attempts);
        Assert.Equal("otpauth://totp/reconnected", vm.DecodedText);
        Assert.False(vm.HasError);
    }

    [Fact]
    public async Task CancelCommand_CancelsRunnerAndRequestsFailureClose()
    {
        var closeEvents = new List<bool>();
        var wasCanceled = false;

        var runner = new FakeRunner(async (token, onPreview, onFirstFrame, onDecoded) =>
        {
            try
            {
                await Task.Delay(Timeout.Infinite, token);
            }
            catch (OperationCanceledException)
            {
                wasCanceled = true;
                throw;
            }
        });

        var vm = new QrScannerViewModel(runner, NullLogger<QrScannerViewModel>.Instance);
        vm.CloseRequested += (_, e) => closeEvents.Add(e.DialogResult);

        vm.Start();
        vm.CancelCommand.Execute(null);

        await WaitUntilAsync(() => closeEvents.Count >= 1 && wasCanceled);

        Assert.Contains(false, closeEvents);
        Assert.True(wasCanceled);
    }

    [Fact]
    public async Task Start_CalledTwice_StartsRunnerOnlyOnce()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var invocations = 0;
        var runner = new FakeRunner(async (token, onPreview, onFirstFrame, onDecoded) =>
        {
            Interlocked.Increment(ref invocations);
            await Task.Delay(80, token);
        });

        var vm = new QrScannerViewModel(runner, NullLogger<QrScannerViewModel>.Instance);

        vm.Start();
        vm.Start();

        await Task.Delay(120, cancellationToken);

        Assert.Equal(1, invocations);
    }

    [Fact]
    public async Task Start_WhenRunnerCallbacksArriveFromWorkerThread_MarshalsUiStateChanges()
    {
        var dispatcher = new Mock<IDispatcherService>();
        dispatcher.Setup(d => d.CheckAccess()).Returns(false);
        dispatcher.Setup(d => d.InvokeOnUI(It.IsAny<Action>()))
            .Callback<Action>(action => action());
        var runner = new FakeRunner((token, onPreview, onFirstFrame, onDecoded) => Task.Run(() =>
        {
            onPreview(CreateBitmap());
            onFirstFrame();
            onDecoded("otpauth://totp/worker-thread");
        }, token));
        var vm = new QrScannerViewModel(
            runner,
            NullLogger<QrScannerViewModel>.Instance,
            dispatcher.Object);

        vm.Start();
        await WaitUntilAsync(() => vm.DecodedText is not null);

        Assert.Equal("otpauth://totp/worker-thread", vm.DecodedText);
        dispatcher.Verify(d => d.InvokeOnUI(It.IsAny<Action>()), Times.AtLeast(3));
    }

    private static BitmapSource CreateBitmap()
    {
        var pixels = new byte[] { 0, 0, 0, 255 };
        var bmp = BitmapSource.Create(1, 1, 96, 96, PixelFormats.Bgra32, null, pixels, 4);
        bmp.Freeze();
        return bmp;
    }

    private static async Task WaitUntilAsync(Func<bool> condition, int timeoutMs = 1200)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var start = Environment.TickCount64;
        while (!condition())
        {
            if (Environment.TickCount64 - start > timeoutMs)
            {
                throw new TimeoutException("Condition was not met in time.");
            }

            await Task.Delay(20, cancellationToken);
        }
    }

    private sealed class FakeRunner : IQrScannerRunner
    {
        private readonly Func<CancellationToken, Action<BitmapSource>, Action, Action<string>, Task> _run;

        public FakeRunner(Func<CancellationToken, Action<BitmapSource>, Action, Action<string>, Task> run)
        {
            _run = run;
        }

        public Task RunAsync(CancellationToken token, Action<BitmapSource> onPreview, Action onFirstFrame, Action<string> onDecoded)
            => _run(token, onPreview, onFirstFrame, onDecoded);
    }
}
