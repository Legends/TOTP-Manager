using System;
using Notification.Wpf;
using Notification.Wpf.Base;
using Notification.Wpf.Constants;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using TOTP.Core.Services.Interfaces;
using TOTP.Services.Interfaces;

namespace TOTP.Services;

public sealed class NotificationUiClient : INotificationUiClient
{
    private const string NotificationAreaName = "MainWindowNotificationArea";
    private readonly NotificationManager _notificationManager = new();

    private static readonly TextContentSettings TitleTextSettings = new()
    {
        FontFamily = new FontFamily("Segoe UI Semibold"),
        FontSize = 13,
        TextAlignment = TextAlignment.Left
    };

    private static readonly TextContentSettings MessageTextSettings = new()
    {
        FontFamily = new FontFamily("Segoe UI"),
        FontSize = 12,
        TextAlignment = TextAlignment.Left
    };

    public void Show(NotificationShowRequest request) => RunOnUiThread(() =>
    {
        ConfigureAppearance();
        var type = ToWpfType(request.Severity);
        _notificationManager.Show(
            request.Title,
            request.Message,
            type,
            NotificationAreaName,
            expirationTime: request.ExpirationTime,
            onClick: request.OnClick,
            onClose: request.OnClose,
            LeftButton: request.LeftButton,
            LeftButtonText: request.LeftButtonText,
            RightButton: request.RightButton,
            RightButtonText: request.RightButtonText,
            trim: NotificationTextTrimType.NoTrim,
            RowsCountWhenTrim: 2,
            CloseOnClick: request.CloseOnClick,
            TitleSettings: TitleTextSettings,
            MessageSettings: MessageTextSettings,
            ShowXbtn: true,
            icon: GetIcon(request.Severity));
    });

    public bool Confirm(NotificationConfirmRequest request)
    {
        var result = false;
        RunOnUiThread(() => result = ConfirmOnUiThread(request));
        return result;
    }

    private bool ConfirmOnUiThread(NotificationConfirmRequest request)
    {
        ConfigureAppearance();
        var result = false;
        var frame = new DispatcherFrame();
        var completed = false;

        void Complete(bool value)
        {
            if (completed)
            {
                return;
            }

            completed = true;
            result = value;
            frame.Continue = false;
        }

        _notificationManager.Show(
            request.Title,
            request.Message,
            ToWpfType(request.Severity),
            NotificationAreaName,
            expirationTime: TimeSpan.MaxValue,
            onClick: request.OnClick,
            onClose: () => Complete(false),
            LeftButton: () => Complete(true),
            LeftButtonText: request.OkText,
            RightButton: () => Complete(false),
            RightButtonText: request.CancelText,
            trim: NotificationTextTrimType.NoTrim,
            RowsCountWhenTrim: 2,
            CloseOnClick: false,
            TitleSettings: TitleTextSettings,
            MessageSettings: MessageTextSettings,
            ShowXbtn: true,
            icon: GetIcon(request.Severity));

        Dispatcher.PushFrame(frame);
        return result;
    }

    private static void RunOnUiThread(Action action)
    {
        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher == null || dispatcher.CheckAccess())
        {
            action();
            return;
        }

        dispatcher.Invoke(action);
    }

    private static void ConfigureAppearance()
    {
        var window = Application.Current?.MainWindow;
        var width = window?.ActualWidth > 0 ? window.ActualWidth : window?.Width ?? 300d;
        var maxWidth = Math.Floor(Math.Clamp(width - 28d, 180d, 290d));

        NotificationConstants.MaxWidth = maxWidth;
        NotificationConstants.MinWidth = Math.Floor(Math.Clamp(maxWidth - 70d, 140d, maxWidth));
        NotificationConstants.MessagePosition = Notification.Wpf.Controls.NotificationPosition.BottomLeft;
        NotificationConstants.IsReversedPanel = false;
        NotificationConstants.DefaultForegroundColor = Brushes.White;
        NotificationConstants.DefaultBackgroundColor = Brush(47, 47, 47);
        NotificationConstants.InformationBackgroundColor = Brush(37, 99, 235);
        NotificationConstants.WarningBackgroundColor = Brush(217, 163, 0);
        NotificationConstants.ErrorBackgroundColor = Brush(194, 39, 46);
        NotificationConstants.SuccessBackgroundColor = Brush(46, 125, 50);
    }

    private static SolidColorBrush Brush(byte red, byte green, byte blue) =>
        new(Color.FromArgb(204, red, green, blue));

    private static NotificationType ToWpfType(NotificationSeverity severity) => severity switch
    {
        NotificationSeverity.Information => NotificationType.Information,
        NotificationSeverity.Success => NotificationType.Success,
        NotificationSeverity.Warning => NotificationType.Warning,
        NotificationSeverity.Error => NotificationType.Error,
        _ => NotificationType.Information
    };

    private static object GetIcon(NotificationSeverity severity)
    {
        var (symbol, color) = severity switch
        {
            NotificationSeverity.Information => ("\u2139", Color.FromRgb(126, 200, 255)),
            NotificationSeverity.Warning => ("\u26A0", Color.FromRgb(246, 196, 69)),
            NotificationSeverity.Error => ("\u2716", Color.FromRgb(255, 107, 107)),
            NotificationSeverity.Success => ("\u2714", Color.FromRgb(109, 217, 159)),
            _ => ("\u2139", Color.FromRgb(126, 200, 255))
        };

        return new TextBlock
        {
            Text = symbol,
            Foreground = new SolidColorBrush(color),
            FontFamily = new FontFamily("Segoe UI Symbol"),
            FontSize = 16,
            FontWeight = FontWeights.SemiBold,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };
    }
}
