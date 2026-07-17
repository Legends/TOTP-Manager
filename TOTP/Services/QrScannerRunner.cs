using OpenCvSharp;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using TOTP.Core.Services.Interfaces;
using TOTP.Services.Interfaces;

namespace TOTP.Services;

public sealed class QrScannerRunner(
    IVideoCaptureFactory videoCaptureFactory,
    IQrCodeDecoder qrCodeDecoder,
    IFramePreviewEncoder framePreviewEncoder) : IQrScannerRunner
{
    public async Task RunAsync(
        CancellationToken token,
        Action<byte[]> onPreview,
        Action onFirstFrame,
        Action<string> onDecoded)
    {
        IVideoCaptureAdapter? cap = null;

        try
        {
            cap = await Task.Run(() =>
            {
                try
                {
                    var c = videoCaptureFactory.Create();
                    if (!c.IsOpened())
                    {
                        // Prefer DirectShow on Windows for faster and more reliable startup.
                        var opened = false;
                        try { opened = c.Open(0, VideoCaptureAPIs.DSHOW); } catch { }
                        if (!opened)
                        {
                            opened = c.Open(0);
                        }
                    }

                    if (!c.IsOpened())
                    {
                        c.Dispose();
                        return null;
                    }

                    // Keep startup resolution moderate to reduce camera warm-up latency.
                    c.Set(VideoCaptureProperties.FrameWidth, 640);
                    c.Set(VideoCaptureProperties.FrameHeight, 480);
                    c.Set(VideoCaptureProperties.Fps, 30);
                    try { c.Set(VideoCaptureProperties.FourCC, FourCC.MJPG); } catch { }
                    try { c.Set(VideoCaptureProperties.BufferSize, 1); } catch { }

                    return c;
                }
                catch
                {
                    return null;
                }
            }, token).ConfigureAwait(false);

            if (cap == null)
            {
                throw new InvalidOperationException("No camera found.");
            }

            using var frame = new Mat();
            using var previousFrame = new Mat();
            var firstFrameShown = false;
            var consecutiveReadFailures = 0;
            var unchangedFrameStopwatch = Stopwatch.StartNew();
            var decodeStopwatch = Stopwatch.StartNew();

            while (!token.IsCancellationRequested)
            {
                if (!cap.Read(frame) || frame.Empty())
                {
                    consecutiveReadFailures++;
                    if (consecutiveReadFailures >= 10)
                    {
                        throw new InvalidOperationException("Camera became unavailable.");
                    }

                    await Task.Delay(50, token).ConfigureAwait(false);
                    continue;
                }

                consecutiveReadFailures = 0;

                var frameChanged = previousFrame.Empty()
                    || previousFrame.Size() != frame.Size()
                    || previousFrame.Type() != frame.Type()
                    || Cv2.Norm(frame, previousFrame, NormTypes.L1) > 0;

                if (frameChanged)
                {
                    frame.CopyTo(previousFrame);
                    unchangedFrameStopwatch.Restart();
                }
                else if (unchangedFrameStopwatch.Elapsed >= TimeSpan.FromMilliseconds(1500))
                {
                    throw new InvalidOperationException("Camera stopped providing new frames.");
                }

                onPreview(framePreviewEncoder.Encode(frame));

                if (!firstFrameShown)
                {
                    firstFrameShown = true;
                    onFirstFrame();
                }

                // Decode at a throttled interval so preview appears quickly and smoothly.
                if (decodeStopwatch.ElapsedMilliseconds >= 120)
                {
                    decodeStopwatch.Restart();
                    var decoded = qrCodeDecoder.Decode(frame);
                    if (!string.IsNullOrEmpty(decoded))
                    {
                        onDecoded(decoded);
                        return;
                    }
                }

                await Task.Delay(10, token).ConfigureAwait(false);
            }
        }
        finally
        {
            try { cap?.Release(); } catch { }
            cap?.Dispose();
        }
    }
}
