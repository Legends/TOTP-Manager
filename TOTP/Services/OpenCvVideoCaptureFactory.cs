using OpenCvSharp;
using TOTP.Services.Interfaces;

namespace TOTP.Services;

public sealed class OpenCvVideoCaptureFactory : IVideoCaptureFactory
{
    public IVideoCaptureAdapter Create() => new OpenCvVideoCaptureAdapter(new VideoCapture());

    private sealed class OpenCvVideoCaptureAdapter(VideoCapture inner) : IVideoCaptureAdapter
    {
        public bool Open(int deviceIndex, VideoCaptureAPIs api) => inner.Open(deviceIndex, api);
        public bool Open(int deviceIndex) => inner.Open(deviceIndex);
        public bool IsOpened() => inner.IsOpened();
        public bool Set(VideoCaptureProperties property, double value) => inner.Set(property, value);
        public bool Read(Mat frame) => inner.Read(frame);
        public void Release() => inner.Release();
        public void Dispose() => inner.Dispose();
    }
}
