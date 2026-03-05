using OpenCvSharp;
using System;
using System.Runtime.InteropServices;
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

            PixelFormat wpfFormat;
            Mat src = mat;
            Mat? converted = null;

            switch (mat.Type().Channels)
            {
                case 1:
                    wpfFormat = PixelFormats.Gray8;
                    if (mat.Type() != MatType.CV_8UC1)
                    {
                        converted = new Mat();
                        mat.ConvertTo(converted, MatType.CV_8UC1);
                        src = converted;
                    }
                    break;

                case 3:
                    wpfFormat = PixelFormats.Bgr24;
                    if (mat.Type() != MatType.CV_8UC3)
                    {
                        converted = new Mat();
                        mat.ConvertTo(converted, MatType.CV_8UC3);
                        src = converted;
                    }
                    break;

                case 4:
                    wpfFormat = PixelFormats.Bgra32;
                    if (mat.Type() != MatType.CV_8UC4)
                    {
                        converted = new Mat();
                        mat.ConvertTo(converted, MatType.CV_8UC4);
                        src = converted;
                    }
                    break;

                default:
                    wpfFormat = PixelFormats.Bgr24;
                    converted = new Mat();
                    if (mat.Channels() == 2) Cv2.CvtColor(mat, converted, ColorConversionCodes.BGR5652BGR);
                    else Cv2.CvtColor(mat, converted, ColorConversionCodes.BGRA2BGR);
                    src = converted;
                    break;
            }

            try
            {
                var stride = (int)src.Step();
                var size = stride * src.Height;
                var pixels = new byte[size];
                Marshal.Copy(src.Data, pixels, 0, size);

                return BitmapSource.Create(
                    src.Width,
                    src.Height,
                    96,
                    96,
                    wpfFormat,
                    null,
                    pixels,
                    stride);
            }
            finally
            {
                converted?.Dispose();
            }
        }
    }
}
