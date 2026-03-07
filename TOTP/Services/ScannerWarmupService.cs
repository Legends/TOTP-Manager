using Microsoft.Extensions.Logging;
using OpenCvSharp;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using TOTP.Services.Interfaces;

namespace TOTP.Services;

public sealed class ScannerWarmupService(
    IVideoCaptureFactory videoCaptureFactory,
    ILogger<ScannerWarmupService> logger) : IScannerWarmupService
{
    private int _started;

    public void StartWarmupInBackground(string trigger)
    {
        if (Interlocked.Exchange(ref _started, 1) == 1)
        {
            logger.LogDebug("warmup.scannerbackend.skip trigger={Trigger} reason=already_started", trigger);
            return;
        }

        _ = Task.Run(() =>
        {
            var sw = Stopwatch.StartNew();
            logger.LogInformation("warmup.scannerbackend.begin trigger={Trigger}", trigger);
            try
            {
                _ = Cv2.GetVersionString();
                using var detector = new QRCodeDetector();
                using var capture = videoCaptureFactory.Create();
                logger.LogInformation("warmup.scannerbackend.end trigger={Trigger} elapsed_ms={ElapsedMs}", trigger, sw.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "warmup.scannerbackend.fail trigger={Trigger} elapsed_ms={ElapsedMs}", trigger, sw.ElapsedMilliseconds);
            }
        });
    }
}

