using OpenCvSharp;
using TOTP.Core.Services.Interfaces;

namespace TOTP.Camera.OpenCv;

public sealed class OpenCvCameraBackendWarmup : ICameraBackendWarmup
{
    public void Warmup()
    {
        _ = Cv2.GetVersionString();
        using var detector = new QRCodeDetector();
        using var capture = new VideoCapture();
    }
}
