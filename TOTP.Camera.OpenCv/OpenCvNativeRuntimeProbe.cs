using OpenCvSharp;

namespace TOTP.Camera.OpenCv;

public sealed record OpenCvNativeRuntimeProbeResult(bool IsAvailable, string Version);

public static class OpenCvNativeRuntimeProbe
{
    public static OpenCvNativeRuntimeProbeResult Probe()
    {
        try
        {
            var version = Cv2.GetVersionString();
            using var detector = new QRCodeDetector();
            using var capture = new VideoCapture();
            return string.IsNullOrWhiteSpace(version)
                ? new(false, string.Empty)
                : new(true, version);
        }
        catch (Exception)
        {
            return new(false, string.Empty);
        }
    }
}
