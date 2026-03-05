using OpenCvSharp;
using System.Windows.Media.Imaging;
using TOTP.Infrastructure.Extensions;
using TOTP.Services.Interfaces;

namespace TOTP.Services;

public sealed class MatBitmapSourceConverter : IFrameBitmapSourceConverter
{
    public BitmapSource Convert(Mat frame)
    {
        var bmp = frame.ToBitmapSource();
        bmp.Freeze();
        return bmp;
    }
}
