using System.Buffers.Binary;
using System.Security.Cryptography;
using OpenCvSharp;

namespace TOTP.Camera.OpenCv;

internal sealed class OpenCvCameraSession(
    VideoCapture capture,
    QRCodeDetector detector) : ICameraSession
{
    private const int MaximumEncodedFrameBytes = 4 * 1024 * 1024;
    private const int EnhancedDecodeLongEdge = 1920;
    private readonly VideoCapture _capture = capture ?? throw new ArgumentNullException(nameof(capture));
    private readonly QRCodeDetector _detector = detector ?? throw new ArgumentNullException(nameof(detector));
    private bool _disposed;

    public bool TryRead(bool decodeQr, out CameraFrame frame)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        using var mat = new Mat();
        if (!_capture.Read(mat) || mat.Empty())
        {
            frame = default;
            return false;
        }

        var encoded = mat.ImEncode(".png");
        if (encoded.Length == 0 || encoded.Length > MaximumEncodedFrameBytes)
        {
            CryptographicOperations.ZeroMemory(encoded);
            frame = default;
            return false;
        }

        var fingerprint = ComputeFingerprint(encoded);
        var decoded = decodeQr ? DecodeQr(mat) : null;
        frame = new CameraFrame(encoded, fingerprint, decoded);
        return true;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        try
        {
            _capture.Release();
        }
        finally
        {
            _capture.Dispose();
            _detector.Dispose();
        }
    }

    private static ulong ComputeFingerprint(ReadOnlySpan<byte> encoded)
    {
        Span<byte> digest = stackalloc byte[32];
        SHA256.HashData(encoded, digest);
        return BinaryPrimitives.ReadUInt64LittleEndian(digest);
    }

    private string? DecodeQr(Mat frame)
    {
        var decoded = Detect(frame);
        if (!string.IsNullOrWhiteSpace(decoded)) return decoded;

        using var grayscale = new Mat();
        Cv2.CvtColor(frame, grayscale, ColorConversionCodes.BGR2GRAY);
        decoded = Detect(grayscale);
        if (!string.IsNullOrWhiteSpace(decoded)) return decoded;

        using var threshold = new Mat();
        Cv2.Threshold(
            grayscale,
            threshold,
            0,
            255,
            ThresholdTypes.Binary | ThresholdTypes.Otsu);
        decoded = Detect(threshold);
        if (!string.IsNullOrWhiteSpace(decoded)) return decoded;

        var longestEdge = Math.Max(grayscale.Width, grayscale.Height);
        if (longestEdge <= 0 || longestEdge >= EnhancedDecodeLongEdge) return null;

        var scale = (double)EnhancedDecodeLongEdge / longestEdge;
        using var enlarged = new Mat();
        Cv2.Resize(
            grayscale,
            enlarged,
            new Size(),
            scale,
            scale,
            InterpolationFlags.Cubic);
        decoded = Detect(enlarged);
        return string.IsNullOrWhiteSpace(decoded) ? null : decoded;
    }

    private string? Detect(Mat frame) => _detector.DetectAndDecode(frame, out _);
}
