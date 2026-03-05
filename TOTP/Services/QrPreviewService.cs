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

    public void Toggle(BitmapSource? source)
    {
        if (_previewWindow != null || _overlayWindow != null)
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
            Padding = new Thickness(5),
            Child = previewImage
        };

        var owner = Application.Current?.MainWindow;
        var overlayLeft = owner?.Left ?? 0d;
        var overlayTop = owner?.Top ?? 0d;
        var overlayWidth = owner?.ActualWidth > 0 ? owner.ActualWidth : SystemParameters.PrimaryScreenWidth;
        var overlayHeight = owner?.ActualHeight > 0 ? owner.ActualHeight : SystemParameters.PrimaryScreenHeight;

        _overlayWindow = new Window
        {
            Owner = owner,
            ShowInTaskbar = false,
            Topmost = false,
            ShowActivated = false,
            WindowStyle = WindowStyle.None,
            ResizeMode = ResizeMode.NoResize,
            AllowsTransparency = true,
            IsHitTestVisible = true,
            Background = new SolidColorBrush(Color.FromArgb(1, 0, 0, 0)),
            WindowStartupLocation = WindowStartupLocation.Manual,
            Left = overlayLeft,
            Top = overlayTop,
            Width = overlayWidth,
            Height = overlayHeight
        };

        _overlayWindow.MouseLeftButtonUp += (_, _) => Close();
        _overlayWindow.Closed += (_, _) =>
        {
            _overlayWindow = null;
            if (!_isClosing && _previewWindow != null)
            {
                Close();
            }
        };

        _previewWindow = new Window
        {
            Owner = owner,
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
        _previewWindow.Activate();
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

        var owner = Application.Current?.MainWindow;
        if (owner != null)
        {
            owner.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (owner.WindowState == WindowState.Minimized)
                {
                    owner.WindowState = WindowState.Normal;
                }

                owner.Activate();
                owner.Focus();
            }));
        }
    }
}
