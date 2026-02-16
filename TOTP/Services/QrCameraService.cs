using System.Windows;
using System.Windows.Media;

namespace TOTP.Services;

using OpenCvSharp;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

public sealed class QrCameraService : IDisposable
{
    private readonly object _gate = new();
    private VideoCapture? _cap;
    private bool _initialized;

    public int DeviceIndex { get; private set; } = 0;
    public int Width { get; private set; } = 1280;
    public int Height { get; private set; } = 720;
    public int Fps { get; private set; } = 30;

    public bool IsOpen => _cap != null && _cap.IsOpened();

    public async Task InitializeAsync(int deviceIndex = 0, int width = 1280, int height = 720, int fps = 30)
    {
        if (_initialized) return;

        await Task.Run(() =>
        {
            lock (_gate)
            {
                if (_initialized) return;

                DeviceIndex = deviceIndex; Width = width; Height = height; Fps = fps;

                _cap = new VideoCapture();
                if (!_cap.Open(DeviceIndex, VideoCaptureAPIs.DSHOW))
                {
                    if (!_cap.Open(DeviceIndex))
                        throw new InvalidOperationException("Unable to open camera.");
                }

                _cap.Set(VideoCaptureProperties.FrameWidth, Width);
                _cap.Set(VideoCaptureProperties.FrameHeight, Height);
                _cap.Set(VideoCaptureProperties.Fps, Fps);
                try { _cap.Set(VideoCaptureProperties.FourCC, FourCC.MJPG); } catch { }
                try { _cap.Set(VideoCaptureProperties.BufferSize, 1); } catch { }

                if (!_cap.IsOpened()) throw new InvalidOperationException("Camera failed to open.");

                using var tmp = new Mat();
                for (int i = 0; i < 4; i++) _cap.Read(tmp); // warm frames

                _initialized = true;
            }
        });//.ConfigureAwait(false);
    }

    // QrCameraService.cs
    // using System;
    // using System.Diagnostics;
    // using System.Threading;
    // using System.Threading.Tasks;
    // using System.Windows.Controls;
    // using System.Windows.Media.Imaging;
    // using OpenCvSharp;

    public async Task StartPreviewLoopAsync(
        Image target,
        CancellationToken ct,
        TaskCompletionSource<bool>? firstFrameTcs = null,
        Action<string>? onDecoded = null)
    {
        if (!_initialized) throw new InvalidOperationException("InitializeAsync first.");

        await Task.Run(async () =>
        {
            using var frame = new Mat();
            WriteableBitmap? wb = null;

            // Create detector once (expensive to construct per frame)
            var detector = new QRCodeDetector();

            // Throttle decode to e.g. every 150 ms
            var sw = Stopwatch.StartNew();
            bool decoding = false;

            bool firstSent = false;

            while (!ct.IsCancellationRequested)
            {
                if (!_cap.Read(frame) || frame.Empty())
                    continue;

                // render preview
                await target.Dispatcher.InvokeAsync(() =>
                {
                    wb = CvToWpf.EnsureWriteableBitmap(wb!, frame);
                    if (!ReferenceEquals(target.Source, wb))
                        target.Source = wb;
                    CvToWpf.Write(frame, wb);
                }, System.Windows.Threading.DispatcherPriority.Render, ct);

                if (!firstSent)
                {
                    firstSent = true;
                    firstFrameTcs?.TrySetResult(true);
                }

                // Try decode (throttled, background, on a clone to avoid data races)
                if (!decoding && sw.ElapsedMilliseconds >= 150 && onDecoded != null)
                {
                    sw.Restart();
                    decoding = true;

                    _ = Task.Run(() =>
                    {
                        using var clone = frame.Clone(); // avoid mutating frame being rendered
                        string text = detector.DetectAndDecode(clone, out _);
                        decoding = false;

                        if (!string.IsNullOrWhiteSpace(text))
                            onDecoded(text);
                    }, ct);
                }

                await Task.Delay(TimeSpan.FromMilliseconds(1000.0 / Math.Max(1, Fps)), ct);
            }
        }, ct);//.ConfigureAwait(false);
    }


    public async Task StartPreviewAsync(Image target, CancellationToken ct)
    {
        if (!_initialized) throw new InvalidOperationException("InitializeAsync first.");
        WriteableBitmap? wb = null;

        await Task.Run(async () =>
        {
            using var frame = new Mat();

            while (!ct.IsCancellationRequested)
            {
                if (!_cap.Read(frame) || frame.Empty()) continue;

                await target.Dispatcher.InvokeAsync(() =>
                {
                    wb = CvToWpf.EnsureWriteableBitmap(wb, frame);
                    if (!ReferenceEquals(target.Source, wb))
                        target.Source = wb;
                    CvToWpf.Write(frame, wb);
                });

                await Task.Delay(TimeSpan.FromMilliseconds(1000.0 / Math.Max(1, Fps)), ct);
            }
        }, ct);//.ConfigureAwait(false);
    }

    public void Dispose()
    {
        lock (_gate)
        {
            try { _cap?.Release(); } catch { }
            _cap?.Dispose();
            _cap = null;
            _initialized = false;
        }
    }
}

public static class CvToWpf
{
    // Create (or reuse) a WriteableBitmap compatible with the mat
    public static WriteableBitmap EnsureWriteableBitmap(WriteableBitmap wb, Mat mat)
    {
        if (mat.Empty()) throw new ArgumentException("Mat is empty.", nameof(mat));
        // We will write BGRA32 into WPF
        if (wb == null || wb.PixelWidth != mat.Cols || wb.PixelHeight != mat.Rows || wb.Format != PixelFormats.Bgra32)
        {
            wb = new WriteableBitmap(mat.Cols, mat.Rows, 96, 96, PixelFormats.Bgra32, null);
        }
        return wb;
    }

    // Write Mat pixels into the WriteableBitmap (converts to BGRA32 when needed)
    public static void Write(Mat mat, WriteableBitmap wb)
    {
        if (mat.Empty()) return;

        // Convert any incoming format to BGRA32
        using var bgra = new Mat();
        switch (mat.Channels())
        {
            case 4:
                // Assume mat already BGRA; copy header only (no deep copy)
                mat.CopyTo(bgra);
                break;
            case 3:
                Cv2.CvtColor(mat, bgra, ColorConversionCodes.BGR2BGRA);
                break;
            case 1:
                Cv2.CvtColor(mat, bgra, ColorConversionCodes.GRAY2BGRA);
                break;
            default:
                Cv2.CvtColor(mat, bgra, ColorConversionCodes.BGR2BGRA);
                break;
        }

        int width = bgra.Cols;
        int height = bgra.Rows;
        int stride = (int)bgra.Step(); // bytes per row

        // WritePixels must run on the UI thread
        wb.WritePixels(new Int32Rect(0, 0, width, height), bgra.Data, stride * height, stride);
    }
}
