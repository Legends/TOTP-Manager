using FluentResults;
using Notification.Wpf;
using Notification.Wpf.Base;
using Notification.Wpf.Constants;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using TOTP.Core.Common;
using TOTP.Resources;
using TOTP.Services.Interfaces;

namespace TOTP.Services;

public sealed class MessageService : IMessageService
{
    private const string NotificationAreaName = "MainWindowNotificationArea";
    private readonly NotificationManager _notificationManager = new();
    private readonly ILogFileService _logFileService;
    private static int msgDuration = 4;
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

    public MessageService(ILogFileService logFileService)
    {
        _logFileService = logFileService;
    }

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
            buttonAction: _logFileService.OpenCurrentLogFile);
    }

    public void ShowInfo(string msg) => Show(UI.ui_Caption_Info, msg, NotificationType.Information);
    public void ShowWarning(string msg) => Show(UI.ui_Caption_Warning, msg, NotificationType.Warning);
    public void ShowError(string msg) => Show(UI.ui_Caption_Error, msg, NotificationType.Error, UI.ui_btnDetails, _logFileService.OpenCurrentLogFile);

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
                    onClick: type == NotificationType.Error ? _logFileService.OpenCurrentLogFile : null,
                    trim: NotificationTextTrimType.NoTrim,
                    RowsCountWhenTrim: 2,
                    CloseOnClick: true,
                    TitleSettings: TitleTextSettings,
                    MessageSettings: MessageTextSettings,
                    ShowXbtn: true,
                    icon: GetIconForType(type));

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
                TitleSettings: TitleTextSettings,
                MessageSettings: MessageTextSettings,
                ShowXbtn: true,
                icon: GetIconForType(type));
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
                onClick: type == NotificationType.Error ? _logFileService.OpenCurrentLogFile : null,
                onClose: () => Complete(false),
                LeftButton: () => Complete(true),
                LeftButtonText: ok ?? UI.ui_btnOK,
                RightButton: () => Complete(false),
                RightButtonText: cancel ?? UI.ui_btnCancel,
                trim: NotificationTextTrimType.NoTrim,
                RowsCountWhenTrim: 2,
                CloseOnClick: false,
                TitleSettings: TitleTextSettings,
                MessageSettings: MessageTextSettings,
                ShowXbtn: true,
                icon: GetIconForType(type));

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

    private static void ConfigureNotificationSizing()
    {
        var window = Application.Current?.MainWindow;
        var width = window?.ActualWidth > 0 ? window.ActualWidth : window?.Width ?? 300d;

        // Keep notifications inside the compact app window.
        var maxWidth = Math.Floor(Math.Clamp(width - 28d, 180d, 290d));
        var minWidth = Math.Floor(Math.Clamp(maxWidth - 70d, 140d, maxWidth));

        NotificationConstants.MaxWidth = maxWidth;
        NotificationConstants.MinWidth = minWidth;
        NotificationConstants.MessagePosition = Notification.Wpf.Controls.NotificationPosition.BottomLeft;
        NotificationConstants.IsReversedPanel = false;
    }

    private static void ConfigureNotificationTheme()
    {
        var defaultBackground = new SolidColorBrush(Color.FromArgb(204, 47, 47, 47)); // #2F2F2F @ 0.8
        var infoBackground = new SolidColorBrush(Color.FromArgb(204, 37, 99, 235));    // blue
        var warningBackground = new SolidColorBrush(Color.FromArgb(204, 217, 163, 0)); // yellow
        var errorBackground = new SolidColorBrush(Color.FromArgb(204, 194, 39, 46));   // red
        var successBackground = new SolidColorBrush(Color.FromArgb(204, 46, 125, 50)); // green

        NotificationConstants.DefaultForegroundColor = Brushes.White;
        NotificationConstants.DefaultBackgroundColor = defaultBackground;
        NotificationConstants.InformationBackgroundColor = infoBackground;
        NotificationConstants.WarningBackgroundColor = warningBackground;
        NotificationConstants.ErrorBackgroundColor = errorBackground;
        NotificationConstants.SuccessBackgroundColor = successBackground;
    }

    private static object? GetIconForType(NotificationType type)
    {
        return type switch
        {
            NotificationType.Information => CreateStatusIcon("\u2139", Color.FromRgb(126, 200, 255)),
            NotificationType.Warning => CreateStatusIcon("\u26A0", Color.FromRgb(246, 196, 69)),
            NotificationType.Error => CreateStatusIcon("\u2716", Color.FromRgb(255, 107, 107)),
            NotificationType.Success => CreateStatusIcon("\u2714", Color.FromRgb(109, 217, 159)),
            _ => null
        };
    }

    private static UIElement CreateStatusIcon(string symbol, Color color)
    {
        var iconBrush = new SolidColorBrush(color);
        if (iconBrush.CanFreeze)
        {
            iconBrush.Freeze();
        }

        return new TextBlock
        {
            Text = symbol,
            Foreground = iconBrush,
            FontFamily = new FontFamily("Segoe UI Symbol"),
            FontSize = 16,
            FontWeight = FontWeights.SemiBold,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0)
        };
    }

}
