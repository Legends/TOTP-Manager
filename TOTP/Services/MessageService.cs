using FluentResults;
using Notification.Wpf;
using Notification.Wpf.Constants;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using TOTP.Core.Common;
using TOTP.Infrastructure.Common;
using TOTP.Resources;
using TOTP.Services.Interfaces;

namespace TOTP.Services;

public sealed class MessageService : IMessageService
{
    private const string NotificationAreaName = "MainWindowNotificationArea";
    private readonly NotificationManager _notificationManager = new();
    private static int msgDuration = 400;

    public void ShowResultError(IResultBase result, string? context = null)
    {
        if (result.IsSuccess)
        {
            return;
        }

        var errorCode = result.GetErrorCode();
        var localizedMessage = ResultErrorLocalizer.ToUserMessage(errorCode, context);

        Show(
            UI.ui_Caption_Error,
            localizedMessage,
            NotificationType.Error,
            buttonText: UI.ui_btnDetails,
            buttonAction: OpenCurrentLogFile);
    }

    public void ShowInfo(string msg) => Show(UI.ui_Caption_Info, msg, NotificationType.Information);
    public void ShowWarning(string msg) => Show(UI.ui_Caption_Warning, msg, NotificationType.Warning);
    public void ShowError(string msg) => Show(UI.ui_Caption_Error, msg, NotificationType.Error, UI.ui_btnDetails, OpenCurrentLogFile);

    public bool ConfirmInfo(string msg, string? ok = null, string? cancel = null) =>
        Confirm(UI.ui_Caption_Info, msg, NotificationType.Information, ok, cancel);

    public bool ConfirmWarning(string msg, string? ok = null, string? cancel = null) =>
        Confirm(UI.ui_Caption_Warning, msg, NotificationType.Warning, ok, cancel);

    public bool ConfirmError(string msg, string? ok = null, string? cancel = null) =>
        Confirm(UI.ui_Caption_Error, msg, NotificationType.Error, ok, cancel);

    private void Show(
        string title,
        string message,
        NotificationType type,
        string? buttonText = null,
        Action? buttonAction = null)
    {
        RunOnUiThread(() =>
        {
            ConfigureNotificationSizing();
            ConfigureNotificationTheme();

            if (buttonAction == null || string.IsNullOrWhiteSpace(buttonText))
            {
                _notificationManager.Show(
                    title,
                    message,
                    type,
                    NotificationAreaName,
                    expirationTime: TimeSpan.FromSeconds(msgDuration),
                    onClose: null,
                    onClick: type == NotificationType.Error ? OpenCurrentLogFile : null,
                    trim: NotificationTextTrimType.NoTrim,
                    RowsCountWhenTrim: 2,
                    CloseOnClick: true,
                    TitleSettings: null,
                    MessageSettings: null,
                    ShowXbtn: true,
                    icon: null);

                return;
            }

            _notificationManager.Show(
                title,
                message,
                type,
                NotificationAreaName,
                expirationTime: TimeSpan.FromSeconds(msgDuration),
                onClick: null,
                onClose: null,
                LeftButton: buttonAction,
                LeftButtonText: buttonText,
                RightButton: null,
                RightButtonText: null,
                trim: NotificationTextTrimType.NoTrim,
                RowsCountWhenTrim: 2,
                CloseOnClick: false,
                TitleSettings: null,
                MessageSettings: null,
                ShowXbtn: true,
                icon: null);
        });
    }

    private bool Confirm(
        string title,
        string message,
        NotificationType type,
        string? ok,
        string? cancel)
    {
        bool result = false;

        RunOnUiThread(() =>
        {
            ConfigureNotificationSizing();
            ConfigureNotificationTheme();

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
                title,
                message,
                type,
                NotificationAreaName,
                expirationTime: TimeSpan.MaxValue,
                onClick: type == NotificationType.Error ? OpenCurrentLogFile : null,
                onClose: () => Complete(false),
                LeftButton: () => Complete(true),
                LeftButtonText: ok ?? UI.ui_btnOK,
                RightButton: () => Complete(false),
                RightButtonText: cancel ?? UI.ui_btnCancel,
                trim: NotificationTextTrimType.NoTrim,
                RowsCountWhenTrim: 2,
                CloseOnClick: false,
                TitleSettings: null,
                MessageSettings: null,
                ShowXbtn: true,
                icon: null);

            Dispatcher.PushFrame(frame);
        });

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

