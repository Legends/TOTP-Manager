using OpenCvSharp;
using System;
using TOTP.Services.Interfaces;

namespace TOTP.Services;

public sealed class OpenCvQrCodeDecoder : IQrCodeDecoder, IDisposable
{
    private readonly QRCodeDetector _detector = new();

    public string Decode(Mat frame) => _detector.DetectAndDecode(frame, out _);

    public void Dispose() => _detector.Dispose();
}
