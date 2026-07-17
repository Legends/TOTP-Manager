using System;
using System.Windows;
using System.Windows.Media.Imaging;
using System.IO;
using System.Security.Cryptography;
using TOTP.Core.Security.Interfaces;
using TOTP.Core.Services.Interfaces;
using TOTP.Services.Interfaces;
using TOTP.Views;

namespace TOTP.Services;

public sealed class QrPreviewService : IQrPreviewService
{
    private const double InlineQrBaseWidth = 160d;
    private Window? _overlayWindow;
    private Window? _previewWindow;
    private bool _isClosing;
    private readonly ISettingsService _settingsService;
    public double PreviewScaleFactor { get; set; }

    public QrPreviewService(ISettingsService settingsService)
    {
        _settingsService = settingsService;
        PreviewScaleFactor = _settingsService.Current.QrPreviewScaleFactor > 0
            ? _settingsService.Current.QrPreviewScaleFactor
            : 2.0;
    }

    public void Toggle(ReadOnlyMemory<byte> pngImage)
    {
        if (_previewWindow != null || _overlayWindow != null)
        {
            Close();
            return;
        }

        if (pngImage.IsEmpty)
        {
            return;
        }

        var source = DecodePng(pngImage);

        // Scale from the inline QR size shown in the main view (not raw bitmap pixels),
        // so "2x" really means twice the normal on-screen QR.
        var safeScale = Math.Max(1d, PreviewScaleFactor);
        var aspectRatio = source.PixelWidth > 0 ? (double)source.PixelHeight / source.PixelWidth : 1d;
        var baseWidth = InlineQrBaseWidth;
        var baseHeight = baseWidth * aspectRatio;

        var requestedWidth = Math.Max(100d, baseWidth * safeScale);
        var requestedHeight = Math.Max(100d, baseHeight * safeScale);

        var maxWidth = SystemParameters.PrimaryScreenWidth * 0.9;
        var maxHeight = SystemParameters.PrimaryScreenHeight * 0.9;
        var fitScale = Math.Min(1d, Math.Min(maxWidth / requestedWidth, maxHeight / requestedHeight));

        var owner = Application.Current?.MainWindow;
        var overlayLeft = owner?.Left ?? 0d;
        var overlayTop = owner?.Top ?? 0d;
        var overlayWidth = owner?.ActualWidth > 0 ? owner.ActualWidth : SystemParameters.PrimaryScreenWidth;
        var overlayHeight = owner?.ActualHeight > 0 ? owner.ActualHeight : SystemParameters.PrimaryScreenHeight;

        _overlayWindow = new QrPreviewOverlayWindow
        {
            Owner = owner,
            Left = overlayLeft,
            Top = overlayTop,
            Width = overlayWidth,
            Height = overlayHeight
        };

        _overlayWindow.MouseLeftButtonUp += (s, e) =>
        {
            e.Handled = true;
            Close();
        };
        _overlayWindow.Closed += (_, _) =>
        {
            _overlayWindow = null;
            if (!_isClosing && _previewWindow != null)
            {
                Close();
            }
        };

        var previewWindow = new QrPreviewWindow
        {
            Owner = owner
        };
        previewWindow.PreviewImage.Source = source;
        previewWindow.PreviewImage.Width = requestedWidth * fitScale;
        previewWindow.PreviewImage.Height = requestedHeight * fitScale;
        previewWindow.PreviewImage.MouseLeftButtonUp += (_, _) => Close();
        _previewWindow = previewWindow;

        _previewWindow.Closed += (_, _) =>
        {
            _previewWindow = null;
            if (!_isClosing && _overlayWindow != null)
            {
                Close();
            }
        };

        _overlayWindow.Show();
        _previewWindow.Show();
    }

    private static BitmapSource DecodePng(ReadOnlyMemory<byte> pngImage)
    {
        var temporaryBytes = pngImage.ToArray();
        try
        {
            using var stream = new MemoryStream(temporaryBytes, writable: false);
            var image = new BitmapImage();
            image.BeginInit();
            image.StreamSource = stream;
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.EndInit();
            image.Freeze();
            return image;
        }
        finally
        {
            CryptographicOperations.ZeroMemory(temporaryBytes);
        }
    }

    public void Close()
    {
        if (_overlayWindow == null && _previewWindow == null)
        {
            return;
        }

        _isClosing = true;

        var overlayWindow = _overlayWindow;
        var previewWindow = _previewWindow;
        _overlayWindow = null;
        _previewWindow = null;

        overlayWindow?.Close();
        previewWindow?.Close();

        _isClosing = false;

    }
}
