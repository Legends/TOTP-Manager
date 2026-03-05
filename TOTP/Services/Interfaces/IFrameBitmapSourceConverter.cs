using OpenCvSharp;
using System.Windows.Media.Imaging;

namespace TOTP.Services.Interfaces;

public interface IFrameBitmapSourceConverter
{
    BitmapSource Convert(Mat frame);
}
