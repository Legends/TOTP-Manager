using FluentResults;
using Notification.Wpf;
using Notification.Wpf.Constants;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TOTP.Core.Common;
using TOTP.Resources;
using TOTP.Services.Interfaces;

namespace TOTP.Services;

public sealed class MessageService : IMessageService
{
    private readonly ILogFileService _logFileService;
    private readonly INotificationUiClient _notificationUiClient;
    private static int msgDuration = 5;

    public MessageService(ILogFileService logFileService, INotificationUiClient notificationUiClient)
    {
        _logFileService = logFileService;
        _notificationUiClient = notificationUiClient;
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

    public void ShowInfo(string msg, int? durationSeconds = null) => Show(UI.ui_Caption_Info, msg, NotificationType.Information, durationSeconds: durationSeconds);
    public void ShowSuccess(string msg, int? durationSeconds = null) => Show(UI.ui_Caption_Info, msg, NotificationType.Success, durationSeconds: durationSeconds);
    public void ShowWarning(string msg) => Show(UI.ui_Caption_Warning, msg, NotificationType.Warning);
    public void ShowError(string msg) => Show(
        UI.ui_Caption_Error,
        msg,
        NotificationType.Error,
        buttonText: UI.ui_btnDetails,
        buttonAction: _logFileService.OpenCurrentLogFile);

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
        int? durationSeconds = null,
        string? buttonText = null,
        Action? buttonAction = null)
    {
        RunOnUiThread(() =>
        {
            ConfigureNotificationSizing();
            ConfigureNotificationTheme();

            if (buttonAction == null || string.IsNullOrWhiteSpace(buttonText))
            {
                _notificationUiClient.Show(new NotificationShowRequest
                {
                    Title = title,
                    Message = message,
                    Type = type,
                    ExpirationTime = TimeSpan.FromSeconds(durationSeconds ?? msgDuration),
                    OnClick = type == NotificationType.Error ? _logFileService.OpenCurrentLogFile : null,
                    OnClose = null,
                    LeftButton = null,
                    LeftButtonText = null,
                    RightButton = null,
                    RightButtonText = null,
                    CloseOnClick = true,
                    Icon = GetIconForType(type)
                });

                return;
            }

            _notificationUiClient.Show(new NotificationShowRequest
            {
                Title = title,
                Message = message,
                Type = type,
                ExpirationTime = TimeSpan.FromSeconds(durationSeconds ?? msgDuration),
                OnClick = null,
                OnClose = null,
                LeftButton = buttonAction,
                LeftButtonText = buttonText,
                RightButton = null,
                RightButtonText = null,
                CloseOnClick = false,
                Icon = GetIconForType(type)
            });
        });
    }

    private bool Confirm(
        string title,
        string message,
        NotificationType type,
        string? ok,
        string? cancel)
    {
        var result = false;

        RunOnUiThread(() =>
        {
            ConfigureNotificationSizing();
            ConfigureNotificationTheme();

            result = _notificationUiClient.Confirm(new NotificationConfirmRequest
            {
                Title = title,
                Message = message,
                Type = type,
                OkText = ok ?? UI.ui_btnOK,
                CancelText = cancel ?? UI.ui_btnCancel,
                OnClick = type == NotificationType.Error ? _logFileService.OpenCurrentLogFile : null,
                Icon = GetIconForType(type)
            });
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

        var maxWidth = Math.Floor(Math.Clamp(width - 28d, 180d, 290d));
        var minWidth = Math.Floor(Math.Clamp(maxWidth - 70d, 140d, maxWidth));

        NotificationConstants.MaxWidth = maxWidth;
        NotificationConstants.MinWidth = minWidth;
        NotificationConstants.MessagePosition = Notification.Wpf.Controls.NotificationPosition.BottomLeft;
        NotificationConstants.IsReversedPanel = false;
    }

    private static void ConfigureNotificationTheme()
    {
        var defaultBackground = new SolidColorBrush(Color.FromArgb(204, 47, 47, 47));
        var infoBackground = new SolidColorBrush(Color.FromArgb(204, 37, 99, 235));
        var warningBackground = new SolidColorBrush(Color.FromArgb(204, 217, 163, 0));
        var errorBackground = new SolidColorBrush(Color.FromArgb(204, 194, 39, 46));
        var successBackground = new SolidColorBrush(Color.FromArgb(204, 46, 125, 50));

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
