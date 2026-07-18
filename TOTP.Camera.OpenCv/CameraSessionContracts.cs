namespace TOTP.Camera.OpenCv;

public enum CameraOpenFailure
{
    None = 0,
    PermissionDenied,
    NoCamera,
    NativeRuntimeUnavailable,
    Unexpected
}

public readonly record struct CameraSessionOpenResult(
    ICameraSession? Session,
    CameraOpenFailure Failure)
{
    public static CameraSessionOpenResult Success(ICameraSession session) =>
        new(session ?? throw new ArgumentNullException(nameof(session)), CameraOpenFailure.None);

    public static CameraSessionOpenResult Failed(CameraOpenFailure failure)
    {
        if (failure == CameraOpenFailure.None)
            throw new ArgumentOutOfRangeException(nameof(failure));

        return new(null, failure);
    }
}

public readonly record struct CameraFrame(
    byte[] PreviewPng,
    ulong Fingerprint,
    string? DecodedText);

public interface ICameraSessionFactory
{
    CameraSessionOpenResult OpenDefault();
}

public interface ICameraSession : IDisposable
{
    bool TryRead(bool decodeQr, out CameraFrame frame);
}
