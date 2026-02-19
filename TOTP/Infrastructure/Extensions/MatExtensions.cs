using OpenCvSharp;
using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TOTP.Infrastructure.Extensions
{
    public static class MatExtensions
    {
        public static BitmapSource ToBitmapSource(this Mat mat)
        {
            if (mat is null) throw new ArgumentNullException(nameof(mat));
            if (mat.Empty()) throw new ArgumentException("Empty Mat.", nameof(mat));

            // Choose WPF pixel format based on channels
            PixelFormat wpfFormat;
            Mat src = mat;

            switch (mat.Type().Channels)
            {
                case 1:
                    wpfFormat = PixelFormats.Gray8;
                    if (mat.Type() != MatType.CV_8UC1)
                    {
                        var tmp = new Mat();
                        mat.ConvertTo(tmp, MatType.CV_8UC1);
                        src = tmp;
                    }
                    break;

                case 3:
                    wpfFormat = PixelFormats.Bgr24;
                    if (mat.Type() != MatType.CV_8UC3)
                    {
                        var tmp = new Mat();
                        mat.ConvertTo(tmp, MatType.CV_8UC3);
                        src = tmp;
                    }
                    break;

                case 4:
                    wpfFormat = PixelFormats.Bgra32;
                    if (mat.Type() != MatType.CV_8UC4)
                    {
                        var tmp = new Mat();
                        mat.ConvertTo(tmp, MatType.CV_8UC4);
                        src = tmp;
                    }
                    break;

                default:
                    // Fallback: convert to BGR24
                    wpfFormat = PixelFormats.Bgr24;
                    var conv = new Mat();
                    if (mat.Channels() == 2) Cv2.CvtColor(mat, conv, ColorConversionCodes.BGR5652BGR);
                    else Cv2.CvtColor(mat, conv, ColorConversionCodes.BGRA2BGR);
                    src = conv;
                    break;
            }

            // Create BitmapSource directly from Mat buffer (zero-copy-ish; respects stride)
            return BitmapSource.Create(
                src.Width,
                src.Height,
                96, 96,
                wpfFormat,
                null,
                src.Data,
                (int)(src.Step() * src.Height),
                (int)src.Step());
        }
    }
}
