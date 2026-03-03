using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TOTP.Core.Security.Interfaces;
using TOTP.Services.Interfaces;

namespace TOTP.Services;

public sealed class QrPreviewService : IQrPreviewService
{
    private const double InlineQrBaseWidth = 160d;
    private Window? _previewWindow;
    private readonly ISettingsService _settingsService;
    public double PreviewScaleFactor { get; set; }

    public QrPreviewService(ISettingsService settingsService)
    {
        _settingsService = settingsService;
        PreviewScaleFactor = _settingsService.Current.QrPreviewScaleFactor > 0
            ? _settingsService.Current.QrPreviewScaleFactor
            : 2.0;
    }

    public void Toggle(BitmapSource? source)
    {
        if (_previewWindow != null)
        {
            Close();
            return;
        }

        if (source == null || source.PixelWidth <= 0 || source.PixelHeight <= 0)
        {
            return;
        }

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

        var previewImage = new Image
        {
            Source = source,
            Width = requestedWidth * fitScale,
            Height = requestedHeight * fitScale,
            Stretch = Stretch.Uniform,
            SnapsToDevicePixels = true,
            UseLayoutRounding = true,
            Cursor = Cursors.Hand
        };

        previewImage.MouseLeftButtonUp += (_, _) => Close();

        var container = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(230, 20, 20, 20)),
            BorderBrush = Brushes.White,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(10),
            Child = previewImage
        };

        _previewWindow = new Window
        {
            Owner = Application.Current?.MainWindow,
            ShowInTaskbar = false,
            Topmost = true,
            WindowStyle = WindowStyle.None,
            ResizeMode = ResizeMode.NoResize,
            AllowsTransparency = true,
            Background = Brushes.Transparent,
            SizeToContent = SizeToContent.WidthAndHeight,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Content = container
        };

        _previewWindow.MouseLeftButtonUp += (_, _) => Close();
        _previewWindow.Closed += (_, _) => _previewWindow = null;
        _previewWindow.Show();
    }

    public void Close()
    {
        if (_previewWindow == null)
        {
            return;
        }

        var window = _previewWindow;
        _previewWindow = null;
        window.Close();
    }
}
