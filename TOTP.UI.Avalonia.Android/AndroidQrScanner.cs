using System.Security.Cryptography;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Provider;
using TOTP.Avalonia.Mobile.Platform;
using ZXing;
using ZXing.Common;
using AndroidResult = Android.App.Result;

namespace TOTP.Avalonia.Android;

internal sealed class AndroidQrScanner(AndroidActivityProvider activityProvider) : IMobileQrScanner
{
    private const int CaptureRequestCode = 0x4f54;
    private const int MaximumPixels = 16_000_000;

    public async Task<MobileQrScanResult> ScanAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var activity = activityProvider.GetCurrent();
        if (activity is null) return MobileQrScanResult.Unavailable;

        using var cameraIntent = new Intent(MediaStore.ActionImageCapture);
        var packageManager = activity.PackageManager;
        if (packageManager is null || cameraIntent.ResolveActivity(packageManager) is null)
            return MobileQrScanResult.Unavailable;

        var completion = new TaskCompletionSource<(AndroidResult Code, Intent? Data)>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        void OnActivityResult(int requestCode, AndroidResult resultCode, Intent? data)
        {
            if (requestCode == CaptureRequestCode)
                completion.TrySetResult((resultCode, data));
        }

        activity.ActivityResultReceived += OnActivityResult;
        using var cancellation = cancellationToken.Register(() =>
            completion.TrySetCanceled(cancellationToken));
        try
        {
            activity.StartActivityForResult(cameraIntent, CaptureRequestCode);
            var capture = await completion.Task;
            if (capture.Code != AndroidResult.Ok) return MobileQrScanResult.Cancelled;

            using var bitmap = capture.Data?.Extras?.Get("data") as Bitmap;
            return bitmap is null ? MobileQrScanResult.Failed : Decode(bitmap);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            return MobileQrScanResult.Failed;
        }
        finally
        {
            activity.ActivityResultReceived -= OnActivityResult;
        }
    }

    private static MobileQrScanResult Decode(Bitmap bitmap)
    {
        var width = bitmap.Width;
        var height = bitmap.Height;
        var pixelCount = checked(width * height);
        if (width <= 0 || height <= 0 || pixelCount > MaximumPixels)
            return MobileQrScanResult.Failed;

        var pixels = new int[pixelCount];
        byte[]? rgb = null;
        try
        {
            bitmap.GetPixels(pixels, 0, width, 0, 0, width, height);
            rgb = new byte[checked(pixelCount * 3)];
            for (var index = 0; index < pixelCount; index++)
            {
                var color = pixels[index];
                var offset = index * 3;
                rgb[offset] = (byte)((color >> 16) & 0xff);
                rgb[offset + 1] = (byte)((color >> 8) & 0xff);
                rgb[offset + 2] = (byte)(color & 0xff);
            }

            var reader = new BarcodeReaderGeneric
            {
                AutoRotate = true,
                Options = new DecodingOptions
                {
                    PossibleFormats = [BarcodeFormat.QR_CODE],
                    TryHarder = true
                }
            };
            var decoded = reader.Decode(
                rgb,
                width,
                height,
                RGBLuminanceSource.BitmapFormat.RGB24);
            return string.IsNullOrWhiteSpace(decoded?.Text)
                ? MobileQrScanResult.Failed
                : MobileQrScanResult.Successful(decoded.Text);
        }
        finally
        {
            Array.Clear(pixels);
            if (rgb is not null) CryptographicOperations.ZeroMemory(rgb);
        }
    }
}
