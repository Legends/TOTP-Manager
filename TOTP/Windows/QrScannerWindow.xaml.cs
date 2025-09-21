using OpenCvSharp;
using Syncfusion.Windows.Shared;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using TOTP.Extensions;

namespace TOTP.Windows
{
    public partial class QrScannerWindow : ChromelessWindow
    {
        private CancellationTokenSource? _cts;
        public string? DecodedText { get; private set; }

        public QrScannerWindow() => InitializeComponent();

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Overlay.Visibility = Visibility.Visible;     // show “initializing” immediately
            _cts = new CancellationTokenSource();
            _ = RunCameraAsync(_cts.Token);              // fire-and-forget; don’t block UI thread
        }

        private async Task RunCameraAsync(CancellationToken token)
        {
            // 1) Open the camera on a background thread so the window can paint instantly
            var cap = await Task.Run(() =>
            {
                try
                {
                    // Prefer DirectShow on Windows; fall back to default
                    var c = new VideoCapture(0, VideoCaptureAPIs.DSHOW);
                    if (!c.IsOpened()) c.Open(0);
                    if (!c.IsOpened()) return null;

                    c.Set(VideoCaptureProperties.FrameWidth, 1280);
                    c.Set(VideoCaptureProperties.FrameHeight, 720);
                    c.Set(VideoCaptureProperties.Fps, 30);
                    try { c.Set(VideoCaptureProperties.FourCC, FourCC.MJPG); } catch { }
                    try { c.Set(VideoCaptureProperties.BufferSize, 1); } catch { }

                    return c;
                }
                catch { return null; }
            });

            if (cap == null)
            {
                await Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show("No camera found.", "Camera", MessageBoxButton.OK, MessageBoxImage.Warning);
                    DialogResult = false;
                    Close();
                });
                return;
            }

            try
            {
                using var frame = new Mat();
                var detector = new QRCodeDetector();
                bool firstFrameShown = false;

                // 2) Capture/Decode loop runs on this background Task
                while (!token.IsCancellationRequested)
                {
                    if (!cap.Read(frame) || frame.Empty())
                        continue;

                    // Try decoding QR (your original fast path)
                    string decoded = detector.DetectAndDecode(frame, out _);
                    if (!string.IsNullOrEmpty(decoded))
                    {
                        DecodedText = decoded;
                        await Dispatcher.InvokeAsync(() =>
                        {
                            DialogResult = true;
                            Close();
                        });
                        return;
                    }

                    // Preview: convert to BitmapSource off-UI, then assign on UI
                    var bmp = frame.ToBitmapSource(); // your extension
                    bmp.Freeze();
                    await Dispatcher.InvokeAsync(() =>
                    {
                        Preview.Source = bmp;
                        if (!firstFrameShown)
                        {
                            Overlay.Visibility = Visibility.Collapsed; // hide loading once first frame is visible
                            firstFrameShown = true;
                        }
                    }, DispatcherPriority.Render, token);

                    await Task.Delay(10, token);
                }
            }
            finally
            {
                try { cap.Release(); } catch { }
                cap.Dispose();
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
}
