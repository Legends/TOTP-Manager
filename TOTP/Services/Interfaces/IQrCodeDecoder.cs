using OpenCvSharp;

namespace TOTP.Services.Interfaces;

public interface IQrCodeDecoder
{
    string Decode(Mat frame);
}
