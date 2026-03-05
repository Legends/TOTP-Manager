using OpenCvSharp;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using TOTP.Services.Interfaces;

namespace TOTP.Services;

public sealed class QrScannerRunner(
    IVideoCaptureFactory videoCaptureFactory,
    IQrCodeDecoder qrCodeDecoder,
    IFrameBitmapSourceConverter frameBitmapSourceConverter) : IQrScannerRunner
{
    public async Task RunAsync(
        CancellationToken token,
        Action<BitmapSource> onPreview,
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
                    if (!c.IsOpened()) c.Open(0);
                    if (!c.IsOpened())
                    {
                        c.Dispose();
                        return null;
                    }

                    c.Set(VideoCaptureProperties.FrameWidth, 1280);
                    c.Set(VideoCaptureProperties.FrameHeight, 720);
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
            var firstFrameShown = false;

            while (!token.IsCancellationRequested)
            {
                if (!cap.Read(frame) || frame.Empty())
                {
                    continue;
                }

                var decoded = qrCodeDecoder.Decode(frame);
                if (!string.IsNullOrEmpty(decoded))
                {
                    onDecoded(decoded);
                    return;
                }

                var bmp = frameBitmapSourceConverter.Convert(frame);
                onPreview(bmp);

                if (!firstFrameShown)
                {
                    firstFrameShown = true;
                    onFirstFrame();
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
