using System.Security;
using OpenCvSharp;

namespace TOTP.Camera.OpenCv;

public sealed class OpenCvCameraSessionFactory : ICameraSessionFactory
{
    public CameraSessionOpenResult OpenDefault()
    {
        VideoCapture? capture = null;
        try
        {
            capture = new VideoCapture();
            var opened = capture.IsOpened();

            if (!opened && OperatingSystem.IsWindows())
            {
                try
                {
                    opened = capture.Open(0, VideoCaptureAPIs.DSHOW);
                }
                catch (OpenCVException)
                {
                    opened = false;
                }
            }

            if (!opened)
                opened = capture.Open(0);

            if (!opened || !capture.IsOpened())
            {
                capture.Dispose();
                return CameraSessionOpenResult.Failed(CameraOpenFailure.NoCamera);
            }

            ConfigureCapture(capture);
            var session = new OpenCvCameraSession(capture, new QRCodeDetector());
            capture = null;
            return CameraSessionOpenResult.Success(session);
        }
        catch (UnauthorizedAccessException)
        {
            capture?.Dispose();
            return CameraSessionOpenResult.Failed(CameraOpenFailure.PermissionDenied);
        }
        catch (SecurityException)
        {
            capture?.Dispose();
            return CameraSessionOpenResult.Failed(CameraOpenFailure.PermissionDenied);
        }
        catch (Exception ex) when (IsNativeRuntimeFailure(ex))
        {
            capture?.Dispose();
            return CameraSessionOpenResult.Failed(CameraOpenFailure.NativeRuntimeUnavailable);
        }
        catch
        {
            capture?.Dispose();
            return CameraSessionOpenResult.Failed(CameraOpenFailure.Unexpected);
        }
    }

    private static void ConfigureCapture(VideoCapture capture)
    {
        capture.Set(VideoCaptureProperties.FrameWidth, 640);
        capture.Set(VideoCaptureProperties.FrameHeight, 480);
        capture.Set(VideoCaptureProperties.Fps, 30);
        TrySet(capture, VideoCaptureProperties.FourCC, FourCC.MJPG);
        TrySet(capture, VideoCaptureProperties.BufferSize, 1);
    }

    private static void TrySet(VideoCapture capture, VideoCaptureProperties property, double value)
    {
        try
        {
            capture.Set(property, value);
        }
        catch (OpenCVException)
        {
            // Optional tuning is backend-dependent.
        }
    }

    private static bool IsNativeRuntimeFailure(Exception exception) =>
        exception is DllNotFoundException
        or BadImageFormatException
        or TypeInitializationException;
}
