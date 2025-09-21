namespace TOTP.Services;

using System;
using System.Threading;
using System.Threading.Tasks;

public static class QrWarmupService
{
    private static readonly SemaphoreSlim _gate = new(1, 1);
    private static QrCameraService? _camera;
    private static CancellationTokenSource? _keepAliveCts;
    private static bool _ready;

    public static QrCameraService Camera => _camera;

    // Call once at app start (or just before showing the dialog)
    public static async Task WarmUpAsync(int deviceIndex = 0, int width = 1280, int height = 720, int fps = 30)
    {
        if (_ready) return;

        await _gate.WaitAsync();
        try
        {
            if (_ready) return;
            _camera = new QrCameraService();
            await _camera.InitializeAsync(deviceIndex, width, height, fps);
            _ready = true;
        }
        finally { _gate.Release(); }
    }

    /// Keep the camera hot for a while after closing the dialog
    public static async Task ReleaseAfterDelayAsync(TimeSpan delay)
    {
        CancelKeepAlive();
        _keepAliveCts = new CancellationTokenSource();
        var token = _keepAliveCts.Token;

        try
        {
            await Task.Delay(delay, token);
            await _gate.WaitAsync(token);
            try
            {
                _camera?.Dispose();
                _camera = null;
                _ready = false;
            }
            finally { _gate.Release(); }
        }
        catch (TaskCanceledException) { /* another dialog reopened in time */ }
    }

    public static void CancelKeepAlive()
    {
        try { _keepAliveCts?.Cancel(); } catch { }
        _keepAliveCts?.Dispose();
        _keepAliveCts = null;
    }
}
