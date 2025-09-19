using OpenCvSharp;
using Syncfusion.Windows.Shared;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace TOTP.Windows
{
    /// <summary>
    /// Interaction logic for QrScannerWindow.xaml
    /// </summary>
    public partial class QrScannerWindow : ChromelessWindow
    {
        private CancellationTokenSource? _cts;
        public string? DecodedText { get; private set; }

        public QrScannerWindow() => InitializeComponent();

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _cts = new CancellationTokenSource();
            _ = RunCameraAsync(_cts.Token);
        }

        private async Task RunCameraAsync(CancellationToken token)
        {
            // 0 = default (internal laptop camera in most cases)
            using var cap = new VideoCapture(0, VideoCaptureAPIs.ANY);

            if (!cap.Open(0))
            {
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show("No camera found.", "Camera", MessageBoxButton.OK, MessageBoxImage.Warning);
                    DialogResult = false;
                    Close();
                });
                return;
            }

            // Set some reasonable defaults (not all cams respect these)
            cap.Set(VideoCaptureProperties.FrameWidth, 1280);
            cap.Set(VideoCaptureProperties.FrameHeight, 720);
            cap.Set(VideoCaptureProperties.Fps, 30);

            using var frame = new Mat();
            var detector = new QRCodeDetector();

            while (!token.IsCancellationRequested)
            {
                if (!cap.Read(frame) || frame.Empty())
                    continue;

                // Try decoding QR
                string decoded = detector.DetectAndDecode(frame, out _);
                if (!string.IsNullOrEmpty(decoded))
                {
                    DecodedText = decoded;
                    Dispatcher.Invoke(() =>
                    {
                        DialogResult = true;
                        Close();
                    });
                    return;
                }

                // Show preview (Mat → BitmapSource via our helper)
                var bmp = frame.ToBitmapSource();
                bmp.Freeze(); // cross-thread use
                Dispatcher.Invoke(() => Preview.Source = bmp);

                await Task.Delay(10, token);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            _cts?.Cancel();
            DialogResult = false;
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }
    }




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