    private static void OpenCurrentLogFile()
    {
        try
        {
            var fullPath = ResolveLogFilePath();
            if (!File.Exists(fullPath))
            {
                return;
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = fullPath,
                UseShellExecute = true
            });
        }
        catch
        {
        }
    }

    private static string ResolveLogFilePath()
    {
        var currentRolling = StringsConstants.CurrentRollingAppLogFilePath;
        if (File.Exists(currentRolling))
        {
            return currentRolling;
        }

        if (Directory.Exists(StringsConstants.AppLogDirectoryPath))
        {
            var latestRolling = Directory.GetFiles(StringsConstants.AppLogDirectoryPath, "app*.log")
                .OrderByDescending(File.GetLastWriteTimeUtc)
                .FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(latestRolling))
            {
                return latestRolling;
            }
        }

        return StringsConstants.AppLogFilePath;
    }

    private static void ConfigureNotificationSizing()
    {
        var window = Application.Current?.MainWindow;
        var width = window?.ActualWidth > 0 ? window.ActualWidth : window?.Width ?? 300d;

        // Keep notifications inside the compact app window.
        var maxWidth = Math.Clamp(width - 28d, 180d, 290d);
        var minWidth = Math.Clamp(maxWidth - 70d, 140d, maxWidth);

        NotificationConstants.MaxWidth = maxWidth;
        NotificationConstants.MinWidth = minWidth;
        NotificationConstants.MessagePosition = Notification.Wpf.Controls.NotificationPosition.BottomLeft;
        NotificationConstants.IsReversedPanel = false;
    }

    private static void ConfigureNotificationTheme()
    {
        var foreground = TryGetBrush("Brush.Foreground") ?? Brushes.White;
        bool isDarkTheme = IsDarkForeground(foreground);

        NotificationConstants.DefaultForegroundColor = foreground;

        if (isDarkTheme)
        {
            NotificationConstants.DefaultBackgroundColor = new SolidColorBrush(Color.FromRgb(34, 44, 64));
            NotificationConstants.InformationBackgroundColor = new SolidColorBrush(Color.FromRgb(28, 58, 92));
            NotificationConstants.WarningBackgroundColor = new SolidColorBrush(Color.FromRgb(92, 70, 24));
            NotificationConstants.ErrorBackgroundColor = new SolidColorBrush(Colors.Red);
            NotificationConstants.SuccessBackgroundColor = new SolidColorBrush(Color.FromRgb(30, 98, 78));
            return;
        }

        NotificationConstants.DefaultBackgroundColor = new SolidColorBrush(Color.FromRgb(244, 247, 252));
        NotificationConstants.InformationBackgroundColor = new SolidColorBrush(Color.FromRgb(219, 235, 255));
        NotificationConstants.WarningBackgroundColor = new SolidColorBrush(Color.FromRgb(255, 244, 214));
        NotificationConstants.ErrorBackgroundColor = new SolidColorBrush(Color.FromRgb(255, 224, 227));
        NotificationConstants.SuccessBackgroundColor = new SolidColorBrush(Color.FromRgb(218, 247, 239));
    }

    private static Brush? TryGetBrush(string key)
    {
        return Application.Current?.TryFindResource(key) as Brush;
    }

    private static bool IsDarkForeground(Brush brush)
    {
        if (brush is not SolidColorBrush solidBrush)
        {
            return true;
        }

        var c = solidBrush.Color;
        var luminance = (0.2126 * c.R + 0.7152 * c.G + 0.0722 * c.B) / 255.0;
        return luminance >= 0.6;
    }
}
