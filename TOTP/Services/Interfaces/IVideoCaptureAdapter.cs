using OpenCvSharp;
using System;

namespace TOTP.Services.Interfaces;

public interface IVideoCaptureAdapter : IDisposable
{
    bool Open(int deviceIndex, VideoCaptureAPIs api);
    bool Open(int deviceIndex);
    bool IsOpened();
    bool Set(VideoCaptureProperties property, double value);
    bool Read(Mat frame);
    void Release();
}
