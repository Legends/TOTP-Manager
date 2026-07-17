using OpenCvSharp;
using TOTP.Services.Interfaces;

namespace TOTP.Services;

public sealed class MatFramePreviewEncoder : IFramePreviewEncoder
{
    public byte[] Encode(Mat frame) => frame.ImEncode(".png");
}
