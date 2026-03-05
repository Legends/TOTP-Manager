using OpenCvSharp;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TOTP.Services;
using TOTP.Services.Interfaces;

namespace TOTP.Tests.Services;

public sealed class QrScannerRunnerTests
{
    [Fact]
    public async Task RunAsync_WhenCameraCannotOpen_ThrowsNoCameraFound()
    {
        var capture = new FakeCaptureAdapter
        {
            IsOpenedResult = false,
            OpenResult = false
        };
        var sut = CreateSut(capture, _ => string.Empty);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.RunAsync(CancellationToken.None, _ => { }, () => { }, _ => { }));

        Assert.Equal("No camera found.", ex.Message);
    }

    [Fact]
    public async Task RunAsync_WhenDecodeSucceeds_EmitsDecodedAndStopsWithoutPreview()
    {
        var capture = new FakeCaptureAdapter
        {
            IsOpenedResult = true,
            OpenResult = true,
            ReadAction = frame =>
            {
                frame.Create(1, 1, MatType.CV_8UC3);
                frame.SetTo(new Scalar(0, 0, 0));
                return true;
            }
        };
        var sut = CreateSut(capture, _ => "otpauth://totp/demo");

        string? decoded = null;
        var previewCalls = 0;

        await sut.RunAsync(
            CancellationToken.None,
            _ => previewCalls++,
            () => { },
            text => decoded = text);

        Assert.Equal("otpauth://totp/demo", decoded);
        Assert.Equal(0, previewCalls);
    }

    [Fact]
    public async Task RunAsync_WhenNotDecoded_EmitsPreviewAndFirstFrame_ThenCancels()
    {
        var cts = new CancellationTokenSource();
        var capture = new FakeCaptureAdapter
        {
            IsOpenedResult = true,
            OpenResult = true,
            ReadAction = frame =>
            {
                frame.Create(1, 1, MatType.CV_8UC3);
                frame.SetTo(new Scalar(0, 0, 0));
                return true;
            }
        };
        var sut = CreateSut(capture, _ => string.Empty);

        var firstFrameCalls = 0;
        var previewCalls = 0;

        await Assert.ThrowsAsync<TaskCanceledException>(() =>
            sut.RunAsync(
                cts.Token,
                _ =>
                {
                    previewCalls++;
                    cts.Cancel();
                },
                () => firstFrameCalls++,
                _ => { }));

        Assert.True(previewCalls >= 1);
        Assert.Equal(1, firstFrameCalls);
    }

    [Fact]
    public async Task RunAsync_AlwaysReleasesAndDisposesCapture()
    {
        var capture = new FakeCaptureAdapter
        {
            IsOpenedResult = false,
            OpenResult = false
        };
        var sut = CreateSut(capture, _ => string.Empty);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.RunAsync(CancellationToken.None, _ => { }, () => { }, _ => { }));

        Assert.False(capture.ReleaseCalled);
        Assert.True(capture.DisposeCalled);
    }

    private static QrScannerRunner CreateSut(FakeCaptureAdapter capture, Func<Mat, string> decode)
    {
        return new QrScannerRunner(
            new FakeFactory(capture),
            new FakeDecoder(decode),
            new FakeConverter());
    }

    private sealed class FakeFactory(FakeCaptureAdapter capture) : IVideoCaptureFactory
    {
        public IVideoCaptureAdapter Create() => capture;
    }

    private sealed class FakeCaptureAdapter : IVideoCaptureAdapter
    {
        public bool IsOpenedResult { get; set; }
        public bool OpenResult { get; set; }
        public Func<Mat, bool>? ReadAction { get; set; }
        public bool ReleaseCalled { get; private set; }
        public bool DisposeCalled { get; private set; }

        public bool Open(int deviceIndex, VideoCaptureAPIs api) => OpenResult;
        public bool Open(int deviceIndex) => OpenResult;
        public bool IsOpened() => IsOpenedResult;
        public bool Set(VideoCaptureProperties property, double value) => true;
        public bool Read(Mat frame) => ReadAction?.Invoke(frame) ?? false;
        public void Release() => ReleaseCalled = true;
        public void Dispose() => DisposeCalled = true;
    }

    private sealed class FakeDecoder(Func<Mat, string> decode) : IQrCodeDecoder
    {
        public string Decode(Mat frame) => decode(frame);
    }

    private sealed class FakeConverter : IFrameBitmapSourceConverter
    {
        public BitmapSource Convert(Mat frame)
        {
            var pixels = new byte[] { 0, 0, 0, 255 };
            var bmp = BitmapSource.Create(1, 1, 96, 96, PixelFormats.Bgra32, null, pixels, 4);
            bmp.Freeze();
            return bmp;
        }
    }
}
