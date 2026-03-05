using OpenCvSharp;
using TOTP.Services;
using TOTP.Services.Interfaces;

namespace TOTP.Tests.Services;

public sealed class QrCameraServiceTests
{
    [Fact]
    public async Task InitializeAsync_WhenDshowOpenSucceeds_InitializesAndSetsProperties()
    {
        var adapter = new FakeCaptureAdapter
        {
            OpenWithApiResult = true,
            OpenResult = false,
            IsOpenedResult = true,
            ReadResult = true
        };
        var sut = new QrCameraService(new FakeFactory(adapter));

        await sut.InitializeAsync(deviceIndex: 2, width: 640, height: 480, fps: 25);

        Assert.Equal(2, sut.DeviceIndex);
        Assert.Equal(640, sut.Width);
        Assert.Equal(480, sut.Height);
        Assert.Equal(25, sut.Fps);
        Assert.True(sut.IsOpen);
        Assert.True(adapter.OpenWithApiCalled);
        Assert.False(adapter.OpenCalledWithoutApi);
        Assert.True(adapter.SetCalls.Count >= 4);
    }

    [Fact]
    public async Task InitializeAsync_WhenDshowFailsAndFallbackSucceeds_UsesFallback()
    {
        var adapter = new FakeCaptureAdapter
        {
            OpenWithApiResult = false,
            OpenResult = true,
            IsOpenedResult = true,
            ReadResult = true
        };
        var sut = new QrCameraService(new FakeFactory(adapter));

        await sut.InitializeAsync();

        Assert.True(adapter.OpenWithApiCalled);
        Assert.True(adapter.OpenCalledWithoutApi);
        Assert.True(sut.IsOpen);
    }

    [Fact]
    public async Task InitializeAsync_WhenBothOpenAttemptsFail_Throws()
    {
        var adapter = new FakeCaptureAdapter
        {
            OpenWithApiResult = false,
            OpenResult = false,
            IsOpenedResult = false
        };
        var sut = new QrCameraService(new FakeFactory(adapter));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => sut.InitializeAsync());

        Assert.Equal("Unable to open camera.", ex.Message);
    }

    [Fact]
    public async Task InitializeAsync_WhenIsOpenedFalseAfterOpen_Throws()
    {
        var adapter = new FakeCaptureAdapter
        {
            OpenWithApiResult = true,
            OpenResult = false,
            IsOpenedResult = false
        };
        var sut = new QrCameraService(new FakeFactory(adapter));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => sut.InitializeAsync());

        Assert.Equal("Camera failed to open.", ex.Message);
    }

    [Fact]
    public async Task InitializeAsync_WhenAlreadyInitialized_DoesNotRecreateCapture()
    {
        var adapter = new FakeCaptureAdapter
        {
            OpenWithApiResult = true,
            IsOpenedResult = true,
            ReadResult = true
        };
        var factory = new FakeFactory(adapter);
        var sut = new QrCameraService(factory);

        await sut.InitializeAsync();
        await sut.InitializeAsync();

        Assert.Equal(1, factory.CreateCalls);
    }

    [Fact]
    public async Task Dispose_ReleasesAndResetsState()
    {
        var adapter = new FakeCaptureAdapter
        {
            OpenWithApiResult = true,
            IsOpenedResult = true,
            ReadResult = true
        };
        var sut = new QrCameraService(new FakeFactory(adapter));

        await sut.InitializeAsync();
        sut.Dispose();

        Assert.True(adapter.ReleaseCalled);
        Assert.True(adapter.DisposeCalled);
        Assert.False(sut.IsOpen);
    }

    [StaFact]
    public async Task StartPreviewLoopAsync_WhenNotInitialized_Throws()
    {
        var sut = new QrCameraService(new FakeFactory(new FakeCaptureAdapter()));
        var image = new System.Windows.Controls.Image();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.StartPreviewLoopAsync(image, CancellationToken.None));
    }

    [StaFact]
    public async Task StartPreviewAsync_WhenNotInitialized_Throws()
    {
        var sut = new QrCameraService(new FakeFactory(new FakeCaptureAdapter()));
        var image = new System.Windows.Controls.Image();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.StartPreviewAsync(image, CancellationToken.None));
    }

    private sealed class FakeFactory(FakeCaptureAdapter adapter) : IVideoCaptureFactory
    {
        public int CreateCalls { get; private set; }
        public IVideoCaptureAdapter Create()
        {
            CreateCalls++;
            return adapter;
        }
    }

    private sealed class FakeCaptureAdapter : IVideoCaptureAdapter
    {
        public bool OpenWithApiResult { get; set; }
        public bool OpenResult { get; set; }
        public bool IsOpenedResult { get; set; }
        public bool ReadResult { get; set; }

        public bool OpenWithApiCalled { get; private set; }
        public bool OpenCalledWithoutApi { get; private set; }
        public bool ReleaseCalled { get; private set; }
        public bool DisposeCalled { get; private set; }
        public List<(VideoCaptureProperties Property, double Value)> SetCalls { get; } = [];

        public bool Open(int deviceIndex, VideoCaptureAPIs api)
        {
            OpenWithApiCalled = true;
            return OpenWithApiResult;
        }

        public bool Open(int deviceIndex)
        {
            OpenCalledWithoutApi = true;
            return OpenResult;
        }

        public bool IsOpened() => IsOpenedResult;

        public bool Set(VideoCaptureProperties property, double value)
        {
            SetCalls.Add((property, value));
            return true;
        }

        public bool Read(Mat frame) => ReadResult;

        public void Release() => ReleaseCalled = true;

        public void Dispose() => DisposeCalled = true;
    }
}
