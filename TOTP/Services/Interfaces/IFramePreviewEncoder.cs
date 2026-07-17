using OpenCvSharp;

namespace TOTP.Services.Interfaces;

public interface IFramePreviewEncoder
{
    byte[] Encode(Mat frame);
}
